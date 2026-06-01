using AutoMapper;
using Fotografia.Infrastructure.Data;
using Fotografia.Application.DTOs.Common;
using Fotografia.Application.DTOs.Pedidos;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Services;

public sealed class PedidoService(AppDbContext dbContext, IMapper mapper, ICurrentUserService currentUser) : IPedidoService
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

        return ApiResponse<IReadOnlyCollection<PedidoResponseDto>>.Ok(mapper.Map<List<PedidoResponseDto>>(pedidos));
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

        return ApiResponse<PaginatedResponseDto<PedidoResponseDto>>.Ok(new PaginatedResponseDto<PedidoResponseDto>
        {
            Items = mapper.Map<List<PedidoResponseDto>>(paginated.Items),
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

        return ApiResponse<PedidoResponseDto>.Ok(mapper.Map<PedidoResponseDto>(pedido));
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

        var pedido = new Pedido
        {
            EventoId = request.EventoId,
            ClienteId = request.ClienteId,
            Total = fotos.Sum(x => x.PrecioUnitario),
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

        var created = await QueryPedidos()
            .AsNoTracking()
            .FirstAsync(x => x.Id == pedido.Id, cancellationToken);

        return ApiResponse<PedidoResponseDto>.Ok(mapper.Map<PedidoResponseDto>(created), "Pedido creado.");
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
}
