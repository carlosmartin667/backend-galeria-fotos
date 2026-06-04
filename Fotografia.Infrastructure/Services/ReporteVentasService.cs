using Fotografia.Application.DTOs.Reportes;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Services;

public sealed class ReporteVentasService(
    AppDbContext dbContext,
    ICurrentUserService currentUser) : IReporteVentasService
{
    public async Task<ApiResponse<ReporteVentasResumenDto>> GetVentasResumenAsync(
        DateTime? desde,
        DateTime? hasta,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<ReporteVentasResumenDto>.Forbidden("Solo un administrador puede consultar reportes de ventas.");
        }

        var pedidos = await QueryPedidos(desde, hasta)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var pagados = pedidos
            .Where(x => PedidoEstados.EsPagado(x.Estado, x.Pago?.Estado))
            .ToList();

        var cupones = await QueryCupones(desde, hasta)
            .AsNoTracking()
            .Where(x => x.Confirmado)
            .ToListAsync(cancellationToken);

        var response = new ReporteVentasResumenDto
        {
            TotalPedidos = pedidos.Count,
            TotalVentas = pagados.Sum(x => x.Total),
            TotalDescuentos = pagados.Sum(x => x.DescuentoTotal),
            PedidosPagados = pagados.Count,
            PedidosPendientes = pedidos.Count(x => PedidoEstados.EsPendientePago(x.Estado)),
            PedidosCancelados = pedidos.Count(x => PedidoEstados.EsFinal(x.Estado)),
            VentasPorDia = pagados
                .GroupBy(x => x.CreadoEnUtc.Date)
                .OrderBy(x => x.Key)
                .Select(x => new VentasPorDiaDto
                {
                    Fecha = x.Key,
                    Pedidos = x.Count(),
                    Total = x.Sum(p => p.Total)
                })
                .ToList(),
            VentasPorEstado = pedidos
                .GroupBy(x => string.IsNullOrWhiteSpace(x.Estado) ? "SinEstado" : x.Estado)
                .OrderBy(x => x.Key)
                .Select(x => new VentasPorEstadoDto
                {
                    Estado = x.Key,
                    Pedidos = x.Count(),
                    Total = x.Sum(p => p.Total)
                })
                .ToList(),
            VentasPorTipoItem = BuildVentasPorTipoItem(pagados),
            TopEventos = BuildTopEventos(pagados),
            TopFotos = BuildTopFotos(pagados),
            TopPaquetes = BuildTopPaquetes(pagados),
            CuponesMasUsados = cupones
                .GroupBy(x => new { x.CuponDescuentoId, x.Codigo })
                .OrderByDescending(x => x.Count())
                .ThenByDescending(x => x.Sum(u => u.MontoDescuento))
                .Take(10)
                .Select(x => new CuponUsoResumenDto
                {
                    CuponDescuentoId = x.Key.CuponDescuentoId,
                    Codigo = x.Key.Codigo,
                    Usos = x.Count(),
                    DescuentoTotal = x.Sum(u => u.MontoDescuento)
                })
                .ToList()
        };

        response.TicketPromedio = response.PedidosPagados == 0
            ? 0
            : Math.Round(response.TotalVentas / response.PedidosPagados, 2);

        return ApiResponse<ReporteVentasResumenDto>.Ok(response);
    }

    private IQueryable<Pedido> QueryPedidos(DateTime? desde, DateTime? hasta)
    {
        var query = dbContext.Pedidos
            .Include(x => x.Evento)
            .Include(x => x.Pago)
            .Include(x => x.PedidoFotos)
            .ThenInclude(x => x.Foto)
            .Include(x => x.PedidoItems)
            .ThenInclude(x => x.Foto)
            .Include(x => x.PedidoItems)
            .ThenInclude(x => x.PaqueteEvento)
            .Include(x => x.PedidoItems)
            .ThenInclude(x => x.FotoPrivada)
            .AsQueryable();

        if (desde is DateTime desdeValue)
        {
            query = query.Where(x => x.CreadoEnUtc >= desdeValue);
        }

        if (hasta is DateTime hastaValue)
        {
            query = query.Where(x => x.CreadoEnUtc <= hastaValue);
        }

        return query;
    }

    private IQueryable<CuponUso> QueryCupones(DateTime? desde, DateTime? hasta)
    {
        var query = dbContext.CuponUsos.AsQueryable();

        if (desde is DateTime desdeValue)
        {
            query = query.Where(x => x.FechaUsoUtc >= desdeValue);
        }

        if (hasta is DateTime hastaValue)
        {
            query = query.Where(x => x.FechaUsoUtc <= hastaValue);
        }

        return query;
    }

    private static List<VentasPorTipoItemDto> BuildVentasPorTipoItem(IEnumerable<Pedido> pedidos)
    {
        var items = pedidos.SelectMany(x => x.PedidoItems);
        return items
            .GroupBy(x => x.TipoItem)
            .OrderByDescending(x => x.Sum(i => i.Subtotal))
            .Select(x => new VentasPorTipoItemDto
            {
                TipoItem = x.Key,
                Cantidad = x.Sum(i => i.Cantidad),
                Total = x.Sum(i => i.Subtotal)
            })
            .ToList();
    }

    private static List<TopVentaDto> BuildTopEventos(IEnumerable<Pedido> pedidos)
    {
        return pedidos
            .Where(x => x.EventoId is not null)
            .GroupBy(x => new { x.EventoId, Nombre = x.Evento?.Nombre ?? "Evento" })
            .OrderByDescending(x => x.Sum(p => p.Total))
            .Take(10)
            .Select(x => new TopVentaDto
            {
                Id = x.Key.EventoId,
                Nombre = x.Key.Nombre,
                Cantidad = x.Count(),
                Total = x.Sum(p => p.Total)
            })
            .ToList();
    }

    private static List<TopVentaDto> BuildTopFotos(IEnumerable<Pedido> pedidos)
    {
        var items = pedidos.SelectMany(x => x.PedidoItems)
            .Where(x => x.FotoId is not null)
            .Select(x => new
            {
                Id = x.FotoId,
                Nombre = x.Foto?.NombreArchivo ?? x.Descripcion,
                x.Cantidad,
                Total = x.Subtotal
            });

        var legacyItems = pedidos
            .Where(x => x.PedidoItems.Count == 0)
            .SelectMany(x => x.PedidoFotos)
            .Where(x => x.FotoId != Guid.Empty)
            .Select(x => new
            {
                Id = (Guid?)x.FotoId,
                Nombre = x.Foto?.NombreArchivo ?? "Foto digital",
                x.Cantidad,
                Total = x.PrecioUnitario * x.Cantidad
            });

        return items.Concat(legacyItems)
            .GroupBy(x => new { x.Id, x.Nombre })
            .OrderByDescending(x => x.Sum(i => i.Total))
            .Take(10)
            .Select(x => new TopVentaDto
            {
                Id = x.Key.Id,
                Nombre = x.Key.Nombre,
                Cantidad = x.Sum(i => i.Cantidad),
                Total = x.Sum(i => i.Total)
            })
            .ToList();
    }

    private static List<TopVentaDto> BuildTopPaquetes(IEnumerable<Pedido> pedidos)
    {
        return pedidos.SelectMany(x => x.PedidoItems)
            .Where(x => x.PaqueteEventoId is not null)
            .GroupBy(x => new { x.PaqueteEventoId, Nombre = x.PaqueteEvento?.Nombre ?? x.Descripcion })
            .OrderByDescending(x => x.Sum(i => i.Subtotal))
            .Take(10)
            .Select(x => new TopVentaDto
            {
                Id = x.Key.PaqueteEventoId,
                Nombre = x.Key.Nombre,
                Cantidad = x.Sum(i => i.Cantidad),
                Total = x.Sum(i => i.Subtotal)
            })
            .ToList();
    }
}
