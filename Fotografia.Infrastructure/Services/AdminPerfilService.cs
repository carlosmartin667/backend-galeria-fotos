using AutoMapper;
using Fotografia.Application.DTOs.Admin;
using Fotografia.Application.Helpers;
using Fotografia.Application.Security;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Services;

public sealed class AdminPerfilService(
    AppDbContext dbContext,
    IMapper mapper,
    ICurrentUserService currentUser,
    IPerfilFotografaService perfilFotografaService) : IAdminPerfilService
{
    public async Task<ApiResponse<AdminPerfilPublicoResponseDto>> GetPerfilPublicoAsync(CancellationToken cancellationToken = default)
    {
        var perfilFotografa = await perfilFotografaService.GetPublicoAsync(cancellationToken);
        if (perfilFotografa.Success && perfilFotografa.Data is not null)
        {
            return ApiResponse<AdminPerfilPublicoResponseDto>.Ok(new AdminPerfilPublicoResponseDto
            {
                Nombre = perfilFotografa.Data.Nombre,
                Descripcion = perfilFotografa.Data.Descripcion,
                WhatsApp = perfilFotografa.Data.WhatsApp,
                Instagram = perfilFotografa.Data.Instagram,
                CorreoPublico = perfilFotografa.Data.CorreoPublico,
                Direccion = perfilFotografa.Data.Direccion
            });
        }

        var admin = await QueryAdmin()
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        return admin is null
            ? ApiResponse<AdminPerfilPublicoResponseDto>.NotFound("Perfil publico no encontrado.")
            : ApiResponse<AdminPerfilPublicoResponseDto>.Ok(mapper.Map<AdminPerfilPublicoResponseDto>(admin));
    }

    public async Task<ApiResponse<AdminPerfilResponseDto>> GetMiPerfilAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin || currentUser.UserId is null)
        {
            return ApiResponse<AdminPerfilResponseDto>.Forbidden("No tiene permiso para consultar este perfil.");
        }

        var admin = await dbContext.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == currentUser.UserId.Value && x.Rol == SistemaRoles.Admin, cancellationToken);

        return admin is null
            ? ApiResponse<AdminPerfilResponseDto>.NotFound("Administrador no encontrado.")
            : ApiResponse<AdminPerfilResponseDto>.Ok(mapper.Map<AdminPerfilResponseDto>(admin));
    }

    public async Task<ApiResponse<AdminPerfilResponseDto>> UpdateMiPerfilAsync(
        ActualizarAdminPerfilRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin || currentUser.UserId is null)
        {
            return ApiResponse<AdminPerfilResponseDto>.Forbidden("No tiene permiso para editar este perfil.");
        }

        var admin = await dbContext.Usuarios
            .FirstOrDefaultAsync(x => x.Id == currentUser.UserId.Value && x.Rol == SistemaRoles.Admin, cancellationToken);

        if (admin is null)
        {
            return ApiResponse<AdminPerfilResponseDto>.NotFound("Administrador no encontrado.");
        }

        admin.Nombre = request.Nombre.Trim();
        admin.Descripcion = Normalize(request.Descripcion);
        admin.WhatsApp = Normalize(request.WhatsApp);
        admin.Instagram = Normalize(request.Instagram);
        admin.CorreoPublico = Normalize(request.CorreoPublico)?.ToLowerInvariant();
        admin.Direccion = Normalize(request.Direccion);
        admin.ActualizadoEnUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<AdminPerfilResponseDto>.Ok(mapper.Map<AdminPerfilResponseDto>(admin), "Perfil actualizado.");
    }

    private IQueryable<Usuario> QueryAdmin()
    {
        return dbContext.Usuarios
            .Where(x => x.Activo && x.Rol == SistemaRoles.Admin)
            .OrderBy(x => x.CreadoEnUtc);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
