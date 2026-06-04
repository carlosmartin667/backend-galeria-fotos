using AutoMapper;
using Fotografia.Application.DTOs.Common;
using Fotografia.Application.DTOs.Favoritos;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Fotografia.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Services;

public sealed class FavoritoService(
    AppDbContext dbContext,
    IMapper mapper,
    ICurrentUserService currentUser,
    IResourceAccessService resourceAccessService) : IFavoritoService
{
    public async Task<ApiResponse<IReadOnlyCollection<FavoritoEventoResponseDto>>> GetEventosAsync(
        CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return ApiResponse<IReadOnlyCollection<FavoritoEventoResponseDto>>.Forbidden("Debe iniciar sesion.");
        }

        var favoritos = await ApplyEventoFavoriteAccess(dbContext.EventosFavoritos
            .AsNoTracking()
            .Include(x => x.Evento)
            .Where(x => x.UsuarioId == currentUser.UserId.Value))
            .OrderByDescending(x => x.FechaCreacionUtc)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<FavoritoEventoResponseDto>>.Ok(
            mapper.Map<List<FavoritoEventoResponseDto>>(favoritos));
    }

    public async Task<ApiResponse<PaginatedResponseDto<FavoritoEventoResponseDto>>> GetEventosPaginatedAsync(
        PaginationQueryDto pagination,
        CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return ApiResponse<PaginatedResponseDto<FavoritoEventoResponseDto>>.Forbidden("Debe iniciar sesion.");
        }

        var validationError = pagination.Validate();
        if (validationError is not null)
        {
            return ApiResponse<PaginatedResponseDto<FavoritoEventoResponseDto>>.Fail(validationError);
        }

        var query = ApplyEventoFavoriteAccess(dbContext.EventosFavoritos
            .AsNoTracking()
            .Include(x => x.Evento)
            .Where(x => x.UsuarioId == currentUser.UserId.Value))
            .OrderByDescending(x => x.FechaCreacionUtc);

        var paginated = await query.ToPaginatedResponseAsync(pagination, cancellationToken);

        return ApiResponse<PaginatedResponseDto<FavoritoEventoResponseDto>>.Ok(new PaginatedResponseDto<FavoritoEventoResponseDto>
        {
            Items = mapper.Map<List<FavoritoEventoResponseDto>>(paginated.Items),
            Page = paginated.Page,
            PageSize = paginated.PageSize,
            TotalItems = paginated.TotalItems,
            TotalPages = paginated.TotalPages,
            HasPreviousPage = paginated.HasPreviousPage,
            HasNextPage = paginated.HasNextPage,
            All = paginated.All
        });
    }

    public async Task<ApiResponse<FavoritoEventoResponseDto>> AddEventoAsync(
        Guid eventoId,
        CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return ApiResponse<FavoritoEventoResponseDto>.Forbidden("Debe iniciar sesion.");
        }

        if (!await resourceAccessService.CanAccessEventoAsync(eventoId, cancellationToken))
        {
            return ApiResponse<FavoritoEventoResponseDto>.NotFound("Evento no encontrado.");
        }

        var favorito = await dbContext.EventosFavoritos
            .Include(x => x.Evento)
            .FirstOrDefaultAsync(x => x.EventoId == eventoId && x.UsuarioId == currentUser.UserId.Value, cancellationToken);

        if (favorito is null)
        {
            favorito = new EventoFavorito
            {
                EventoId = eventoId,
                UsuarioId = currentUser.UserId.Value,
                FechaCreacionUtc = DateTime.UtcNow
            };

            dbContext.EventosFavoritos.Add(favorito);
            await dbContext.SaveChangesAsync(cancellationToken);

            favorito = await dbContext.EventosFavoritos
                .AsNoTracking()
                .Include(x => x.Evento)
                .FirstAsync(x => x.Id == favorito.Id, cancellationToken);
        }

        return ApiResponse<FavoritoEventoResponseDto>.Ok(mapper.Map<FavoritoEventoResponseDto>(favorito), "Evento favorito guardado.");
    }

    public async Task<ApiResponse<bool>> DeleteEventoAsync(Guid eventoId, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return ApiResponse<bool>.Forbidden("Debe iniciar sesion.");
        }

        var favorito = await dbContext.EventosFavoritos
            .FirstOrDefaultAsync(x => x.EventoId == eventoId && x.UsuarioId == currentUser.UserId.Value, cancellationToken);

        if (favorito is null)
        {
            return ApiResponse<bool>.NotFound("Favorito no encontrado.");
        }

        dbContext.EventosFavoritos.Remove(favorito);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Evento favorito eliminado.");
    }

    public async Task<ApiResponse<IReadOnlyCollection<FavoritoFotoResponseDto>>> GetFotosAsync(
        CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return ApiResponse<IReadOnlyCollection<FavoritoFotoResponseDto>>.Forbidden("Debe iniciar sesion.");
        }

        var favoritos = await ApplyFotoFavoriteAccess(dbContext.FotosFavoritas
            .AsNoTracking()
            .Include(x => x.Foto)
            .Where(x => x.UsuarioId == currentUser.UserId.Value))
            .OrderByDescending(x => x.FechaCreacionUtc)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<FavoritoFotoResponseDto>>.Ok(
            mapper.Map<List<FavoritoFotoResponseDto>>(favoritos));
    }

    public async Task<ApiResponse<PaginatedResponseDto<FavoritoFotoResponseDto>>> GetFotosPaginatedAsync(
        PaginationQueryDto pagination,
        CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return ApiResponse<PaginatedResponseDto<FavoritoFotoResponseDto>>.Forbidden("Debe iniciar sesion.");
        }

        var validationError = pagination.Validate();
        if (validationError is not null)
        {
            return ApiResponse<PaginatedResponseDto<FavoritoFotoResponseDto>>.Fail(validationError);
        }

        var query = ApplyFotoFavoriteAccess(dbContext.FotosFavoritas
            .AsNoTracking()
            .Include(x => x.Foto)
            .Where(x => x.UsuarioId == currentUser.UserId.Value))
            .OrderByDescending(x => x.FechaCreacionUtc);

        var paginated = await query.ToPaginatedResponseAsync(pagination, cancellationToken);

        return ApiResponse<PaginatedResponseDto<FavoritoFotoResponseDto>>.Ok(new PaginatedResponseDto<FavoritoFotoResponseDto>
        {
            Items = mapper.Map<List<FavoritoFotoResponseDto>>(paginated.Items),
            Page = paginated.Page,
            PageSize = paginated.PageSize,
            TotalItems = paginated.TotalItems,
            TotalPages = paginated.TotalPages,
            HasPreviousPage = paginated.HasPreviousPage,
            HasNextPage = paginated.HasNextPage,
            All = paginated.All
        });
    }

    public async Task<ApiResponse<FavoritoFotoResponseDto>> AddFotoAsync(
        Guid fotoId,
        CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return ApiResponse<FavoritoFotoResponseDto>.Forbidden("Debe iniciar sesion.");
        }

        if (!await resourceAccessService.CanAccessFotoAsync(fotoId, cancellationToken))
        {
            return ApiResponse<FavoritoFotoResponseDto>.NotFound("Foto no encontrada.");
        }

        var favorito = await dbContext.FotosFavoritas
            .Include(x => x.Foto)
            .FirstOrDefaultAsync(x => x.FotoId == fotoId && x.UsuarioId == currentUser.UserId.Value, cancellationToken);

        if (favorito is null)
        {
            favorito = new FotoFavorita
            {
                FotoId = fotoId,
                UsuarioId = currentUser.UserId.Value,
                FechaCreacionUtc = DateTime.UtcNow
            };

            dbContext.FotosFavoritas.Add(favorito);
            await dbContext.SaveChangesAsync(cancellationToken);

            favorito = await dbContext.FotosFavoritas
                .AsNoTracking()
                .Include(x => x.Foto)
                .FirstAsync(x => x.Id == favorito.Id, cancellationToken);
        }

        return ApiResponse<FavoritoFotoResponseDto>.Ok(mapper.Map<FavoritoFotoResponseDto>(favorito), "Foto favorita guardada.");
    }

    public async Task<ApiResponse<bool>> DeleteFotoAsync(Guid fotoId, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return ApiResponse<bool>.Forbidden("Debe iniciar sesion.");
        }

        var favorito = await dbContext.FotosFavoritas
            .FirstOrDefaultAsync(x => x.FotoId == fotoId && x.UsuarioId == currentUser.UserId.Value, cancellationToken);

        if (favorito is null)
        {
            return ApiResponse<bool>.NotFound("Favorito no encontrado.");
        }

        dbContext.FotosFavoritas.Remove(favorito);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Foto favorita eliminada.");
    }

    private IQueryable<EventoFavorito> ApplyEventoFavoriteAccess(IQueryable<EventoFavorito> query)
    {
        if (currentUser.IsAdmin)
        {
            return query;
        }

        if (currentUser.UserId is not Guid userId)
        {
            return query.Where(x => false);
        }

        return query.Where(x =>
            x.Evento != null
            && x.Evento.Activo
            && ((x.Evento.Visibilidad == EventoVisibilidades.Publico
                    && (x.Evento.Estado == EventoEstados.Publicado || x.Evento.Estado == EventoEstados.LegacyActivo))
                || x.Evento.CreadoPorUsuarioId == userId
                || (x.Evento.ClientePrincipal != null && x.Evento.ClientePrincipal.UsuarioId == userId)));
    }

    private IQueryable<FotoFavorita> ApplyFotoFavoriteAccess(IQueryable<FotoFavorita> query)
    {
        if (currentUser.IsAdmin)
        {
            return query.Where(x => x.Foto != null && x.Foto.Activa);
        }

        if (currentUser.UserId is not Guid userId)
        {
            return query.Where(x => false);
        }

        return query.Where(x =>
            x.Foto != null
            && x.Foto.Activa
            && x.Foto.Evento != null
            && x.Foto.Evento.Activo
            && ((x.Foto.Evento.Visibilidad == EventoVisibilidades.Publico
                    && (x.Foto.Evento.Estado == EventoEstados.Publicado || x.Foto.Evento.Estado == EventoEstados.LegacyActivo))
                || x.Foto.Evento.CreadoPorUsuarioId == userId
                || (x.Foto.Evento.ClientePrincipal != null && x.Foto.Evento.ClientePrincipal.UsuarioId == userId)));
    }
}
