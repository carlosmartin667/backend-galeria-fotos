using Fotografia.Application.DTOs.Auth;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Fotografia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        return result.Success ? Ok(result) : Unauthorized(result);
    }
}
