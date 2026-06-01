namespace Fotografia.Domain.Entities;

public sealed class SolicitudPresupuesto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Nombre { get; set; }
    public required string Email { get; set; }
    public string? WhatsApp { get; set; }
    public string? TipoEvento { get; set; }
    public Guid? ServicioId { get; set; }
    public ServicioFotografia? Servicio { get; set; }
    public DateTime? FechaTentativaUtc { get; set; }
    public string? Lugar { get; set; }
    public int? CantidadInvitados { get; set; }
    public required string Mensaje { get; set; }
    public string Estado { get; set; } = "Nuevo";
    public bool Activa { get; set; } = true;
    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacionUtc { get; set; }
}
