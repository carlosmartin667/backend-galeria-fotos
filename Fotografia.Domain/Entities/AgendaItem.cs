namespace Fotografia.Domain.Entities;

public sealed class AgendaItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Titulo { get; set; }
    public string? Descripcion { get; set; }
    public string Tipo { get; set; } = "Otro";
    public DateTime FechaInicioUtc { get; set; }
    public DateTime FechaFinUtc { get; set; }
    public string? Ubicacion { get; set; }
    public string Estado { get; set; } = "Programado";

    public Guid? EventoId { get; set; }
    public Evento? Evento { get; set; }

    public Guid? SesionPrivadaId { get; set; }
    public SesionPrivada? SesionPrivada { get; set; }

    public Guid? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public Guid? SolicitudPresupuestoId { get; set; }
    public SolicitudPresupuesto? SolicitudPresupuesto { get; set; }

    public bool Activo { get; set; } = true;
    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacionUtc { get; set; }
}
