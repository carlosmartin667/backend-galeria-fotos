using Fotografia.Application.DTOs.Admin;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Services;

public sealed class AdminVentasService(
    AppDbContext dbContext,
    ICurrentUserService currentUser) : IAdminVentasService
{
    public async Task<ApiResponse<AdminVentasResumenDto>> GetResumenAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<AdminVentasResumenDto>.Forbidden("Solo un administrador puede consultar ventas.");
        }

        var now = DateTime.UtcNow;
        var inicioMes = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var inicioMesAnterior = inicioMes.AddMonths(-1);

        var pedidos = await dbContext.Pedidos
            .AsNoTracking()
            .Include(x => x.Pago)
            .Include(x => x.PedidoItems)
            .ThenInclude(x => x.Foto)
            .Include(x => x.PedidoItems)
            .ThenInclude(x => x.PaqueteEvento)
            .Include(x => x.PedidoItems)
            .ThenInclude(x => x.FotoPrivada)
            .Where(x => x.CreadoEnUtc >= inicioMesAnterior)
            .ToListAsync(cancellationToken);

        var pedidosPagados = pedidos
            .Where(x => PedidoEstados.EsPagado(x.Estado, x.Pago?.Estado))
            .ToList();

        var ventasMes = pedidosPagados
            .Where(x => x.CreadoEnUtc >= inicioMes)
            .Sum(x => x.Total);

        var ventasMesAnterior = pedidosPagados
            .Where(x => x.CreadoEnUtc >= inicioMesAnterior && x.CreadoEnUtc < inicioMes)
            .Sum(x => x.Total);

        var response = new AdminVentasResumenDto
        {
            VentasMes = ventasMes,
            VentasMesAnterior = ventasMesAnterior,
            CrecimientoPorcentual = CalculateGrowth(ventasMesAnterior, ventasMes),
            CarritosAbandonados = await dbContext.CarritoAbandonadoRegistros
                .AsNoTracking()
                .CountAsync(x => x.Activo
                    && (x.Estado == CarritoAbandonadoEstados.Detectado || x.Estado == CarritoAbandonadoEstados.Notificado),
                    cancellationToken),
            CarritosRecuperados = await dbContext.CarritoAbandonadoRegistros
                .AsNoTracking()
                .CountAsync(x => x.Estado == CarritoAbandonadoEstados.Recuperado, cancellationToken),
            CuponesActivos = await dbContext.CuponesDescuento
                .AsNoTracking()
                .CountAsync(x => x.Activo, cancellationToken),
            PromocionesActivas = await dbContext.Promociones
                .AsNoTracking()
                .CountAsync(x => x.Activa, cancellationToken),
            TestimoniosPendientes = await dbContext.Testimonios
                .AsNoTracking()
                .CountAsync(x => x.Activo && !x.Publicado, cancellationToken),
            ProductosMasVendidos = BuildProductos(pedidosPagados),
            FotosMasVendidas = BuildFotos(pedidosPagados),
            PaquetesMasVendidos = BuildPaquetes(pedidosPagados)
        };

        return ApiResponse<AdminVentasResumenDto>.Ok(response);
    }

    private static decimal CalculateGrowth(decimal previous, decimal current)
    {
        if (previous <= 0)
        {
            return current > 0 ? 100 : 0;
        }

        return Math.Round((current - previous) / previous * 100m, 2);
    }

    private static List<AdminProductoVendidoDto> BuildProductos(IEnumerable<Pedido> pedidos)
    {
        return pedidos.SelectMany(x => x.PedidoItems)
            .GroupBy(x => new { x.TipoItem, Id = x.FotoId ?? x.PaqueteEventoId ?? x.FotoPrivadaId, Nombre = x.Descripcion })
            .OrderByDescending(x => x.Sum(i => i.Subtotal))
            .Take(10)
            .Select(x => new AdminProductoVendidoDto
            {
                Id = x.Key.Id,
                Nombre = x.Key.Nombre,
                Tipo = x.Key.TipoItem,
                Cantidad = x.Sum(i => i.Cantidad),
                Total = x.Sum(i => i.Subtotal)
            })
            .ToList();
    }

    private static List<AdminProductoVendidoDto> BuildFotos(IEnumerable<Pedido> pedidos)
    {
        return pedidos.SelectMany(x => x.PedidoItems)
            .Where(x => x.FotoId is not null || x.FotoPrivadaId is not null)
            .GroupBy(x => new
            {
                Id = x.FotoId ?? x.FotoPrivadaId,
                Nombre = x.Foto?.NombreArchivo ?? x.FotoPrivada?.NombreArchivo ?? x.Descripcion,
                x.TipoItem
            })
            .OrderByDescending(x => x.Sum(i => i.Subtotal))
            .Take(10)
            .Select(x => new AdminProductoVendidoDto
            {
                Id = x.Key.Id,
                Nombre = x.Key.Nombre,
                Tipo = x.Key.TipoItem,
                Cantidad = x.Sum(i => i.Cantidad),
                Total = x.Sum(i => i.Subtotal)
            })
            .ToList();
    }

    private static List<AdminProductoVendidoDto> BuildPaquetes(IEnumerable<Pedido> pedidos)
    {
        return pedidos.SelectMany(x => x.PedidoItems)
            .Where(x => x.PaqueteEventoId is not null)
            .GroupBy(x => new { x.PaqueteEventoId, Nombre = x.PaqueteEvento?.Nombre ?? x.Descripcion })
            .OrderByDescending(x => x.Sum(i => i.Subtotal))
            .Take(10)
            .Select(x => new AdminProductoVendidoDto
            {
                Id = x.Key.PaqueteEventoId,
                Nombre = x.Key.Nombre,
                Tipo = PedidoItemTipos.PaqueteEvento,
                Cantidad = x.Sum(i => i.Cantidad),
                Total = x.Sum(i => i.Subtotal)
            })
            .ToList();
    }
}
