using Fotografia.Application.DTOs.Auth;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
}
