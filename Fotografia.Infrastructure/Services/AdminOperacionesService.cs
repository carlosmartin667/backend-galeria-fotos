using Fotografia.Application.DTOs.Admin;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Fotografia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Services;

public sealed class AdminOperacionesService(
    AppDbContext dbContext,
    ICurrentUserService currentUser) : IAdminOperacionesService
{
    private static readonly string[] PendingOrderStates =
    [
        PedidoEstados.Pendiente,
        PedidoEstados.PendientePago,
        PedidoEstados.PagoPendiente
    ];

    private static readonly string[] PaidOrderStates =
    [
        PedidoEstados.Pagado,
        PedidoEstados.Aprobado,
        PedidoEstados.PagoAprobado,
        "Pago aprobado",
        "approved"
    ];

    public async Task<ApiResponse<AdminOperacionesResumenDto>> GetResumenAsync(
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<AdminOperacionesResumenDto>.Forbidden("Solo un administrador puede consultar operaciones.");
        }

        var now = DateTime.UtcNow;
        var soon = now.AddDays(7);
        var upcoming = now.AddDays(30);

        return ApiResponse<AdminOperacionesResumenDto>.Ok(new AdminOperacionesResumenDto
        {
            SolicitudesNuevas = await dbContext.SolicitudesPresupuesto.CountAsync(
                x => x.Activa && x.Estado == SolicitudPresupuestoEstados.Nuevo,
                cancellationToken),
            SolicitudesPendientesContacto = await dbContext.SolicitudesPresupuesto.CountAsync(
                x => x.Activa && (x.Estado == SolicitudPresupuestoEstados.Nuevo || x.Estado == SolicitudPresupuestoEstados.Contactado),
                cancellationToken),
            PedidosPendientesPago = await dbContext.Pedidos.CountAsync(x => PendingOrderStates.Contains(x.Estado), cancellationToken),
            PedidosPagados = await dbContext.Pedidos.CountAsync(x => PaidOrderStates.Contains(x.Estado), cancellationToken),
            PedidosPreparandoDescarga = await dbContext.Pedidos.CountAsync(x => x.Estado == PedidoEstados.PreparandoDescarga, cancellationToken),
            PedidosListosParaDescargar = await dbContext.Pedidos.CountAsync(x => x.Estado == PedidoEstados.ListoParaDescargar, cancellationToken),
            SesionesPrivadasActivas = await dbContext.SesionesPrivadas.CountAsync(
                x => x.Activa && x.Estado != SesionPrivadaEstados.Cancelada && x.Estado != SesionPrivadaEstados.Finalizada,
                cancellationToken),
            EventosProximos = await dbContext.Eventos.CountAsync(
                x => x.Activo && x.FechaEventoUtc >= now && x.FechaEventoUtc <= upcoming,
                cancellationToken),
            AgendaProxima = await dbContext.AgendaItems.CountAsync(
                x => x.Activo && x.Estado != AgendaItemEstados.Cancelado && x.FechaInicioUtc >= now && x.FechaInicioUtc <= upcoming,
                cancellationToken),
            DescargasVencidas = await dbContext.Descargas.CountAsync(
                x => x.Activa && x.ExpiraEnUtc < now,
                cancellationToken),
            DescargasPorVencer = await dbContext.Descargas.CountAsync(
                x => x.Activa && x.ExpiraEnUtc >= now && x.ExpiraEnUtc <= soon,
                cancellationToken),
            PedidosRecientes = await GetPedidosRecientesAsync(10, cancellationToken),
            SolicitudesRecientes = await GetSolicitudesRecientesAsync(10, cancellationToken),
            ProximosAgendaItems = await GetAgendaProximaAsync(now, upcoming, 10, cancellationToken)
        });
    }

    public async Task<ApiResponse<AdminOperacionesPendientesDto>> GetPendientesAsync(
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<AdminOperacionesPendientesDto>.Forbidden("Solo un administrador puede consultar pendientes.");
        }

        var now = DateTime.UtcNow;
        var soon = now.AddDays(7);
        var upcoming = now.AddDays(30);

        var solicitudes = await dbContext.SolicitudesPresupuesto
            .AsNoTracking()
            .Where(x => x.Activa && (x.Estado == SolicitudPresupuestoEstados.Nuevo || x.Estado == SolicitudPresupuestoEstados.Contactado))
            .OrderByDescending(x => x.FechaCreacionUtc)
            .Take(25)
            .Select(x => new AdminOperacionSolicitudResumenDto
            {
                SolicitudId = x.Id,
                Nombre = x.Nombre,
                Email = x.Email,
                WhatsApp = x.WhatsApp,
                Estado = x.Estado,
                FechaCreacionUtc = x.FechaCreacionUtc
            })
            .ToListAsync(cancellationToken);

        var pedidos = await dbContext.Pedidos
            .AsNoTracking()
            .Include(x => x.Cliente)
            .Where(x => PendingOrderStates.Contains(x.Estado)
                || x.Estado == PedidoEstados.Pagado
                || x.Estado == PedidoEstados.PreparandoDescarga)
            .OrderByDescending(x => x.CreadoEnUtc)
            .Take(25)
            .Select(x => new AdminOperacionPedidoResumenDto
            {
                PedidoId = x.Id,
                ClienteId = x.ClienteId,
                ClienteNombre = x.Cliente == null ? string.Empty : x.Cliente.Nombre,
                Estado = x.Estado,
                Total = x.Total,
                Moneda = x.Moneda,
                CreadoEnUtc = x.CreadoEnUtc
            })
            .ToListAsync(cancellationToken);

        var sesiones = await dbContext.SesionesPrivadas
            .AsNoTracking()
            .Include(x => x.Cliente)
            .Where(x => x.Activa && x.Estado != SesionPrivadaEstados.Cancelada && x.Estado != SesionPrivadaEstados.Finalizada)
            .OrderBy(x => x.FechaSesionUtc)
            .Take(25)
            .Select(x => new AdminOperacionSesionPrivadaResumenDto
            {
                SesionPrivadaId = x.Id,
                ClienteId = x.ClienteId,
                ClienteNombre = x.Cliente == null ? string.Empty : x.Cliente.Nombre,
                Titulo = x.Titulo,
                Estado = x.Estado,
                FechaSesionUtc = x.FechaSesionUtc
            })
            .ToListAsync(cancellationToken);

        var descargas = await dbContext.Descargas
            .AsNoTracking()
            .Include(x => x.Cliente)
            .Where(x => x.Activa && x.ExpiraEnUtc >= now && x.ExpiraEnUtc <= soon)
            .OrderBy(x => x.ExpiraEnUtc)
            .Take(25)
            .Select(x => new AdminOperacionDescargaResumenDto
            {
                DescargaId = x.Id,
                PedidoId = x.PedidoId,
                ClienteId = x.ClienteId,
                ClienteNombre = x.Cliente == null ? string.Empty : x.Cliente.Nombre,
                NombreArchivo = x.NombreArchivo,
                ExpiraEnUtc = x.ExpiraEnUtc,
                MaxDescargas = x.MaxDescargas,
                DescargasRealizadas = x.DescargasRealizadas
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<AdminOperacionesPendientesDto>.Ok(new AdminOperacionesPendientesDto
        {
            Solicitudes = solicitudes,
            Pedidos = pedidos,
            SesionesPrivadas = sesiones,
            Agenda = await GetAgendaProximaAsync(now, upcoming, 25, cancellationToken),
            DescargasPorVencer = descargas
        });
    }

    private async Task<List<AdminOperacionPedidoResumenDto>> GetPedidosRecientesAsync(
        int take,
        CancellationToken cancellationToken)
    {
        return await dbContext.Pedidos
            .AsNoTracking()
            .Include(x => x.Cliente)
            .OrderByDescending(x => x.CreadoEnUtc)
            .Take(take)
            .Select(x => new AdminOperacionPedidoResumenDto
            {
                PedidoId = x.Id,
                ClienteId = x.ClienteId,
                ClienteNombre = x.Cliente == null ? string.Empty : x.Cliente.Nombre,
                Estado = x.Estado,
                Total = x.Total,
                Moneda = x.Moneda,
                CreadoEnUtc = x.CreadoEnUtc
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<List<AdminOperacionSolicitudResumenDto>> GetSolicitudesRecientesAsync(
        int take,
        CancellationToken cancellationToken)
    {
        return await dbContext.SolicitudesPresupuesto
            .AsNoTracking()
            .OrderByDescending(x => x.FechaCreacionUtc)
            .Take(take)
            .Select(x => new AdminOperacionSolicitudResumenDto
            {
                SolicitudId = x.Id,
                Nombre = x.Nombre,
                Email = x.Email,
                WhatsApp = x.WhatsApp,
                Estado = x.Estado,
                FechaCreacionUtc = x.FechaCreacionUtc
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<List<AdminOperacionAgendaItemResumenDto>> GetAgendaProximaAsync(
        DateTime from,
        DateTime to,
        int take,
        CancellationToken cancellationToken)
    {
        return await dbContext.AgendaItems
            .AsNoTracking()
            .Where(x => x.Activo && x.Estado != AgendaItemEstados.Cancelado && x.FechaInicioUtc >= from && x.FechaInicioUtc <= to)
            .OrderBy(x => x.FechaInicioUtc)
            .Take(take)
            .Select(x => new AdminOperacionAgendaItemResumenDto
            {
                AgendaItemId = x.Id,
                Titulo = x.Titulo,
                Tipo = x.Tipo,
                Estado = x.Estado,
                FechaInicioUtc = x.FechaInicioUtc,
                FechaFinUtc = x.FechaFinUtc
            })
            .ToListAsync(cancellationToken);
    }
}
