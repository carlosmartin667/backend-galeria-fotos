using AutoMapper;
using Fotografia.Application.DTOs.Paquetes;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Services;

public sealed class PaqueteEventoService(AppDbContext dbContext, IMapper mapper, ICurrentUserService currentUser) : IPaqueteEventoService
{
    public async Task<ApiResponse<IReadOnlyCollection<PaqueteEventoResponseDto>>> GetByEventoAsync(
        Guid eventoId,
        CancellationToken cancellationToken = default)
    {
        var eventoExists = await CanAccessEventoAsync(eventoId, cancellationToken);
        if (!eventoExists)
        {
            return ApiResponse<IReadOnlyCollection<PaqueteEventoResponseDto>>.NotFound("Evento no encontrado.");
        }

        var paquetes = await dbContext.PaquetesEvento
            .AsNoTracking()
            .Where(x => x.EventoId == eventoId && x.Activo)
            .OrderByDescending(x => x.Destacado)
            .ThenBy(x => x.OrdenDestacado ?? int.MaxValue)
            .ThenBy(x => x.Precio)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<PaqueteEventoResponseDto>>.Ok(
            mapper.Map<List<PaqueteEventoResponseDto>>(paquetes));
    }

    public async Task<ApiResponse<PaqueteEventoResponseDto>> CreateAsync(
        Guid eventoId,
        CrearPaqueteEventoRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var eventoExists = await dbContext.Eventos.AnyAsync(x => x.Id == eventoId, cancellationToken);
        if (!eventoExists)
        {
            return ApiResponse<PaqueteEventoResponseDto>.NotFound("Evento no encontrado.");
        }

        var paquete = new PaqueteEvento
        {
            EventoId = eventoId,
            Nombre = request.Nombre.Trim(),
            Descripcion = Normalize(request.Descripcion),
            Precio = request.Precio,
            IncluyeTodasLasFotos = request.IncluyeTodasLasFotos,
            Activo = request.Activo,
            Destacado = request.Destacado,
            OrdenDestacado = request.OrdenDestacado,
            FechaCreacionUtc = DateTime.UtcNow
        };

        dbContext.PaquetesEvento.Add(paquete);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<PaqueteEventoResponseDto>.Ok(
            mapper.Map<PaqueteEventoResponseDto>(paquete),
            "Paquete creado.");
    }

    public async Task<ApiResponse<PaqueteEventoResponseDto>> UpdateAsync(
        Guid paqueteId,
        ActualizarPaqueteEventoRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var paquete = await dbContext.PaquetesEvento.FirstOrDefaultAsync(x => x.Id == paqueteId, cancellationToken);
        if (paquete is null)
        {
            return ApiResponse<PaqueteEventoResponseDto>.NotFound("Paquete no encontrado.");
        }

        paquete.Nombre = request.Nombre.Trim();
        paquete.Descripcion = Normalize(request.Descripcion);
        paquete.Precio = request.Precio;
        paquete.IncluyeTodasLasFotos = request.IncluyeTodasLasFotos;
        paquete.Activo = request.Activo;
        paquete.Destacado = request.Destacado;
        paquete.OrdenDestacado = request.OrdenDestacado;
        paquete.FechaActualizacionUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<PaqueteEventoResponseDto>.Ok(
            mapper.Map<PaqueteEventoResponseDto>(paquete),
            "Paquete actualizado.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid paqueteId, CancellationToken cancellationToken = default)
    {
        var paquete = await dbContext.PaquetesEvento.FirstOrDefaultAsync(x => x.Id == paqueteId, cancellationToken);
        if (paquete is null)
        {
            return ApiResponse<bool>.NotFound("Paquete no encontrado.");
        }

        paquete.Activo = false;
        paquete.FechaActualizacionUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Paquete desactivado.");
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private async Task<bool> CanAccessEventoAsync(Guid eventoId, CancellationToken cancellationToken)
    {
        var query = dbContext.Eventos.AsNoTracking().Where(x => x.Id == eventoId);

        if (currentUser.IsAdmin)
        {
            return await query.AnyAsync(cancellationToken);
        }

        if (currentUser.IsAuthenticated && currentUser.UserId is Guid userId)
        {
            return await query.AnyAsync(x =>
                x.Activo
                && ((x.Visibilidad == EventoVisibilidades.Publico
                        && (x.Estado == EventoEstados.Publicado || x.Estado == EventoEstados.LegacyActivo))
                    || x.CreadoPorUsuarioId == userId
                    || (x.ClientePrincipal != null && x.ClientePrincipal.UsuarioId == userId)),
                cancellationToken);
        }

        return await query.AnyAsync(x =>
            x.Activo
            && x.Visibilidad == EventoVisibilidades.Publico
            && (x.Estado == EventoEstados.Publicado || x.Estado == EventoEstados.LegacyActivo),
            cancellationToken);
    }
}
