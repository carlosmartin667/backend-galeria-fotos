using Fotografia.Application.DTOs.Sitio;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IPerfilFotografaService
{
    Task<ApiResponse<PerfilFotografaResponseDto>> GetPublicoAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<PerfilFotografaResponseDto>> GetAdminAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<PerfilFotografaResponseDto>> UpdateAsync(ActualizarPerfilFotografaRequestDto request, CancellationToken cancellationToken = default);
}
