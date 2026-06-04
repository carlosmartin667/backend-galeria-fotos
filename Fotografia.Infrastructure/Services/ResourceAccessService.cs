using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Services;

public sealed class ResourceAccessService(
    AppDbContext dbContext,
    ICurrentUserService currentUser) : IResourceAccessService
{
    public Task<bool> CanAccessEventoAsync(Guid eventoId, CancellationToken cancellationToken = default)
    {
        return ApplyEventoAccess(dbContext.Eventos.AsNoTracking().Where(x => x.Id == eventoId))
            .AnyAsync(cancellationToken);
    }

    public Task<bool> CanAccessFotoAsync(Guid fotoId, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Fotos.AsNoTracking().Where(x => x.Id == fotoId && x.Activa);

        if (currentUser.IsAdmin)
        {
            return query.AnyAsync(cancellationToken);
        }

        if (currentUser.IsAuthenticated && currentUser.UserId is Guid userId)
        {
            return query.AnyAsync(x =>
                x.Evento != null
                && x.Evento.Activo
                && ((x.Evento.Visibilidad == EventoVisibilidades.Publico
                        && (x.Evento.Estado == EventoEstados.Publicado || x.Evento.Estado == EventoEstados.LegacyActivo))
                    || x.Evento.CreadoPorUsuarioId == userId
                    || (x.Evento.ClientePrincipal != null && x.Evento.ClientePrincipal.UsuarioId == userId)),
                cancellationToken);
        }

        return query.AnyAsync(x =>
            x.Evento != null
            && x.Evento.Activo
            && x.Evento.Visibilidad == EventoVisibilidades.Publico
            && (x.Evento.Estado == EventoEstados.Publicado || x.Evento.Estado == EventoEstados.LegacyActivo),
            cancellationToken);
    }

    public Task<bool> CanAccessPaqueteEventoAsync(Guid paqueteId, CancellationToken cancellationToken = default)
    {
        var query = dbContext.PaquetesEvento.AsNoTracking().Where(x => x.Id == paqueteId && x.Activo);

        if (currentUser.IsAdmin)
        {
            return query.AnyAsync(cancellationToken);
        }

        if (currentUser.IsAuthenticated && currentUser.UserId is Guid userId)
        {
            return query.AnyAsync(x =>
                x.Evento != null
                && x.Evento.Activo
                && ((x.Evento.Visibilidad == EventoVisibilidades.Publico
                        && (x.Evento.Estado == EventoEstados.Publicado || x.Evento.Estado == EventoEstados.LegacyActivo))
                    || x.Evento.CreadoPorUsuarioId == userId
                    || (x.Evento.ClientePrincipal != null && x.Evento.ClientePrincipal.UsuarioId == userId)),
                cancellationToken);
        }

        return query.AnyAsync(x =>
            x.Evento != null
            && x.Evento.Activo
            && x.Evento.Visibilidad == EventoVisibilidades.Publico
            && (x.Evento.Estado == EventoEstados.Publicado || x.Evento.Estado == EventoEstados.LegacyActivo),
            cancellationToken);
    }

    private IQueryable<Evento> ApplyEventoAccess(IQueryable<Evento> query)
    {
        if (currentUser.IsAdmin)
        {
            return query;
        }

        if (currentUser.IsAuthenticated && currentUser.UserId is Guid userId)
        {
            return query.Where(x =>
                x.Activo
                && ((x.Visibilidad == EventoVisibilidades.Publico
                        && (x.Estado == EventoEstados.Publicado || x.Estado == EventoEstados.LegacyActivo))
                    || x.CreadoPorUsuarioId == userId
                    || (x.ClientePrincipal != null && x.ClientePrincipal.UsuarioId == userId)));
        }

        return query.Where(x =>
            x.Activo
            && x.Visibilidad == EventoVisibilidades.Publico
            && (x.Estado == EventoEstados.Publicado || x.Estado == EventoEstados.LegacyActivo));
    }
}
