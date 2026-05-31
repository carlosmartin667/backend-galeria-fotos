using Fotografia.Application.DTOs.Comentarios;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IComentarioFotoService
{
    Task<ApiResponse<IReadOnlyCollection<ComentarioResponseDto>>> GetByFotoAsync(Guid fotoId, CancellationToken cancellationToken = default);
    Task<ApiResponse<ComentarioResponseDto>> CreateAsync(Guid fotoId, ComentarioRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ComentarioResponseDto>> UpdateAsync(Guid comentarioId, ComentarioRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid comentarioId, CancellationToken cancellationToken = default);
}
