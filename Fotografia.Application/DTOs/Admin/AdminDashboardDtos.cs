namespace Fotografia.Application.DTOs.Admin;

public sealed class AdminDashboardResponseDto
{
    public int EventosActivos { get; set; }
    public int EventosPublicados { get; set; }
    public int FotosSubidas { get; set; }
    public int PedidosPendientes { get; set; }
    public int PedidosPagados { get; set; }
    public int CarritosActivos { get; set; }
    public decimal IngresosMes { get; set; }
    public int ClientesRegistrados { get; set; }
    public int SesionesPrivadasActivas { get; set; }
    public List<DashboardFotoMasCompradaDto> FotosMasCompradas { get; set; } = [];
    public List<DashboardPaqueteMasVendidoDto> PaquetesMasVendidos { get; set; } = [];
    public List<DashboardPedidoResumenDto> UltimosPedidos { get; set; } = [];
}

public sealed class DashboardFotoMasCompradaDto
{
    public Guid FotoId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public int CantidadVendida { get; set; }
    public decimal TotalVendido { get; set; }
}

public sealed class DashboardPaqueteMasVendidoDto
{
    public Guid PaqueteEventoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int CantidadVendida { get; set; }
    public decimal TotalVendido { get; set; }
}

public sealed class DashboardPedidoResumenDto
{
    public Guid PedidoId { get; set; }
    public Guid ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime CreadoEnUtc { get; set; }
}
