namespace Fotografia.Domain.Entities;

public sealed class SesionPrivada
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public required string Titulo { get; set; }
    public string? Descripcion { get; set; }
    public DateTime FechaSesionUtc { get; set; }
    public string Estado { get; set; } = "Activa";
    public decimal? PrecioPaquete { get; set; }
    public bool Activa { get; set; } = true;
    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacionUtc { get; set; }

    public ICollection<FotoPrivada> Fotos { get; set; } = [];
}
