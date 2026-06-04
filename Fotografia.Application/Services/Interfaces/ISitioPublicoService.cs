using Fotografia.Application.DTOs.Sitio;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface ISitioPublicoService
{
    Task<ApiResponse<SitioHomeResponseDto>> GetHomeAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<SitioContactoResponseDto>> GetContactoAsync(CancellationToken cancellationToken = default);
}
