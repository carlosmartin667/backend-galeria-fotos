using Fotografia.Application.DTOs.Promociones;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IPromocionService
{
    Task<ApiResponse<IReadOnlyCollection<PromocionResponseDto>>> GetPublicAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<PromocionResponseDto>> GetPublicByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyCollection<PromocionResponseDto>>> GetAdminAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<PromocionResponseDto>> CreateAsync(CrearPromocionRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PromocionResponseDto>> UpdateAsync(Guid id, ActualizarPromocionRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<PromocionResponseDto>> ActivarAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<PromocionResponseDto>> DesactivarAsync(Guid id, CancellationToken cancellationToken = default);
}
