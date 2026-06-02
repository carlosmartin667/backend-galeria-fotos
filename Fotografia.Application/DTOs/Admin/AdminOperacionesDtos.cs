namespace Fotografia.Application.DTOs.Admin;

public sealed class AdminOperacionesResumenDto
{
    public int SolicitudesNuevas { get; set; }
    public int SolicitudesPendientesContacto { get; set; }
    public int PedidosPendientesPago { get; set; }
    public int PedidosPagados { get; set; }
    public int PedidosPreparandoDescarga { get; set; }
    public int PedidosListosParaDescargar { get; set; }
    public int SesionesPrivadasActivas { get; set; }
    public int EventosProximos { get; set; }
    public int AgendaProxima { get; set; }
    public int DescargasVencidas { get; set; }
    public int DescargasPorVencer { get; set; }
    public List<AdminOperacionPedidoResumenDto> PedidosRecientes { get; set; } = [];
    public List<AdminOperacionSolicitudResumenDto> SolicitudesRecientes { get; set; } = [];
    public List<AdminOperacionAgendaItemResumenDto> ProximosAgendaItems { get; set; } = [];
}

public sealed class AdminOperacionesPendientesDto
{
    public List<AdminOperacionSolicitudResumenDto> Solicitudes { get; set; } = [];
    public List<AdminOperacionPedidoResumenDto> Pedidos { get; set; } = [];
    public List<AdminOperacionSesionPrivadaResumenDto> SesionesPrivadas { get; set; } = [];
    public List<AdminOperacionAgendaItemResumenDto> Agenda { get; set; } = [];
    public List<AdminOperacionDescargaResumenDto> DescargasPorVencer { get; set; } = [];
}

public sealed class AdminOperacionPedidoResumenDto
{
    public Guid PedidoId { get; set; }
    public Guid ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public DateTime CreadoEnUtc { get; set; }
}

public sealed class AdminOperacionSolicitudResumenDto
{
    public Guid SolicitudId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? WhatsApp { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacionUtc { get; set; }
}

public sealed class AdminOperacionSesionPrivadaResumenDto
{
    public Guid SesionPrivadaId { get; set; }
    public Guid ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaSesionUtc { get; set; }
}

public sealed class AdminOperacionAgendaItemResumenDto
{
    public Guid AgendaItemId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaInicioUtc { get; set; }
    public DateTime FechaFinUtc { get; set; }
}

public sealed class AdminOperacionDescargaResumenDto
{
    public Guid DescargaId { get; set; }
    public Guid PedidoId { get; set; }
    public Guid ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string NombreArchivo { get; set; } = string.Empty;
    public DateTime ExpiraEnUtc { get; set; }
    public int? MaxDescargas { get; set; }
    public int DescargasRealizadas { get; set; }
}
