namespace Fotografia.Domain.Entities;

public sealed class CuponUso
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CuponDescuentoId { get; set; }
    public CuponDescuento? CuponDescuento { get; set; }

    public Guid PedidoId { get; set; }
    public Pedido? Pedido { get; set; }

    public Guid? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public Guid? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public required string Codigo { get; set; }
    public decimal MontoDescuento { get; set; }
    public DateTime FechaUsoUtc { get; set; } = DateTime.UtcNow;
    public bool Confirmado { get; set; }
}
