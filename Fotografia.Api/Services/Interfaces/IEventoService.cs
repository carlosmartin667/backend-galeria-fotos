using Fotografia.Api.DTOs.Eventos;
using Fotografia.Api.Helpers;

namespace Fotografia.Api.Services.Interfaces;

public interface IEventoService
{
    Task<ApiResponse<IReadOnlyCollection<EventoResponseDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<EventoResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<EventoResponseDto>> CreateAsync(CrearEventoRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<EventoResponseDto>> UpdateAsync(Guid id, ActualizarEventoRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
