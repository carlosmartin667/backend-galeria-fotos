using Fotografia.Application.DTOs.Testimonios;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface ITestimonioService
{
    Task<ApiResponse<IReadOnlyCollection<TestimonioPublicoResponseDto>>> GetPublicAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyCollection<TestimonioPublicoResponseDto>>> GetDestacadosAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<TestimonioAdminResponseDto>> CreateAsync(CrearTestimonioRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyCollection<TestimonioAdminResponseDto>>> GetAdminAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<TestimonioAdminResponseDto>> GetAdminByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<TestimonioAdminResponseDto>> UpdateAdminAsync(Guid id, ActualizarTestimonioAdminRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<TestimonioAdminResponseDto>> PublicarAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<TestimonioAdminResponseDto>> OcultarAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
