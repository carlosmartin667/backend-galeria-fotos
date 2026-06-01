using Fotografia.Api.Helpers;
using Fotografia.Application.DTOs.Pagos;
using Fotografia.Application.Security;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fotografia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PagosController(IMercadoPagoService mercadoPagoService) : ControllerBase
{
    [Authorize(Roles = SistemaRoles.AdminUsuario)]
    [HttpPost("checkout-pro/preferencias")]
    public async Task<IActionResult> CreateCheckoutPreference(CrearPreferenciaPagoRequestDto request, CancellationToken cancellationToken)
    {
        var result = await mercadoPagoService.CreateCheckoutPreferenceAsync(request.PedidoId, cancellationToken);
        return this.ToActionResult(result);
    }

    [AllowAnonymous]
    [HttpPost("webhooks/mercado-pago")]
    public async Task<IActionResult> MercadoPagoWebhook(MercadoPagoWebhookDto request, CancellationToken cancellationToken)
    {
        var result = await mercadoPagoService.ProcessWebhookAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }
}
