using Fotografia.Application.DTOs.Admin;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IAdminDashboardService
{
    Task<ApiResponse<AdminDashboardResponseDto>> GetDashboardAsync(CancellationToken cancellationToken = default);
}
