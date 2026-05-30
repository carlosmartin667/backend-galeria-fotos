using Fotografia.Application.DTOs.Pagos;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IMercadoPagoService
{
    Task<ApiResponse<MercadoPagoPreferenceResponseDto>> CreateCheckoutPreferenceAsync(Guid pedidoId, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagoResponseDto>> ProcessWebhookAsync(MercadoPagoWebhookDto request, CancellationToken cancellationToken = default);
}
