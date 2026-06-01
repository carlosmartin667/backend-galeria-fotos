namespace Fotografia.Domain.Entities;

public sealed class Pago
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PedidoId { get; set; }
    public Pedido? Pedido { get; set; }

    public string? MercadoPagoPaymentId { get; set; }
    public string? MercadoPagoPreferenceId { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public decimal Monto { get; set; }
    public string Moneda { get; set; } = "ARS";
    public DateTime CreadoEnUtc { get; set; } = DateTime.UtcNow;
    public DateTime? PagadoEnUtc { get; set; }
    public DateTime? ActualizadoEnUtc { get; set; }
}
