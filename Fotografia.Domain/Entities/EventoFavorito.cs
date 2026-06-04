namespace Fotografia.Domain.Entities;

public sealed class EventoFavorito
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EventoId { get; set; }
    public Evento? Evento { get; set; }

    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
}
