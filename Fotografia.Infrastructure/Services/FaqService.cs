using AutoMapper;
using Fotografia.Application.DTOs.Faq;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Services;

public sealed class FaqService(
    AppDbContext dbContext,
    IMapper mapper,
    ICurrentUserService currentUser) : IFaqService
{
    public async Task<ApiResponse<IReadOnlyCollection<PreguntaFrecuenteResponseDto>>> GetPublicAsync(
        CancellationToken cancellationToken = default)
    {
        var preguntas = await dbContext.PreguntasFrecuentes
            .AsNoTracking()
            .Where(x => x.Activa)
            .OrderBy(x => x.Orden)
            .ThenBy(x => x.Pregunta)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<PreguntaFrecuenteResponseDto>>.Ok(
            mapper.Map<List<PreguntaFrecuenteResponseDto>>(preguntas));
    }

    public async Task<ApiResponse<PreguntaFrecuenteResponseDto>> GetPublicByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var pregunta = await dbContext.PreguntasFrecuentes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.Activa, cancellationToken);

        return pregunta is null
            ? ApiResponse<PreguntaFrecuenteResponseDto>.NotFound("Pregunta frecuente no encontrada.")
            : ApiResponse<PreguntaFrecuenteResponseDto>.Ok(mapper.Map<PreguntaFrecuenteResponseDto>(pregunta));
    }

    public async Task<ApiResponse<IReadOnlyCollection<PreguntaFrecuenteResponseDto>>> GetAdminAsync(
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<IReadOnlyCollection<PreguntaFrecuenteResponseDto>>.Forbidden("Solo un administrador puede consultar todas las preguntas frecuentes.");
        }

        var preguntas = await dbContext.PreguntasFrecuentes
            .AsNoTracking()
            .OrderBy(x => x.Orden)
            .ThenBy(x => x.Pregunta)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<PreguntaFrecuenteResponseDto>>.Ok(
            mapper.Map<List<PreguntaFrecuenteResponseDto>>(preguntas));
    }

    public async Task<ApiResponse<PreguntaFrecuenteResponseDto>> CreateAsync(
        CrearPreguntaFrecuenteRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<PreguntaFrecuenteResponseDto>.Forbidden("Solo un administrador puede crear preguntas frecuentes.");
        }

        var pregunta = new PreguntaFrecuente
        {
            Pregunta = request.Pregunta.Trim(),
            Respuesta = request.Respuesta.Trim(),
            Categoria = Normalize(request.Categoria),
            Orden = request.Orden,
            Activa = request.Activa,
            FechaCreacionUtc = DateTime.UtcNow
        };

        dbContext.PreguntasFrecuentes.Add(pregunta);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<PreguntaFrecuenteResponseDto>.Ok(
            mapper.Map<PreguntaFrecuenteResponseDto>(pregunta),
            "Pregunta frecuente creada.");
    }

    public async Task<ApiResponse<PreguntaFrecuenteResponseDto>> UpdateAsync(
        Guid id,
        ActualizarPreguntaFrecuenteRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<PreguntaFrecuenteResponseDto>.Forbidden("Solo un administrador puede editar preguntas frecuentes.");
        }

        var pregunta = await dbContext.PreguntasFrecuentes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (pregunta is null)
        {
            return ApiResponse<PreguntaFrecuenteResponseDto>.NotFound("Pregunta frecuente no encontrada.");
        }

        pregunta.Pregunta = request.Pregunta.Trim();
        pregunta.Respuesta = request.Respuesta.Trim();
        pregunta.Categoria = Normalize(request.Categoria);
        pregunta.Orden = request.Orden;
        pregunta.Activa = request.Activa;
        pregunta.FechaActualizacionUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<PreguntaFrecuenteResponseDto>.Ok(
            mapper.Map<PreguntaFrecuenteResponseDto>(pregunta),
            "Pregunta frecuente actualizada.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<bool>.Forbidden("Solo un administrador puede eliminar preguntas frecuentes.");
        }

        var pregunta = await dbContext.PreguntasFrecuentes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (pregunta is null)
        {
            return ApiResponse<bool>.NotFound("Pregunta frecuente no encontrada.");
        }

        pregunta.Activa = false;
        pregunta.FechaActualizacionUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Pregunta frecuente desactivada.");
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
