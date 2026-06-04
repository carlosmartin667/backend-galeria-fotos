namespace Fotografia.Application.Services.Interfaces;

public interface IResourceAccessService
{
    Task<bool> CanAccessEventoAsync(Guid eventoId, CancellationToken cancellationToken = default);
    Task<bool> CanAccessFotoAsync(Guid fotoId, CancellationToken cancellationToken = default);
    Task<bool> CanAccessPaqueteEventoAsync(Guid paqueteId, CancellationToken cancellationToken = default);
}
