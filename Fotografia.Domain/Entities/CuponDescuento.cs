namespace Fotografia.Domain.Entities;

public sealed class CuponDescuento
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Codigo { get; set; }
    public string? Descripcion { get; set; }
    public required string TipoDescuento { get; set; }
    public decimal ValorDescuento { get; set; }
    public decimal? MontoMinimoCompra { get; set; }
    public decimal? MontoMaximoDescuento { get; set; }
    public DateTime? FechaInicioUtc { get; set; }
    public DateTime? FechaFinUtc { get; set; }
    public int? UsosMaximos { get; set; }
    public int UsosActuales { get; set; }
    public int? UsosMaximosPorUsuario { get; set; }
    public bool SoloPrimerCompra { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacionUtc { get; set; }

    public ICollection<CuponUso> Usos { get; set; } = [];
    public ICollection<Pedido> Pedidos { get; set; } = [];
    public ICollection<CarritoCompra> CarritosCompra { get; set; } = [];
    public ICollection<Promocion> Promociones { get; set; } = [];
}
