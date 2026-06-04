using Fotografia.Application.DTOs.Paquetes;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IPaqueteEventoService
{
    Task<ApiResponse<IReadOnlyCollection<PaqueteEventoResponseDto>>> GetByEventoAsync(Guid eventoId, CancellationToken cancellationToken = default);
    Task<ApiResponse<PaqueteEventoResponseDto>> CreateAsync(Guid eventoId, CrearPaqueteEventoRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PaqueteEventoResponseDto>> UpdateAsync(Guid paqueteId, ActualizarPaqueteEventoRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid paqueteId, CancellationToken cancellationToken = default);
}
