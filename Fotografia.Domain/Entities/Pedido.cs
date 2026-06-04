namespace Fotografia.Domain.Entities;

public sealed class Pedido
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? EventoId { get; set; }
    public Evento? Evento { get; set; }

    public Guid ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public string Estado { get; set; } = "Pendiente";
    public decimal Subtotal { get; set; }
    public decimal DescuentoTotal { get; set; }
    public decimal Total { get; set; }
    public string Moneda { get; set; } = "ARS";
    public string? MercadoPagoPreferenceId { get; set; }
    public string? CuponCodigo { get; set; }
    public Guid? CuponDescuentoId { get; set; }
    public CuponDescuento? CuponDescuento { get; set; }
    public DateTime CreadoEnUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ActualizadoEnUtc { get; set; }

    public ICollection<PedidoFoto> PedidoFotos { get; set; } = [];
    public ICollection<PedidoItem> PedidoItems { get; set; } = [];
    public Pago? Pago { get; set; }
    public ICollection<Descarga> Descargas { get; set; } = [];
    public ICollection<PedidoEstadoHistorial> HistorialEstados { get; set; } = [];
    public ICollection<CuponUso> CuponUsos { get; set; } = [];
}
