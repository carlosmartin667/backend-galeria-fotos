using Fotografia.Application.DTOs.Comentarios;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IComentarioEventoService
{
    Task<ApiResponse<IReadOnlyCollection<ComentarioResponseDto>>> GetByEventoAsync(Guid eventoId, CancellationToken cancellationToken = default);
    Task<ApiResponse<ComentarioResponseDto>> CreateAsync(Guid eventoId, ComentarioRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ComentarioResponseDto>> UpdateAsync(Guid comentarioId, ComentarioRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid comentarioId, CancellationToken cancellationToken = default);
}
