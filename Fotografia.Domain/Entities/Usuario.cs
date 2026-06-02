namespace Fotografia.Domain.Entities;

public sealed class Usuario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Nombre { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public string Rol { get; set; } = "Usuario";
    public bool Activo { get; set; } = true;
    public string? Descripcion { get; set; }
    public string? WhatsApp { get; set; }
    public string? Instagram { get; set; }
    public string? CorreoPublico { get; set; }
    public string? Direccion { get; set; }
    public DateTime CreadoEnUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ActualizadoEnUtc { get; set; }

    public Cliente? Cliente { get; set; }
    public ICollection<Evento> EventosCreados { get; set; } = [];
    public ICollection<ComentarioEvento> ComentariosEventos { get; set; } = [];
    public ICollection<ComentarioFoto> ComentariosFotos { get; set; } = [];
    public ICollection<EventoFavorito> EventosFavoritos { get; set; } = [];
    public ICollection<FotoFavorita> FotosFavoritas { get; set; } = [];
    public ICollection<CarritoCompra> CarritosCompra { get; set; } = [];
    public ICollection<Notificacion> Notificaciones { get; set; } = [];
}
