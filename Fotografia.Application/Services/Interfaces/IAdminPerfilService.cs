using Fotografia.Application.DTOs.Admin;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IAdminPerfilService
{
    Task<ApiResponse<AdminPerfilPublicoResponseDto>> GetPerfilPublicoAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<AdminPerfilResponseDto>> GetMiPerfilAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<AdminPerfilResponseDto>> UpdateMiPerfilAsync(ActualizarAdminPerfilRequestDto request, CancellationToken cancellationToken = default);
}
