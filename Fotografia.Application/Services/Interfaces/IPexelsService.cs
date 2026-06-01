using Fotografia.Application.DTOs.Pexels;

namespace Fotografia.Application.Services.Interfaces;

public interface IPexelsService
{
    Task<IReadOnlyList<PexelsPhotoDto>> BuscarFotosAsync(
        string query,
        int cantidad,
        CancellationToken cancellationToken = default);
}
