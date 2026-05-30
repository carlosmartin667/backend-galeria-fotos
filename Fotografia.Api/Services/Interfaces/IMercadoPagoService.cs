using Fotografia.Api.DTOs.Pagos;
using Fotografia.Api.Helpers;

namespace Fotografia.Api.Services.Interfaces;

public interface IMercadoPagoService
{
    Task<ApiResponse<MercadoPagoPreferenceResponseDto>> CreateCheckoutPreferenceAsync(Guid pedidoId, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagoResponseDto>> ProcessWebhookAsync(MercadoPagoWebhookDto request, CancellationToken cancellationToken = default);
}
