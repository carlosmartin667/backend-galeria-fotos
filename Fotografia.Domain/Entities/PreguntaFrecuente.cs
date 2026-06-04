namespace Fotografia.Domain.Entities;

public sealed class PreguntaFrecuente
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Pregunta { get; set; }
    public required string Respuesta { get; set; }
    public string? Categoria { get; set; }
    public int Orden { get; set; }
    public bool Activa { get; set; } = true;
    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacionUtc { get; set; }
}
