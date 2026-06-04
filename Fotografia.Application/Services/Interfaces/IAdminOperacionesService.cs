using Fotografia.Application.DTOs.Admin;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IAdminOperacionesService
{
    Task<ApiResponse<AdminOperacionesResumenDto>> GetResumenAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<AdminOperacionesPendientesDto>> GetPendientesAsync(CancellationToken cancellationToken = default);
}
