using Fotografia.Application.DTOs.Favoritos;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IFavoritoService
{
    Task<ApiResponse<IReadOnlyCollection<FavoritoEventoResponseDto>>> GetEventosAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<FavoritoEventoResponseDto>> AddEventoAsync(Guid eventoId, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteEventoAsync(Guid eventoId, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyCollection<FavoritoFotoResponseDto>>> GetFotosAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<FavoritoFotoResponseDto>> AddFotoAsync(Guid fotoId, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteFotoAsync(Guid fotoId, CancellationToken cancellationToken = default);
}
