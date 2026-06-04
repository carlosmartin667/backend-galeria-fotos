namespace Fotografia.Domain.Entities;

public sealed class Cliente
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Nombre { get; set; }
    public required string Email { get; set; }
    public string? Telefono { get; set; }
    public string? Documento { get; set; }
    public Guid? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public DateTime CreadoEnUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ActualizadoEnUtc { get; set; }

    public ICollection<Evento> EventosPrincipales { get; set; } = [];
    public ICollection<Pedido> Pedidos { get; set; } = [];
    public ICollection<Descarga> Descargas { get; set; } = [];
    public ICollection<SesionPrivada> SesionesPrivadas { get; set; } = [];
    public ICollection<FotoPrivada> FotosPrivadas { get; set; } = [];
    public ICollection<CuponUso> CuponUsos { get; set; } = [];
    public ICollection<Testimonio> Testimonios { get; set; } = [];
    public ICollection<CarritoAbandonadoRegistro> CarritosAbandonados { get; set; } = [];
}
