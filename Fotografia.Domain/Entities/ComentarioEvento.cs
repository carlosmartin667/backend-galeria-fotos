namespace Fotografia.Domain.Entities;

public sealed class ComentarioEvento
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EventoId { get; set; }
    public Evento? Evento { get; set; }

    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public required string Texto { get; set; }
    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacionUtc { get; set; }
    public bool Activo { get; set; } = true;
}
