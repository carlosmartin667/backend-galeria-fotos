namespace Fotografia.Domain.Entities;

public sealed class Evento
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public string Slug { get; set; } = string.Empty;
    public DateTime FechaEventoUtc { get; set; }
    public string Estado { get; set; } = "Activo";
    public string Visibilidad { get; set; } = "Publico";
    public DateTime? FechaLimiteCompraUtc { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime CreadoEnUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ActualizadoEnUtc { get; set; }

    public Guid? ClientePrincipalId { get; set; }
    public Cliente? ClientePrincipal { get; set; }

    public Guid? PortadaFotoId { get; set; }
    public Foto? PortadaFoto { get; set; }

    public Guid? CreadoPorUsuarioId { get; set; }
    public Usuario? CreadoPorUsuario { get; set; }

    public ICollection<Foto> Fotos { get; set; } = [];
    public ICollection<Pedido> Pedidos { get; set; } = [];
    public ICollection<Descarga> Descargas { get; set; } = [];
    public ICollection<ComentarioEvento> Comentarios { get; set; } = [];
    public ICollection<EventoFavorito> Favoritos { get; set; } = [];
    public ICollection<PaqueteEvento> Paquetes { get; set; } = [];
    public ICollection<Promocion> Promociones { get; set; } = [];
    public ICollection<Testimonio> Testimonios { get; set; } = [];
}
