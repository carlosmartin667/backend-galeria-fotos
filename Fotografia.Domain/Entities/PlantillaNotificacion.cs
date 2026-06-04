namespace Fotografia.Domain.Entities;

public sealed class PlantillaNotificacion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Codigo { get; set; }
    public required string Canal { get; set; }
    public required string Asunto { get; set; }
    public required string CuerpoHtml { get; set; }
    public string? CuerpoTexto { get; set; }
    public bool Activa { get; set; } = true;
    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacionUtc { get; set; }
}
