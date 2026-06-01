using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Fotografia.Application.Security;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Fotografia.Infrastructure.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public Guid? UserId
    {
        get
        {
            var value = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User?.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }

    public string? Email => User?.FindFirstValue(ClaimTypes.Email)
        ?? User?.FindFirstValue(JwtRegisteredClaimNames.Email);

    public string? Rol => User?.FindFirstValue(ClaimTypes.Role);

    public bool IsAdmin => string.Equals(Rol, SistemaRoles.Admin, StringComparison.OrdinalIgnoreCase);

    public bool IsUsuario => string.Equals(Rol, SistemaRoles.Usuario, StringComparison.OrdinalIgnoreCase);

    public bool IsInvitado => !IsAuthenticated || string.Equals(Rol, SistemaRoles.Invitado, StringComparison.OrdinalIgnoreCase);
}
