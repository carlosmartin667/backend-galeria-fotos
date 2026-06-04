using Fotografia.Application.DTOs.Servicios;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IServicioFotografiaService
{
    Task<ApiResponse<IReadOnlyCollection<ServicioFotografiaResponseDto>>> GetPublicAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<ServicioFotografiaResponseDto>> GetPublicByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyCollection<ServicioFotografiaResponseDto>>> GetAdminAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<ServicioFotografiaResponseDto>> CreateAsync(CrearServicioFotografiaRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ServicioFotografiaResponseDto>> UpdateAsync(Guid id, ActualizarServicioFotografiaRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
