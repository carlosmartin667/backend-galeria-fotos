using AutoMapper;
using Fotografia.Application.DTOs.Carrito;
using Fotografia.Application.DTOs.Notificaciones;
using Fotografia.Application.DTOs.Pedidos;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fotografia.Infrastructure.Services;

public sealed class CarritoService(
    AppDbContext dbContext,
    IMapper mapper,
    ICurrentUserService currentUser,
    INotificacionService notificacionService,
    ILogger<CarritoService> logger) : ICarritoService
{
    public async Task<ApiResponse<CarritoResponseDto>> GetActivoAsync(CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return ApiResponse<CarritoResponseDto>.Forbidden("Debe iniciar sesion para usar el carrito.");
        }

        var carrito = await GetOrCreateActiveCartAsync(currentUser.UserId.Value, cancellationToken);
        return ApiResponse<CarritoResponseDto>.Ok(MapCarrito(carrito));
    }

    public async Task<ApiResponse<CarritoResponseDto>> AddFotoEventoAsync(
        Guid fotoId,
        CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return ApiResponse<CarritoResponseDto>.Forbidden("Debe iniciar sesion para usar el carrito.");
        }

        var foto = await dbContext.Fotos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == fotoId && x.Activa, cancellationToken);
        if (foto is null)
        {
            return ApiResponse<CarritoResponseDto>.NotFound("Foto no encontrada o inactiva.");
        }

        var carrito = await GetOrCreateActiveCartAsync(currentUser.UserId.Value, cancellationToken);
        if (!carrito.Items.Any(x => x.TipoItem == PedidoItemTipos.FotoEvento && x.FotoId == fotoId))
        {
            carrito.Items.Add(new CarritoItem
            {
                TipoItem = PedidoItemTipos.FotoEvento,
                FotoId = foto.Id,
                Cantidad = 1,
                PrecioUnitario = foto.PrecioUnitario,
                Descripcion = foto.NombreArchivo,
                FechaCreacionUtc = DateTime.UtcNow
            });
            carrito.FechaActualizacionUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return ApiResponse<CarritoResponseDto>.Ok(MapCarrito(carrito), "Foto agregada al carrito.");
    }

    public async Task<ApiResponse<CarritoResponseDto>> AddPaqueteEventoAsync(
        Guid paqueteId,
        CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return ApiResponse<CarritoResponseDto>.Forbidden("Debe iniciar sesion para usar el carrito.");
        }

        var paquete = await dbContext.PaquetesEvento
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == paqueteId && x.Activo, cancellationToken);
        if (paquete is null)
        {
            return ApiResponse<CarritoResponseDto>.NotFound("Paquete no encontrado o inactivo.");
        }

        var carrito = await GetOrCreateActiveCartAsync(currentUser.UserId.Value, cancellationToken);
        if (!carrito.Items.Any(x => x.TipoItem == PedidoItemTipos.PaqueteEvento && x.PaqueteEventoId == paqueteId))
        {
            carrito.Items.Add(new CarritoItem
            {
                TipoItem = PedidoItemTipos.PaqueteEvento,
                PaqueteEventoId = paquete.Id,
                Cantidad = 1,
                PrecioUnitario = paquete.Precio,
                Descripcion = paquete.Nombre,
                FechaCreacionUtc = DateTime.UtcNow
            });
            carrito.FechaActualizacionUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return ApiResponse<CarritoResponseDto>.Ok(MapCarrito(carrito), "Paquete agregado al carrito.");
    }

    public async Task<ApiResponse<CarritoResponseDto>> AddFotoPrivadaAsync(
        Guid fotoPrivadaId,
        CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return ApiResponse<CarritoResponseDto>.Forbidden("Debe iniciar sesion para usar el carrito.");
        }

        var foto = await dbContext.FotosPrivadas
            .AsNoTracking()
            .Include(x => x.Cliente)
            .FirstOrDefaultAsync(x => x.Id == fotoPrivadaId && x.Activa, cancellationToken);
        if (foto is null)
        {
            return ApiResponse<CarritoResponseDto>.NotFound("Foto privada no encontrada o inactiva.");
        }

        if (!currentUser.IsAdmin && foto.Cliente?.UsuarioId != currentUser.UserId.Value)
        {
            return ApiResponse<CarritoResponseDto>.Forbidden("No puede comprar fotos privadas de otro usuario.");
        }

        var carrito = await GetOrCreateActiveCartAsync(currentUser.UserId.Value, cancellationToken);
        if (!carrito.Items.Any(x => x.TipoItem == PedidoItemTipos.FotoPrivada && x.FotoPrivadaId == fotoPrivadaId))
        {
            carrito.Items.Add(new CarritoItem
            {
                TipoItem = PedidoItemTipos.FotoPrivada,
                FotoPrivadaId = foto.Id,
                Cantidad = 1,
                PrecioUnitario = foto.PrecioUnitario,
                Descripcion = foto.NombreArchivo,
                FechaCreacionUtc = DateTime.UtcNow
            });
            carrito.FechaActualizacionUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return ApiResponse<CarritoResponseDto>.Ok(MapCarrito(carrito), "Foto privada agregada al carrito.");
    }

    public async Task<ApiResponse<bool>> DeleteItemAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return ApiResponse<bool>.Forbidden("Debe iniciar sesion para usar el carrito.");
        }

        var item = await dbContext.CarritoItems
            .Include(x => x.CarritoCompra)
            .FirstOrDefaultAsync(x =>
                x.Id == itemId
                && x.CarritoCompra != null
                && x.CarritoCompra.UsuarioId == currentUser.UserId.Value
                && x.CarritoCompra.Estado == CarritoEstados.Activo,
                cancellationToken);

        if (item is null)
        {
            return ApiResponse<bool>.NotFound("Item de carrito no encontrado.");
        }

        dbContext.CarritoItems.Remove(item);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Item eliminado del carrito.");
    }

    public async Task<ApiResponse<bool>> VaciarAsync(CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return ApiResponse<bool>.Forbidden("Debe iniciar sesion para usar el carrito.");
        }

        var carrito = await QueryActiveCart(currentUser.UserId.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (carrito is null)
        {
            return ApiResponse<bool>.Ok(true, "Carrito vacio.");
        }

        dbContext.CarritoItems.RemoveRange(carrito.Items);
        carrito.FechaActualizacionUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Carrito vaciado.");
    }

    public async Task<ApiResponse<PedidoResponseDto>> CrearPedidoAsync(CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return ApiResponse<PedidoResponseDto>.Forbidden("Debe iniciar sesion para crear un pedido.");
        }

        var cliente = await dbContext.Clientes
            .FirstOrDefaultAsync(x => x.UsuarioId == currentUser.UserId.Value, cancellationToken);
        if (cliente is null)
        {
            return ApiResponse<PedidoResponseDto>.Fail("El usuario no tiene un cliente asociado.");
        }

        var carrito = await QueryActiveCart(currentUser.UserId.Value)
            .FirstOrDefaultAsync(cancellationToken);
        if (carrito is null || carrito.Items.Count == 0)
        {
            return ApiResponse<PedidoResponseDto>.Fail("El carrito esta vacio.");
        }

        var pedidoItems = new List<PedidoItem>();
        var eventIds = new HashSet<Guid>();
        var hasPrivateItems = false;

        foreach (var item in carrito.Items)
        {
            switch (item.TipoItem)
            {
                case PedidoItemTipos.FotoEvento:
                {
                    if (item.FotoId is null)
                    {
                        return ApiResponse<PedidoResponseDto>.Fail("El carrito contiene una foto de evento invalida.");
                    }

                    var foto = await dbContext.Fotos
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == item.FotoId.Value && x.Activa, cancellationToken);
                    if (foto is null)
                    {
                        return ApiResponse<PedidoResponseDto>.Fail("Una foto del carrito ya no esta disponible.");
                    }

                    eventIds.Add(foto.EventoId);
                    pedidoItems.Add(CreatePedidoItem(
                        PedidoItemTipos.FotoEvento,
                        foto.NombreArchivo,
                        foto.PrecioUnitario,
                        fotoId: foto.Id));
                    break;
                }
                case PedidoItemTipos.PaqueteEvento:
                {
                    if (item.PaqueteEventoId is null)
                    {
                        return ApiResponse<PedidoResponseDto>.Fail("El carrito contiene un paquete invalido.");
                    }

                    var paquete = await dbContext.PaquetesEvento
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == item.PaqueteEventoId.Value && x.Activo, cancellationToken);
                    if (paquete is null)
                    {
                        return ApiResponse<PedidoResponseDto>.Fail("Un paquete del carrito ya no esta disponible.");
                    }

                    eventIds.Add(paquete.EventoId);
                    pedidoItems.Add(CreatePedidoItem(
                        PedidoItemTipos.PaqueteEvento,
                        paquete.Nombre,
                        paquete.Precio,
                        paqueteEventoId: paquete.Id));
                    break;
                }
                case PedidoItemTipos.FotoPrivada:
                {
                    if (item.FotoPrivadaId is null)
                    {
                        return ApiResponse<PedidoResponseDto>.Fail("El carrito contiene una foto privada invalida.");
                    }

                    var fotoPrivada = await dbContext.FotosPrivadas
                        .AsNoTracking()
                        .Include(x => x.Cliente)
                        .FirstOrDefaultAsync(x => x.Id == item.FotoPrivadaId.Value && x.Activa, cancellationToken);
                    if (fotoPrivada is null)
                    {
                        return ApiResponse<PedidoResponseDto>.Fail("Una foto privada del carrito ya no esta disponible.");
                    }

                    if (!currentUser.IsAdmin && fotoPrivada.Cliente?.UsuarioId != currentUser.UserId.Value)
                    {
                        return ApiResponse<PedidoResponseDto>.Forbidden("No puede comprar fotos privadas de otro usuario.");
                    }

                    hasPrivateItems = true;
                    pedidoItems.Add(CreatePedidoItem(
                        PedidoItemTipos.FotoPrivada,
                        fotoPrivada.NombreArchivo,
                        fotoPrivada.PrecioUnitario,
                        fotoPrivadaId: fotoPrivada.Id));
                    break;
                }
                default:
                    return ApiResponse<PedidoResponseDto>.Fail("El carrito contiene un tipo de item invalido.");
            }
        }

        var pedido = new Pedido
        {
            ClienteId = cliente.Id,
            EventoId = !hasPrivateItems && eventIds.Count == 1 ? eventIds.Single() : null,
            Estado = PedidoEstados.Pendiente,
            Moneda = "ARS",
            CreadoEnUtc = DateTime.UtcNow,
            PedidoItems = pedidoItems,
            Total = pedidoItems.Sum(x => x.Subtotal)
        };

        dbContext.Pedidos.Add(pedido);
        carrito.Estado = CarritoEstados.Convertido;
        carrito.FechaActualizacionUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        await TryEnqueuePedidoCreadoAsync(pedido, cliente, cancellationToken);

        var created = await QueryPedido(pedido.Id).FirstAsync(cancellationToken);

        return ApiResponse<PedidoResponseDto>.Ok(
            mapper.Map<PedidoResponseDto>(created),
            "Pedido creado desde carrito.");
    }

    private async Task<CarritoCompra> GetOrCreateActiveCartAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        var carrito = await QueryActiveCart(usuarioId).FirstOrDefaultAsync(cancellationToken);
        if (carrito is not null)
        {
            return carrito;
        }

        carrito = new CarritoCompra
        {
            UsuarioId = usuarioId,
            Estado = CarritoEstados.Activo,
            FechaCreacionUtc = DateTime.UtcNow
        };

        dbContext.CarritosCompra.Add(carrito);
        await dbContext.SaveChangesAsync(cancellationToken);

        return carrito;
    }

    private IQueryable<CarritoCompra> QueryActiveCart(Guid usuarioId)
    {
        return dbContext.CarritosCompra
            .Include(x => x.Items)
            .Where(x => x.UsuarioId == usuarioId && x.Estado == CarritoEstados.Activo);
    }

    private IQueryable<Pedido> QueryPedido(Guid pedidoId)
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
            .ThenInclude(x => x.FotoPrivada)
            .Where(x => x.Id == pedidoId);
    }

    private CarritoResponseDto MapCarrito(CarritoCompra carrito)
    {
        carrito.Items = carrito.Items
            .OrderBy(x => x.FechaCreacionUtc)
            .ToList();

        return mapper.Map<CarritoResponseDto>(carrito);
    }

    private static PedidoItem CreatePedidoItem(
        string tipoItem,
        string descripcion,
        decimal precioUnitario,
        Guid? fotoId = null,
        Guid? paqueteEventoId = null,
        Guid? fotoPrivadaId = null)
    {
        return new PedidoItem
        {
            TipoItem = tipoItem,
            Descripcion = descripcion,
            PrecioUnitario = precioUnitario,
            Cantidad = 1,
            Subtotal = precioUnitario,
            FotoId = fotoId,
            PaqueteEventoId = paqueteEventoId,
            FotoPrivadaId = fotoPrivadaId,
            FechaCreacionUtc = DateTime.UtcNow
        };
    }

    private async Task TryEnqueuePedidoCreadoAsync(
        Pedido pedido,
        Cliente cliente,
        CancellationToken cancellationToken)
    {
        var replacements = new Dictionary<string, string?>
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
                    "No se pudo encolar notificacion de carrito. Codigo={Codigo} EntidadId={EntidadId} Motivo={Motivo}",
                    request.Codigo,
                    request.EntidadId,
                    result.Message);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(
                ex,
                "No se pudo encolar notificacion de carrito. Codigo={Codigo} EntidadId={EntidadId}",
                request.Codigo,
                request.EntidadId);
        }
    }
}
