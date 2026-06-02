using AutoMapper;
using Fotografia.Application.DTOs.Comentarios;
using Fotografia.Application.DTOs.Notificaciones;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fotografia.Infrastructure.Services;

public sealed class ComentarioFotoService(
    AppDbContext dbContext,
    IMapper mapper,
    ICurrentUserService currentUser,
    INotificacionService notificacionService,
    ILogger<ComentarioFotoService> logger) : IComentarioFotoService
{
    public async Task<ApiResponse<IReadOnlyCollection<ComentarioResponseDto>>> GetByFotoAsync(
        Guid fotoId,
        CancellationToken cancellationToken = default)
    {
        var exists = await dbContext.Fotos.AnyAsync(x => x.Id == fotoId && x.Activa, cancellationToken);
        if (!exists)
        {
            return ApiResponse<IReadOnlyCollection<ComentarioResponseDto>>.NotFound("Foto no encontrada.");
        }

        var comentarios = await QueryComentarios()
            .Where(x => x.FotoId == fotoId && x.Activo)
            .OrderBy(x => x.FechaCreacionUtc)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<ComentarioResponseDto>>.Ok(
            mapper.Map<List<ComentarioResponseDto>>(comentarios));
    }

    public async Task<ApiResponse<ComentarioResponseDto>> CreateAsync(
        Guid fotoId,
        ComentarioRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return ApiResponse<ComentarioResponseDto>.Forbidden("Debe iniciar sesion para comentar.");
        }

        if (string.IsNullOrWhiteSpace(request.Texto))
        {
            return ApiResponse<ComentarioResponseDto>.Fail("El texto del comentario es requerido.");
        }

        var exists = await dbContext.Fotos.AnyAsync(x => x.Id == fotoId && x.Activa, cancellationToken);
        if (!exists)
        {
            return ApiResponse<ComentarioResponseDto>.NotFound("Foto no encontrada.");
        }

        var comentario = new ComentarioFoto
        {
            FotoId = fotoId,
            UsuarioId = currentUser.UserId.Value,
            Texto = request.Texto.Trim(),
            FechaCreacionUtc = DateTime.UtcNow
        };

        dbContext.ComentariosFotos.Add(comentario);
        await dbContext.SaveChangesAsync(cancellationToken);

        await TryEnqueueComentarioAdminAsync(comentario, cancellationToken);

        var created = await QueryComentarios()
            .AsNoTracking()
            .FirstAsync(x => x.Id == comentario.Id, cancellationToken);

        return ApiResponse<ComentarioResponseDto>.Ok(mapper.Map<ComentarioResponseDto>(created), "Comentario creado.");
    }

    public async Task<ApiResponse<ComentarioResponseDto>> UpdateAsync(
        Guid comentarioId,
        ComentarioRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var comentario = await dbContext.ComentariosFotos
            .Include(x => x.Usuario)
            .FirstOrDefaultAsync(x => x.Id == comentarioId && x.Activo, cancellationToken);

        if (comentario is null)
        {
            return ApiResponse<ComentarioResponseDto>.NotFound("Comentario no encontrado.");
        }

        if (string.IsNullOrWhiteSpace(request.Texto))
        {
            return ApiResponse<ComentarioResponseDto>.Fail("El texto del comentario es requerido.");
        }

        if (!CanModify(comentario.UsuarioId))
        {
            return ApiResponse<ComentarioResponseDto>.Forbidden("No puede editar un comentario de otro usuario.");
        }

        comentario.Texto = request.Texto.Trim();
        comentario.FechaActualizacionUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<ComentarioResponseDto>.Ok(mapper.Map<ComentarioResponseDto>(comentario), "Comentario actualizado.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid comentarioId, CancellationToken cancellationToken = default)
    {
        var comentario = await dbContext.ComentariosFotos
            .FirstOrDefaultAsync(x => x.Id == comentarioId && x.Activo, cancellationToken);

        if (comentario is null)
        {
            return ApiResponse<bool>.NotFound("Comentario no encontrado.");
        }

        if (!CanModify(comentario.UsuarioId))
        {
            return ApiResponse<bool>.Forbidden("No puede eliminar un comentario de otro usuario.");
        }

        comentario.Activo = false;
        comentario.FechaActualizacionUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Comentario eliminado.");
    }

    private IQueryable<ComentarioFoto> QueryComentarios()
    {
        return dbContext.ComentariosFotos
            .AsNoTracking()
            .Include(x => x.Usuario);
    }

    private bool CanModify(Guid usuarioId)
    {
        return currentUser.IsAdmin || currentUser.UserId == usuarioId;
    }

    private async Task TryEnqueueComentarioAdminAsync(
        ComentarioFoto comentario,
        CancellationToken cancellationToken)
    {
        var foto = await dbContext.Fotos
            .AsNoTracking()
            .Include(x => x.Evento)
            .FirstOrDefaultAsync(x => x.Id == comentario.FotoId, cancellationToken);

        await TryEnqueueTemplateAsync(new EnqueueTemplateNotificacionRequestDto
        {
            Codigo = NotificacionTipos.NuevoComentarioAdmin,
            EntidadTipo = "ComentarioFoto",
            EntidadId = comentario.Id,
            CorrelationKey = $"comentario-foto:{comentario.Id}:admin",
            Reemplazos = new Dictionary<string, string?>
            {
                ["NombreCliente"] = currentUser.Email ?? "Usuario",
                ["EmailCliente"] = currentUser.Email,
                ["NombreEvento"] = foto?.Evento?.Nombre ?? foto?.NombreArchivo,
                ["PedidoId"] = string.Empty,
                ["Total"] = string.Empty,
                ["Estado"] = "NuevoComentario",
                ["Link"] = string.Empty,
                ["NombreFotografa"] = "Fotografa",
                ["Fecha"] = comentario.FechaCreacionUtc.ToString("yyyy-MM-dd")
            }
        }, cancellationToken);
    }

    private async Task TryEnqueueTemplateAsync(
        EnqueueTemplateNotificacionRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await notificacionService.EnqueueFromTemplateAsync(request, cancellationToken);
            if (!result.Success)
            {
                logger.LogWarning(
                    "No se pudo encolar notificacion de comentario de foto. Codigo={Codigo} EntidadId={EntidadId} Motivo={Motivo}",
                    request.Codigo,
                    request.EntidadId,
                    result.Message);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(
                ex,
                "No se pudo encolar notificacion de comentario de foto. Codigo={Codigo} EntidadId={EntidadId}",
                request.Codigo,
                request.EntidadId);
        }
    }
}
