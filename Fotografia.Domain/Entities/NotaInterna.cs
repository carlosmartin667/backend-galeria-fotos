namespace Fotografia.Domain.Entities;

public sealed class NotaInterna
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string EntidadTipo { get; set; }
    public Guid EntidadId { get; set; }
    public required string Texto { get; set; }

    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public bool Activa { get; set; } = true;
    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacionUtc { get; set; }
}
