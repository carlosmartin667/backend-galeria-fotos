namespace Fotografia.Domain.Entities;

public sealed class Usuario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Nombre { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public string Rol { get; set; } = "Admin";
    public bool Activo { get; set; } = true;
    public DateTime CreadoEnUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ActualizadoEnUtc { get; set; }

    public ICollection<Evento> EventosCreados { get; set; } = [];
}
