using Fotografia.Domain.Constants;

namespace Fotografia.Domain.Entities;

public sealed class PedidoItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PedidoId { get; set; }
    public Pedido? Pedido { get; set; }

    public string TipoItem { get; set; } = PedidoItemTipos.FotoEvento;
    public Guid? FotoId { get; set; }
    public Foto? Foto { get; set; }
    public Guid? PaqueteEventoId { get; set; }
    public PaqueteEvento? PaqueteEvento { get; set; }
    public Guid? FotoPrivadaId { get; set; }
    public FotoPrivada? FotoPrivada { get; set; }

    public required string Descripcion { get; set; }
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; } = 1;
    public decimal Subtotal { get; set; }
    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
}
