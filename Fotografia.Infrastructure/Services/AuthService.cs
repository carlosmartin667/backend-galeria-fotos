using AutoMapper;
using Fotografia.Infrastructure.Data;
using Fotografia.Application.DTOs.Auth;
using Fotografia.Domain.Entities;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Infrastructure.Security;
using Fotografia.Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Fotografia.Infrastructure.Services;

public sealed class AuthService(
    AppDbContext dbContext,
    IMapper mapper,
    JwtHelper jwtHelper,
    IOptions<JwtSettings> jwtOptions) : IAuthService
{
    private readonly PasswordHasher<Usuario> _passwordHasher = new();
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var exists = await dbContext.Usuarios.AnyAsync(x => x.Email == normalizedEmail, cancellationToken);
        if (exists)
        {
            return ApiResponse<AuthResponseDto>.Fail("Ya existe un usuario con ese email.");
        }

        var usuario = new Usuario
        {
            Nombre = request.Nombre.Trim(),
            Email = normalizedEmail,
            PasswordHash = string.Empty
        };
        usuario.PasswordHash = _passwordHasher.HashPassword(usuario, request.Password);

        dbContext.Usuarios.Add(usuario);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<AuthResponseDto>.Ok(CreateAuthResponse(usuario));
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var usuario = await dbContext.Usuarios.FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
        if (usuario is null || !usuario.Activo)
        {
            return ApiResponse<AuthResponseDto>.Fail("Credenciales invalidas.");
        }

        var verification = _passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return ApiResponse<AuthResponseDto>.Fail("Credenciales invalidas.");
        }

        return ApiResponse<AuthResponseDto>.Ok(CreateAuthResponse(usuario));
    }

    private AuthResponseDto CreateAuthResponse(Usuario usuario)
    {
        return new AuthResponseDto
        {
            Token = jwtHelper.GenerateToken(usuario),
            ExpiraEnUtc = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            Usuario = mapper.Map<UsuarioResponseDto>(usuario)
        };
    }
}
