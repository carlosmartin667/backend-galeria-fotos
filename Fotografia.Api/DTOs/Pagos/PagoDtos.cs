using System.ComponentModel.DataAnnotations;

namespace Fotografia.Api.DTOs.Pagos;

public sealed class CrearPreferenciaPagoRequestDto
{
    [Required]
    public Guid PedidoId { get; set; }
}

public sealed class MercadoPagoPreferenceResponseDto
{
    public string PreferenceId { get; set; } = string.Empty;
    public string CheckoutUrl { get; set; } = string.Empty;
    public string? SandboxCheckoutUrl { get; set; }
}

public sealed class MercadoPagoWebhookDto
{
    public string? Action { get; set; }
    public string? Type { get; set; }
    public MercadoPagoWebhookDataDto? Data { get; set; }
}

public sealed class MercadoPagoWebhookDataDto
{
    public string? Id { get; set; }
}

public sealed class PagoResponseDto
{
    public Guid Id { get; set; }
    public Guid PedidoId { get; set; }
    public string? MercadoPagoPaymentId { get; set; }
    public string? MercadoPagoPreferenceId { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public DateTime? PagadoEnUtc { get; set; }
}
