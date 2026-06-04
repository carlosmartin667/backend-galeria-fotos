namespace Fotografia.Application.DTOs.Reportes;

public sealed class ReporteVentasResumenDto
{
    public int TotalPedidos { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal TotalDescuentos { get; set; }
    public decimal TicketPromedio { get; set; }
    public int PedidosPagados { get; set; }
    public int PedidosPendientes { get; set; }
    public int PedidosCancelados { get; set; }
    public List<VentasPorDiaDto> VentasPorDia { get; set; } = [];
    public List<VentasPorEstadoDto> VentasPorEstado { get; set; } = [];
    public List<VentasPorTipoItemDto> VentasPorTipoItem { get; set; } = [];
    public List<TopVentaDto> TopEventos { get; set; } = [];
    public List<TopVentaDto> TopFotos { get; set; } = [];
    public List<TopVentaDto> TopPaquetes { get; set; } = [];
    public List<CuponUsoResumenDto> CuponesMasUsados { get; set; } = [];
}

public sealed class VentasPorDiaDto
{
    public DateTime Fecha { get; set; }
    public int Pedidos { get; set; }
    public decimal Total { get; set; }
}

public sealed class VentasPorEstadoDto
{
    public string Estado { get; set; } = string.Empty;
    public int Pedidos { get; set; }
    public decimal Total { get; set; }
}

public sealed class VentasPorTipoItemDto
{
    public string TipoItem { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Total { get; set; }
}

public sealed class TopVentaDto
{
    public Guid? Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Total { get; set; }
}

public sealed class CuponUsoResumenDto
{
    public Guid? CuponDescuentoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public int Usos { get; set; }
    public decimal DescuentoTotal { get; set; }
}
