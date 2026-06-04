using Fotografia.Application.DTOs.Faq;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IFaqService
{
    Task<ApiResponse<IReadOnlyCollection<PreguntaFrecuenteResponseDto>>> GetPublicAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<PreguntaFrecuenteResponseDto>> GetPublicByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyCollection<PreguntaFrecuenteResponseDto>>> GetAdminAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<PreguntaFrecuenteResponseDto>> CreateAsync(CrearPreguntaFrecuenteRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PreguntaFrecuenteResponseDto>> UpdateAsync(Guid id, ActualizarPreguntaFrecuenteRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
