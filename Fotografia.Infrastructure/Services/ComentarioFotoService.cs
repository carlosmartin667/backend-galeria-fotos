using AutoMapper;
using Fotografia.Application.DTOs.Comentarios;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Services;

public sealed class ComentarioFotoService(
    AppDbContext dbContext,
    IMapper mapper,
    ICurrentUserService currentUser) : IComentarioFotoService
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
}
