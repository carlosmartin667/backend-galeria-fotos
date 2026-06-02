namespace Fotografia.Application.DTOs.Admin;

public sealed class AdminVentasResumenDto
{
    public decimal VentasMes { get; set; }
    public decimal VentasMesAnterior { get; set; }
    public decimal CrecimientoPorcentual { get; set; }
    public int CarritosAbandonados { get; set; }
    public int CarritosRecuperados { get; set; }
    public int CuponesActivos { get; set; }
    public int PromocionesActivas { get; set; }
    public int TestimoniosPendientes { get; set; }
    public List<AdminProductoVendidoDto> ProductosMasVendidos { get; set; } = [];
    public List<AdminProductoVendidoDto> FotosMasVendidas { get; set; } = [];
    public List<AdminProductoVendidoDto> PaquetesMasVendidos { get; set; } = [];
}

public sealed class AdminProductoVendidoDto
{
    public Guid? Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Total { get; set; }
}
