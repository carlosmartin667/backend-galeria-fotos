using Fotografia.Domain.Constants;

namespace Fotografia.Domain.Entities;

public sealed class CarritoCompra
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public string Estado { get; set; } = CarritoEstados.Activo;
    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacionUtc { get; set; }

    public ICollection<CarritoItem> Items { get; set; } = [];
}
