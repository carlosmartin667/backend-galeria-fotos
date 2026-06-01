using AutoMapper;
using Fotografia.Application.DTOs.Sitio;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Services;

public sealed class PerfilFotografaService(
    AppDbContext dbContext,
    IMapper mapper,
    ICurrentUserService currentUser) : IPerfilFotografaService
{
    public async Task<ApiResponse<PerfilFotografaResponseDto>> GetPublicoAsync(CancellationToken cancellationToken = default)
    {
        var perfil = await QueryPrincipal().AsNoTracking().FirstOrDefaultAsync(cancellationToken);

        return perfil is null
            ? ApiResponse<PerfilFotografaResponseDto>.NotFound("Perfil de fotografa no encontrado.")
            : ApiResponse<PerfilFotografaResponseDto>.Ok(mapper.Map<PerfilFotografaResponseDto>(perfil));
    }

    public async Task<ApiResponse<PerfilFotografaResponseDto>> GetAdminAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<PerfilFotografaResponseDto>.Forbidden("Solo un administrador puede consultar este perfil.");
        }

        var perfil = await dbContext.PerfilesFotografa
            .AsNoTracking()
            .OrderByDescending(x => x.Activa)
            .ThenBy(x => x.FechaCreacionUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return perfil is null
            ? ApiResponse<PerfilFotografaResponseDto>.NotFound("Perfil de fotografa no encontrado.")
            : ApiResponse<PerfilFotografaResponseDto>.Ok(mapper.Map<PerfilFotografaResponseDto>(perfil));
    }

    public async Task<ApiResponse<PerfilFotografaResponseDto>> UpdateAsync(
        ActualizarPerfilFotografaRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<PerfilFotografaResponseDto>.Forbidden("Solo un administrador puede editar este perfil.");
        }

        var perfil = await dbContext.PerfilesFotografa
            .OrderByDescending(x => x.Activa)
            .ThenBy(x => x.FechaCreacionUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (perfil is null)
        {
            perfil = new PerfilFotografa
            {
                Nombre = request.Nombre.Trim(),
                FechaCreacionUtc = DateTime.UtcNow
            };

            dbContext.PerfilesFotografa.Add(perfil);
        }

        if (request.Activa)
        {
            var otrosActivos = await dbContext.PerfilesFotografa
                .Where(x => x.Id != perfil.Id && x.Activa)
                .ToListAsync(cancellationToken);

            foreach (var otro in otrosActivos)
            {
                otro.Activa = false;
                otro.FechaActualizacionUtc = DateTime.UtcNow;
            }
        }

        perfil.Nombre = request.Nombre.Trim();
        perfil.Titulo = Normalize(request.Titulo);
        perfil.Descripcion = Normalize(request.Descripcion);
        perfil.Biografia = Normalize(request.Biografia);
        perfil.WhatsApp = Normalize(request.WhatsApp);
        perfil.Instagram = Normalize(request.Instagram);
        perfil.Facebook = Normalize(request.Facebook);
        perfil.TikTok = Normalize(request.TikTok);
        perfil.SitioWeb = Normalize(request.SitioWeb);
        perfil.CorreoPublico = Normalize(request.CorreoPublico)?.ToLowerInvariant();
        perfil.Direccion = Normalize(request.Direccion);
        perfil.Ciudad = Normalize(request.Ciudad);
        perfil.Provincia = Normalize(request.Provincia);
        perfil.Pais = Normalize(request.Pais);
        perfil.FotoPerfilUrl = Normalize(request.FotoPerfilUrl);
        perfil.LogoUrl = Normalize(request.LogoUrl);
        perfil.BannerUrl = Normalize(request.BannerUrl);
        perfil.TextoBienvenida = Normalize(request.TextoBienvenida);
        perfil.Activa = request.Activa;
        perfil.FechaActualizacionUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<PerfilFotografaResponseDto>.Ok(
            mapper.Map<PerfilFotografaResponseDto>(perfil),
            "Perfil de fotografa actualizado.");
    }

    private IQueryable<PerfilFotografa> QueryPrincipal()
    {
        return dbContext.PerfilesFotografa
            .Where(x => x.Activa)
            .OrderBy(x => x.FechaCreacionUtc);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
