namespace Fotografia.Domain.Entities;

public sealed class ServicioFotografia
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public decimal? PrecioDesde { get; set; }
    public string? DuracionEstimada { get; set; }
    public int? CantidadFotosIncluidas { get; set; }
    public string? ImagenUrl { get; set; }
    public bool Destacado { get; set; }
    public int? OrdenDestacado { get; set; }
    public bool Activo { get; set; } = true;
    public int Orden { get; set; }
    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacionUtc { get; set; }
    public ICollection<Promocion> Promociones { get; set; } = [];
    public ICollection<Testimonio> Testimonios { get; set; } = [];
}
