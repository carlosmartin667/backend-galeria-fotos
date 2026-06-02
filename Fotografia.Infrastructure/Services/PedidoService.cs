using AutoMapper;
using Fotografia.Infrastructure.Data;
using Fotografia.Application.DTOs.Common;
using Fotografia.Application.DTOs.Notificaciones;
using Fotografia.Application.DTOs.Pedidos;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fotografia.Infrastructure.Services;

public sealed class PedidoService(
    AppDbContext dbContext,
    IMapper mapper,
    ICurrentUserService currentUser,
    INotificacionService notificacionService,
    ILogger<PedidoService> logger) : IPedidoService
{
    public async Task<ApiResponse<IReadOnlyCollection<PedidoResponseDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = QueryPedidos().AsNoTracking();

        if (!currentUser.IsAdmin)
        {
            if (currentUser.UserId is null)
            {
                return ApiResponse<IReadOnlyCollection<PedidoResponseDto>>.Forbidden("Debe iniciar sesion.");
            }

            query = query.Where(x => x.Cliente != null && x.Cliente.UsuarioId == currentUser.UserId.Value);
        }

        var pedidos = await query.OrderByDescending(x => x.CreadoEnUtc).ToListAsync(cancellationToken);

        var response = mapper.Map<List<PedidoResponseDto>>(pedidos);
        SanitizeStorageKeysForNonAdmin(response);

        return ApiResponse<IReadOnlyCollection<PedidoResponseDto>>.Ok(response);
    }

    public async Task<ApiResponse<PaginatedResponseDto<PedidoResponseDto>>> GetPaginatedAsync(
        PaginationQueryDto pagination,
        CancellationToken cancellationToken = default)
    {
        var validationError = pagination.Validate();
        if (validationError is not null)
        {
            return ApiResponse<PaginatedResponseDto<PedidoResponseDto>>.Fail(validationError);
        }

        var query = QueryPedidos().AsNoTracking();

        if (!currentUser.IsAdmin)
        {
            if (currentUser.UserId is null)
            {
                return ApiResponse<PaginatedResponseDto<PedidoResponseDto>>.Forbidden("Debe iniciar sesion.");
            }

            query = query.Where(x => x.Cliente != null && x.Cliente.UsuarioId == currentUser.UserId.Value);
        }

        var paginated = await query
            .OrderByDescending(x => x.CreadoEnUtc)
            .ToPaginatedResponseAsync(pagination, cancellationToken);

        var items = mapper.Map<List<PedidoResponseDto>>(paginated.Items);
        SanitizeStorageKeysForNonAdmin(items);

        return ApiResponse<PaginatedResponseDto<PedidoResponseDto>>.Ok(new PaginatedResponseDto<PedidoResponseDto>
        {
            Items = items,
            Page = paginated.Page,
            PageSize = paginated.PageSize,
            TotalItems = paginated.TotalItems,
            TotalPages = paginated.TotalPages,
            HasPreviousPage = paginated.HasPreviousPage,
            HasNextPage = paginated.HasNextPage,
            All = paginated.All
        });
    }

    public async Task<ApiResponse<PedidoResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var pedido = await QueryPedidos()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (pedido is null)
        {
            return ApiResponse<PedidoResponseDto>.NotFound("Pedido no encontrado.");
        }

        if (!CanAccess(pedido))
        {
            return ApiResponse<PedidoResponseDto>.Forbidden("No puede consultar pedidos de otro usuario.");
        }

        var response = mapper.Map<PedidoResponseDto>(pedido);
        SanitizeStorageKeysForNonAdmin(response);

        return ApiResponse<PedidoResponseDto>.Ok(response);
    }

    public async Task<ApiResponse<PedidoResponseDto>> CreateAsync(CrearPedidoRequestDto request, CancellationToken cancellationToken = default)
    {
        var cliente = await dbContext.Clientes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ClienteId, cancellationToken);
        if (cliente is null)
        {
            return ApiResponse<PedidoResponseDto>.NotFound("Cliente no encontrado.");
        }

        if (!currentUser.IsAdmin && cliente.UsuarioId != currentUser.UserId)
        {
            return ApiResponse<PedidoResponseDto>.Forbidden("No puede crear pedidos para otro cliente.");
        }

        var eventoExists = await dbContext.Eventos.AnyAsync(x => x.Id == request.EventoId, cancellationToken);
        if (!eventoExists)
        {
            return ApiResponse<PedidoResponseDto>.NotFound("Evento no encontrado.");
        }

        var fotoIds = request.FotoIds.Distinct().ToList();
        var fotos = await dbContext.Fotos
            .Where(x => fotoIds.Contains(x.Id) && x.EventoId == request.EventoId && x.Activa)
            .ToListAsync(cancellationToken);

        if (fotos.Count != fotoIds.Count)
        {
            return ApiResponse<PedidoResponseDto>.Fail("Una o mas fotos no existen, no pertenecen al evento o no estan activas.");
        }

        var subtotal = fotos.Sum(x => x.PrecioUnitario);
        var pedido = new Pedido
        {
            EventoId = request.EventoId,
            ClienteId = request.ClienteId,
            Subtotal = subtotal,
            DescuentoTotal = 0,
            Total = subtotal,
            PedidoFotos = fotos.Select(foto => new PedidoFoto
            {
                FotoId = foto.Id,
                PrecioUnitario = foto.PrecioUnitario,
                Cantidad = 1
            }).ToList(),
            PedidoItems = fotos.Select(foto => new PedidoItem
            {
                TipoItem = PedidoItemTipos.FotoEvento,
                FotoId = foto.Id,
                Descripcion = foto.NombreArchivo,
                PrecioUnitario = foto.PrecioUnitario,
                Cantidad = 1,
                Subtotal = foto.PrecioUnitario,
                FechaCreacionUtc = DateTime.UtcNow
            }).ToList()
        };

        dbContext.Pedidos.Add(pedido);
        await dbContext.SaveChangesAsync(cancellationToken);

        await TryEnqueuePedidoCreadoAsync(pedido, cliente, cancellationToken);

        var created = await QueryPedidos()
            .AsNoTracking()
            .FirstAsync(x => x.Id == pedido.Id, cancellationToken);

        var response = mapper.Map<PedidoResponseDto>(created);
        SanitizeStorageKeysForNonAdmin(response);

        return ApiResponse<PedidoResponseDto>.Ok(response, "Pedido creado.");
    }

    public async Task<ApiResponse<PedidoResponseDto>> CambiarEstadoAsync(
        Guid id,
        CambiarEstadoPedidoRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<PedidoResponseDto>.Forbidden("Solo un administrador puede cambiar el estado de pedidos.");
        }

        if (!PedidoEstados.TryNormalize(request.Estado, out var estadoNuevo))
        {
            return ApiResponse<PedidoResponseDto>.Fail("Estado de pedido invalido.");
        }

        var comentario = Normalize(request.Comentario);
        if (estadoNuevo == PedidoEstados.Reembolsado && string.IsNullOrWhiteSpace(comentario))
        {
            return ApiResponse<PedidoResponseDto>.Fail("Reembolsado requiere comentario.");
        }

        var pedido = await QueryPedidos().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (pedido is null)
        {
            return ApiResponse<PedidoResponseDto>.NotFound("Pedido no encontrado.");
        }

        if (PedidoEstados.EsFinal(pedido.Estado)
            && !PedidoEstados.EsFinal(estadoNuevo)
            && string.IsNullOrWhiteSpace(comentario))
        {
            return ApiResponse<PedidoResponseDto>.Fail("Cambiar un pedido finalizado requiere comentario.");
        }

        var estadoAnterior = pedido.Estado;
        if (string.Equals(estadoAnterior, estadoNuevo, StringComparison.OrdinalIgnoreCase))
        {
            var sameResponse = mapper.Map<PedidoResponseDto>(pedido);
            return ApiResponse<PedidoResponseDto>.Ok(sameResponse, "El pedido ya tenia ese estado.");
        }

        pedido.Estado = estadoNuevo;
        pedido.ActualizadoEnUtc = DateTime.UtcNow;
        dbContext.PedidoEstadoHistorial.Add(new PedidoEstadoHistorial
        {
            PedidoId = pedido.Id,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = estadoNuevo,
            Comentario = comentario,
            UsuarioId = currentUser.UserId,
            FechaCambioUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        if (estadoNuevo == PedidoEstados.ListoParaDescargar)
        {
            await TryEnqueuePedidoListoDescargaAsync(pedido, cancellationToken);
        }

        var updated = await QueryPedidos()
            .AsNoTracking()
            .FirstAsync(x => x.Id == pedido.Id, cancellationToken);

        var response = mapper.Map<PedidoResponseDto>(updated);
        SanitizeStorageKeysForNonAdmin(response);

        return ApiResponse<PedidoResponseDto>.Ok(response, "Estado de pedido actualizado.");
    }

    public async Task<ApiResponse<IReadOnlyCollection<PedidoEstadoHistorialResponseDto>>> GetHistorialEstadosAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var pedido = await dbContext.Pedidos
            .AsNoTracking()
            .Include(x => x.Cliente)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (pedido is null)
        {
            return ApiResponse<IReadOnlyCollection<PedidoEstadoHistorialResponseDto>>.NotFound("Pedido no encontrado.");
        }

        if (!CanAccess(pedido))
        {
            return ApiResponse<IReadOnlyCollection<PedidoEstadoHistorialResponseDto>>.Forbidden("No puede consultar pedidos de otro usuario.");
        }

        var historial = await dbContext.PedidoEstadoHistorial
            .AsNoTracking()
            .Include(x => x.Usuario)
            .Where(x => x.PedidoId == id)
            .OrderByDescending(x => x.FechaCambioUtc)
            .Select(x => new PedidoEstadoHistorialResponseDto
            {
                Id = x.Id,
                PedidoId = x.PedidoId,
                EstadoAnterior = x.EstadoAnterior,
                EstadoNuevo = x.EstadoNuevo,
                Comentario = x.Comentario,
                UsuarioId = x.UsuarioId,
                UsuarioNombre = x.Usuario == null ? null : x.Usuario.Nombre,
                FechaCambioUtc = x.FechaCambioUtc
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<PedidoEstadoHistorialResponseDto>>.Ok(historial);
    }

    private IQueryable<Pedido> QueryPedidos()
    {
        return dbContext.Pedidos
            .Include(x => x.Cliente)
            .Include(x => x.PedidoFotos)
            .ThenInclude(x => x.Foto)
            .Include(x => x.PedidoItems)
            .ThenInclude(x => x.Foto)
            .Include(x => x.PedidoItems)
            .ThenInclude(x => x.PaqueteEvento)
            .Include(x => x.PedidoItems)
            .ThenInclude(x => x.FotoPrivada);
    }

    private bool CanAccess(Pedido pedido)
    {
        return currentUser.IsAdmin || pedido.Cliente?.UsuarioId == currentUser.UserId;
    }

    private void SanitizeStorageKeysForNonAdmin(IEnumerable<PedidoResponseDto> pedidos)
    {
        if (currentUser.IsAdmin)
        {
            return;
        }

        foreach (var pedido in pedidos)
        {
            SanitizeStorageKeysForNonAdmin(pedido);
        }
    }

    private void SanitizeStorageKeysForNonAdmin(PedidoResponseDto pedido)
    {
        if (currentUser.IsAdmin)
        {
            return;
        }

        foreach (var item in pedido.Fotos)
        {
            if (item.Foto is not null)
            {
                item.Foto.StorageKey = string.Empty;
            }
        }
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private async Task TryEnqueuePedidoCreadoAsync(
        Pedido pedido,
        Cliente cliente,
        CancellationToken cancellationToken)
    {
        var replacements = CreatePedidoReplacements(pedido, cliente);
        await TryEnqueueTemplateAsync(new EnqueueTemplateNotificacionRequestDto
        {
            Codigo = NotificacionTipos.PedidoCreadoAdmin,
            EntidadTipo = "Pedido",
            EntidadId = pedido.Id,
            CorrelationKey = $"pedido:{pedido.Id}:creado:admin",
            Reemplazos = replacements
        }, cancellationToken);

        await TryEnqueueTemplateAsync(new EnqueueTemplateNotificacionRequestDto
        {
            Codigo = NotificacionTipos.PedidoCreadoCliente,
            DestinatarioEmail = cliente.Email,
            UsuarioId = cliente.UsuarioId,
            EntidadTipo = "Pedido",
            EntidadId = pedido.Id,
            CorrelationKey = $"pedido:{pedido.Id}:creado:cliente",
            Reemplazos = replacements
        }, cancellationToken);
    }

    private async Task TryEnqueuePedidoListoDescargaAsync(
        Pedido pedido,
        CancellationToken cancellationToken)
    {
        var cliente = pedido.Cliente;
        if (cliente is null)
        {
            cliente = await dbContext.Clientes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == pedido.ClienteId, cancellationToken);
        }

        if (cliente is null)
        {
            return;
        }

        await TryEnqueueTemplateAsync(new EnqueueTemplateNotificacionRequestDto
        {
            Codigo = NotificacionTipos.PedidoListoDescargaCliente,
            DestinatarioEmail = cliente.Email,
            UsuarioId = cliente.UsuarioId,
            EntidadTipo = "Pedido",
            EntidadId = pedido.Id,
            CorrelationKey = $"pedido:{pedido.Id}:listo-descarga:cliente",
            Reemplazos = CreatePedidoReplacements(pedido, cliente)
        }, cancellationToken);
    }

    private async Task TryEnqueueTemplateAsync(
        EnqueueTemplateNotificacionRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await notificacionService.EnqueueFromTemplateAsync(request, cancellationToken);
            if (!result.Success)
            {
                logger.LogWarning(
                    "No se pudo encolar notificacion de pedido. Codigo={Codigo} EntidadId={EntidadId} Motivo={Motivo}",
                    request.Codigo,
                    request.EntidadId,
                    result.Message);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(
                ex,
                "No se pudo encolar notificacion de pedido. Codigo={Codigo} EntidadId={EntidadId}",
                request.Codigo,
                request.EntidadId);
        }
    }

    private static Dictionary<string, string?> CreatePedidoReplacements(Pedido pedido, Cliente cliente)
    {
        return new Dictionary<string, string?>
        {
            ["NombreCliente"] = cliente.Nombre,
            ["EmailCliente"] = cliente.Email,
            ["NombreEvento"] = pedido.Evento?.Nombre,
            ["PedidoId"] = pedido.Id.ToString(),
            ["Total"] = pedido.Total.ToString("0.##"),
            ["Estado"] = pedido.Estado,
            ["Link"] = "Disponible en tu cuenta",
            ["NombreFotografa"] = "Fotografa",
            ["Fecha"] = pedido.CreadoEnUtc.ToString("yyyy-MM-dd")
        };
    }
}
