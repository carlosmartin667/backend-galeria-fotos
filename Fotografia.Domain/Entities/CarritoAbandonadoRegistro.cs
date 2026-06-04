namespace Fotografia.Domain.Entities;

public sealed class CarritoAbandonadoRegistro
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CarritoCompraId { get; set; }
    public CarritoCompra? CarritoCompra { get; set; }

    public Guid? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public Guid? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public DateTime FechaDetectadoUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaUltimaNotificacionUtc { get; set; }
    public int NotificacionesEnviadas { get; set; }
    public required string Estado { get; set; }
    public bool Activo { get; set; } = true;
}
