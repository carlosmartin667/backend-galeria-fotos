namespace Fotografia.Application.Services.Interfaces;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }
    string? Email { get; }
    string? Rol { get; }
    bool IsAdmin { get; }
    bool IsUsuario { get; }
    bool IsInvitado { get; }
}
