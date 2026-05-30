namespace Fotografia.Domain.Entities;

public sealed class PedidoFoto
{
    public Guid PedidoId { get; set; }
    public Pedido? Pedido { get; set; }

    public Guid FotoId { get; set; }
    public Foto? Foto { get; set; }

    public int Cantidad { get; set; } = 1;
    public decimal PrecioUnitario { get; set; }
}
