using Fotografia.Application.DTOs.Admin;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IAdminVentasService
{
    Task<ApiResponse<AdminVentasResumenDto>> GetResumenAsync(CancellationToken cancellationToken = default);
}
