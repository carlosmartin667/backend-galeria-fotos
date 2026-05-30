using Fotografia.Application.DTOs.Pagos;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fotografia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PagosController(IMercadoPagoService mercadoPagoService) : ControllerBase
{
    [Authorize]
    [HttpPost("checkout-pro/preferencias")]
    public async Task<IActionResult> CreateCheckoutPreference(CrearPreferenciaPagoRequestDto request, CancellationToken cancellationToken)
    {
        var result = await mercadoPagoService.CreateCheckoutPreferenceAsync(request.PedidoId, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [AllowAnonymous]
    [HttpPost("webhooks/mercado-pago")]
    public async Task<IActionResult> MercadoPagoWebhook(MercadoPagoWebhookDto request, CancellationToken cancellationToken)
    {
        var result = await mercadoPagoService.ProcessWebhookAsync(request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
