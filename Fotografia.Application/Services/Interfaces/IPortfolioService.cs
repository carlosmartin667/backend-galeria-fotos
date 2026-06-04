using Fotografia.Application.DTOs.Portfolio;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IPortfolioService
{
    Task<ApiResponse<IReadOnlyCollection<PortfolioItemResponseDto>>> GetPublicAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<PortfolioItemResponseDto>> GetPublicByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyCollection<PortfolioItemResponseDto>>> GetAdminAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<PortfolioItemResponseDto>> CreateAsync(CrearPortfolioItemRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PortfolioItemResponseDto>> UpdateAsync(Guid id, ActualizarPortfolioItemRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
