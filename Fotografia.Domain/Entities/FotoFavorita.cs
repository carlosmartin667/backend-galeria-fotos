namespace Fotografia.Domain.Entities;

public sealed class FotoFavorita
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid FotoId { get; set; }
    public Foto? Foto { get; set; }

    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
}
