using Fotografia.Application.DTOs.Admin;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Fotografia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Services;

public sealed class AdminDashboardService(AppDbContext dbContext) : IAdminDashboardService
{
    public async Task<ApiResponse<AdminDashboardResponseDto>> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var ingresosMes = await dbContext.Pagos
            .AsNoTracking()
            .Where(x => x.PagadoEnUtc >= monthStart && x.Estado == "approved")
            .SumAsync(x => (decimal?)x.Monto, cancellationToken) ?? 0m;

        var fotosMasCompradas = await GetFotosMasCompradasAsync(cancellationToken);
        var paquetesMasVendidos = await dbContext.PedidoItems
            .AsNoTracking()
            .Include(x => x.Pedido)
            .Include(x => x.PaqueteEvento)
            .Where(x => x.PaqueteEventoId != null && x.Pedido != null && x.Pedido.Estado == PedidoEstados.Pagado)
            .GroupBy(x => new
            {
                PaqueteEventoId = x.PaqueteEventoId!.Value,
                Nombre = x.PaqueteEvento == null ? x.Descripcion : x.PaqueteEvento.Nombre
            })
            .Select(group => new DashboardPaqueteMasVendidoDto
            {
                PaqueteEventoId = group.Key.PaqueteEventoId,
                Nombre = group.Key.Nombre,
                CantidadVendida = group.Sum(item => item.Cantidad),
                TotalVendido = group.Sum(item => item.Subtotal)
            })
            .OrderByDescending(x => x.CantidadVendida)
            .ThenByDescending(x => x.TotalVendido)
            .Take(5)
            .ToListAsync(cancellationToken);

        var ultimosPedidos = await dbContext.Pedidos
            .AsNoTracking()
            .Include(x => x.Cliente)
            .OrderByDescending(x => x.CreadoEnUtc)
            .Take(5)
            .Select(x => new DashboardPedidoResumenDto
            {
                PedidoId = x.Id,
                ClienteId = x.ClienteId,
                ClienteNombre = x.Cliente == null ? string.Empty : x.Cliente.Nombre,
                Estado = x.Estado,
                Total = x.Total,
                CreadoEnUtc = x.CreadoEnUtc
            })
            .ToListAsync(cancellationToken);

        var dashboard = new AdminDashboardResponseDto
        {
            EventosActivos = await dbContext.Eventos.CountAsync(x => x.Activo, cancellationToken),
            EventosPublicados = await dbContext.Eventos.CountAsync(
                x => x.Activo && (x.Estado == EventoEstados.Publicado || x.Estado == EventoEstados.LegacyActivo),
                cancellationToken),
            FotosSubidas = await dbContext.Fotos.CountAsync(x => x.Activa, cancellationToken),
            PedidosPendientes = await dbContext.Pedidos.CountAsync(
                x => x.Estado == PedidoEstados.Pendiente || x.Estado == PedidoEstados.PagoPendiente,
                cancellationToken),
            PedidosPagados = await dbContext.Pedidos.CountAsync(x => x.Estado == PedidoEstados.Pagado, cancellationToken),
            CarritosActivos = await dbContext.CarritosCompra.CountAsync(x => x.Estado == CarritoEstados.Activo, cancellationToken),
            IngresosMes = ingresosMes,
            ClientesRegistrados = await dbContext.Clientes.CountAsync(cancellationToken),
            SesionesPrivadasActivas = await dbContext.SesionesPrivadas.CountAsync(x => x.Activa, cancellationToken),
            FotosMasCompradas = fotosMasCompradas,
            PaquetesMasVendidos = paquetesMasVendidos,
            UltimosPedidos = ultimosPedidos
        };

        return ApiResponse<AdminDashboardResponseDto>.Ok(dashboard);
    }

    private async Task<List<DashboardFotoMasCompradaDto>> GetFotosMasCompradasAsync(CancellationToken cancellationToken)
    {
        var fromPedidoItems = await dbContext.PedidoItems
            .AsNoTracking()
            .Include(x => x.Pedido)
            .Include(x => x.Foto)
            .Where(x => x.FotoId != null && x.Pedido != null && x.Pedido.Estado == PedidoEstados.Pagado)
            .GroupBy(x => new
            {
                FotoId = x.FotoId!.Value,
                NombreArchivo = x.Foto == null ? x.Descripcion : x.Foto.NombreArchivo
            })
            .Select(group => new DashboardFotoMasCompradaDto
            {
                FotoId = group.Key.FotoId,
                NombreArchivo = group.Key.NombreArchivo,
                CantidadVendida = group.Sum(item => item.Cantidad),
                TotalVendido = group.Sum(item => item.Subtotal)
            })
            .ToListAsync(cancellationToken);

        var fromPedidoFotos = await dbContext.PedidoFotos
            .AsNoTracking()
            .Include(x => x.Pedido)
            .Include(x => x.Foto)
            .Where(x => x.Pedido != null && x.Pedido.Estado == PedidoEstados.Pagado)
            .GroupBy(x => new
            {
                x.FotoId,
                NombreArchivo = x.Foto == null ? "Foto digital" : x.Foto.NombreArchivo
            })
            .Select(group => new DashboardFotoMasCompradaDto
            {
                FotoId = group.Key.FotoId,
                NombreArchivo = group.Key.NombreArchivo,
                CantidadVendida = group.Sum(item => item.Cantidad),
                TotalVendido = group.Sum(item => item.Cantidad * item.PrecioUnitario)
            })
            .ToListAsync(cancellationToken);

        return fromPedidoItems
            .Concat(fromPedidoFotos)
            .GroupBy(x => x.FotoId)
            .Select(group => new DashboardFotoMasCompradaDto
            {
                FotoId = group.Key,
                NombreArchivo = group.First().NombreArchivo,
                CantidadVendida = group.Sum(x => x.CantidadVendida),
                TotalVendido = group.Sum(x => x.TotalVendido)
            })
            .OrderByDescending(x => x.CantidadVendida)
            .ThenByDescending(x => x.TotalVendido)
            .Take(5)
            .ToList();
    }
}
