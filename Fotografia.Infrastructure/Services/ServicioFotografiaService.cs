using AutoMapper;
using Fotografia.Application.DTOs.Servicios;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Services;

public sealed class ServicioFotografiaService(
    AppDbContext dbContext,
    IMapper mapper,
    ICurrentUserService currentUser) : IServicioFotografiaService
{
    public async Task<ApiResponse<IReadOnlyCollection<ServicioFotografiaResponseDto>>> GetPublicAsync(
        CancellationToken cancellationToken = default)
    {
        var servicios = await dbContext.ServiciosFotografia
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderByDescending(x => x.Destacado)
            .ThenBy(x => x.OrdenDestacado ?? int.MaxValue)
            .ThenBy(x => x.Orden)
            .ThenBy(x => x.Nombre)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<ServicioFotografiaResponseDto>>.Ok(
            mapper.Map<List<ServicioFotografiaResponseDto>>(servicios));
    }

    public async Task<ApiResponse<ServicioFotografiaResponseDto>> GetPublicByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var servicio = await dbContext.ServiciosFotografia
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.Activo, cancellationToken);

        return servicio is null
            ? ApiResponse<ServicioFotografiaResponseDto>.NotFound("Servicio no encontrado.")
            : ApiResponse<ServicioFotografiaResponseDto>.Ok(mapper.Map<ServicioFotografiaResponseDto>(servicio));
    }

    public async Task<ApiResponse<IReadOnlyCollection<ServicioFotografiaResponseDto>>> GetAdminAsync(
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<IReadOnlyCollection<ServicioFotografiaResponseDto>>.Forbidden("Solo un administrador puede consultar todos los servicios.");
        }

        var servicios = await dbContext.ServiciosFotografia
            .AsNoTracking()
            .OrderBy(x => x.Orden)
            .ThenBy(x => x.Nombre)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<ServicioFotografiaResponseDto>>.Ok(
            mapper.Map<List<ServicioFotografiaResponseDto>>(servicios));
    }

    public async Task<ApiResponse<ServicioFotografiaResponseDto>> CreateAsync(
        CrearServicioFotografiaRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<ServicioFotografiaResponseDto>.Forbidden("Solo un administrador puede crear servicios.");
        }

        var servicio = new ServicioFotografia
        {
            Nombre = request.Nombre.Trim(),
            Descripcion = Normalize(request.Descripcion),
            PrecioDesde = request.PrecioDesde,
            DuracionEstimada = Normalize(request.DuracionEstimada),
            CantidadFotosIncluidas = request.CantidadFotosIncluidas,
            ImagenUrl = Normalize(request.ImagenUrl),
            Activo = request.Activo,
            Destacado = request.Destacado,
            OrdenDestacado = request.OrdenDestacado,
            Orden = request.Orden,
            FechaCreacionUtc = DateTime.UtcNow
        };

        dbContext.ServiciosFotografia.Add(servicio);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<ServicioFotografiaResponseDto>.Ok(
            mapper.Map<ServicioFotografiaResponseDto>(servicio),
            "Servicio creado.");
    }

    public async Task<ApiResponse<ServicioFotografiaResponseDto>> UpdateAsync(
        Guid id,
        ActualizarServicioFotografiaRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<ServicioFotografiaResponseDto>.Forbidden("Solo un administrador puede editar servicios.");
        }

        var servicio = await dbContext.ServiciosFotografia.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (servicio is null)
        {
            return ApiResponse<ServicioFotografiaResponseDto>.NotFound("Servicio no encontrado.");
        }

        servicio.Nombre = request.Nombre.Trim();
        servicio.Descripcion = Normalize(request.Descripcion);
        servicio.PrecioDesde = request.PrecioDesde;
        servicio.DuracionEstimada = Normalize(request.DuracionEstimada);
        servicio.CantidadFotosIncluidas = request.CantidadFotosIncluidas;
        servicio.ImagenUrl = Normalize(request.ImagenUrl);
        servicio.Activo = request.Activo;
        servicio.Destacado = request.Destacado;
        servicio.OrdenDestacado = request.OrdenDestacado;
        servicio.Orden = request.Orden;
        servicio.FechaActualizacionUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<ServicioFotografiaResponseDto>.Ok(
            mapper.Map<ServicioFotografiaResponseDto>(servicio),
            "Servicio actualizado.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<bool>.Forbidden("Solo un administrador puede eliminar servicios.");
        }

        var servicio = await dbContext.ServiciosFotografia.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (servicio is null)
        {
            return ApiResponse<bool>.NotFound("Servicio no encontrado.");
        }

        servicio.Activo = false;
        servicio.FechaActualizacionUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Servicio desactivado.");
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
