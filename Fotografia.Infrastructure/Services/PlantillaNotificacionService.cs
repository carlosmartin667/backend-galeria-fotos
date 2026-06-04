using Fotografia.Application.DTOs.Notificaciones;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Services;

public sealed class PlantillaNotificacionService(
    AppDbContext dbContext,
    ICurrentUserService currentUser) : IPlantillaNotificacionService
{
    public async Task<ApiResponse<IReadOnlyCollection<PlantillaNotificacionResponseDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<IReadOnlyCollection<PlantillaNotificacionResponseDto>>.Forbidden("Solo un administrador puede consultar plantillas.");
        }

        var plantillas = await dbContext.PlantillasNotificacion
            .AsNoTracking()
            .OrderBy(x => x.Codigo)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<PlantillaNotificacionResponseDto>>.Ok(
            plantillas.Select(MapPlantilla).ToList());
    }

    public async Task<ApiResponse<PlantillaNotificacionResponseDto>> CreateAsync(
        CrearPlantillaNotificacionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<PlantillaNotificacionResponseDto>.Forbidden("Solo un administrador puede crear plantillas.");
        }

        if (!NotificacionCanales.TryNormalize(request.Canal, out var canal))
        {
            return ApiResponse<PlantillaNotificacionResponseDto>.Fail("Canal de notificacion invalido.");
        }

        var codigo = NormalizeRequired(request.Codigo).ToUpperInvariant();
        var exists = await dbContext.PlantillasNotificacion.AnyAsync(x => x.Codigo == codigo, cancellationToken);
        if (exists)
        {
            return ApiResponse<PlantillaNotificacionResponseDto>.Fail("Ya existe una plantilla con ese codigo.");
        }

        var plantilla = new PlantillaNotificacion
        {
            Codigo = codigo,
            Canal = canal,
            Asunto = NormalizeRequired(request.Asunto),
            CuerpoHtml = NormalizeRequired(request.CuerpoHtml),
            CuerpoTexto = Normalize(request.CuerpoTexto),
            Activa = request.Activa,
            FechaCreacionUtc = DateTime.UtcNow
        };

        dbContext.PlantillasNotificacion.Add(plantilla);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<PlantillaNotificacionResponseDto>.Ok(MapPlantilla(plantilla), "Plantilla creada.");
    }

    public async Task<ApiResponse<PlantillaNotificacionResponseDto>> UpdateAsync(
        Guid id,
        ActualizarPlantillaNotificacionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<PlantillaNotificacionResponseDto>.Forbidden("Solo un administrador puede editar plantillas.");
        }

        if (!NotificacionCanales.TryNormalize(request.Canal, out var canal))
        {
            return ApiResponse<PlantillaNotificacionResponseDto>.Fail("Canal de notificacion invalido.");
        }

        var plantilla = await dbContext.PlantillasNotificacion.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (plantilla is null)
        {
            return ApiResponse<PlantillaNotificacionResponseDto>.NotFound("Plantilla no encontrada.");
        }

        plantilla.Canal = canal;
        plantilla.Asunto = NormalizeRequired(request.Asunto);
        plantilla.CuerpoHtml = NormalizeRequired(request.CuerpoHtml);
        plantilla.CuerpoTexto = Normalize(request.CuerpoTexto);
        plantilla.Activa = request.Activa;
        plantilla.FechaActualizacionUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<PlantillaNotificacionResponseDto>.Ok(MapPlantilla(plantilla), "Plantilla actualizada.");
    }

    public Task<ApiResponse<PlantillaNotificacionResponseDto>> ActivarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return SetActivaAsync(id, true, "Plantilla activada.", cancellationToken);
    }

    public Task<ApiResponse<PlantillaNotificacionResponseDto>> DesactivarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return SetActivaAsync(id, false, "Plantilla desactivada.", cancellationToken);
    }

    private async Task<ApiResponse<PlantillaNotificacionResponseDto>> SetActivaAsync(
        Guid id,
        bool activa,
        string message,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<PlantillaNotificacionResponseDto>.Forbidden("Solo un administrador puede cambiar plantillas.");
        }

        var plantilla = await dbContext.PlantillasNotificacion.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (plantilla is null)
        {
            return ApiResponse<PlantillaNotificacionResponseDto>.NotFound("Plantilla no encontrada.");
        }

        plantilla.Activa = activa;
        plantilla.FechaActualizacionUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<PlantillaNotificacionResponseDto>.Ok(MapPlantilla(plantilla), message);
    }

    private static PlantillaNotificacionResponseDto MapPlantilla(PlantillaNotificacion plantilla)
    {
        return new PlantillaNotificacionResponseDto
        {
            Id = plantilla.Id,
            Codigo = plantilla.Codigo,
            Canal = plantilla.Canal,
            Asunto = plantilla.Asunto,
            CuerpoHtml = plantilla.CuerpoHtml,
            CuerpoTexto = plantilla.CuerpoTexto,
            Activa = plantilla.Activa,
            FechaCreacionUtc = plantilla.FechaCreacionUtc,
            FechaActualizacionUtc = plantilla.FechaActualizacionUtc
        };
    }

    private static string NormalizeRequired(string value)
    {
        return value.Trim();
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
