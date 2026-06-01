using Fotografia.Application.DTOs.Common;
using Fotografia.Application.DTOs.Eventos;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IEventoService
{
    Task<ApiResponse<IReadOnlyCollection<EventoResponseDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<PaginatedResponseDto<EventoResponseDto>>> GetPaginatedAsync(PaginationQueryDto pagination, CancellationToken cancellationToken = default);
    Task<ApiResponse<EventoResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<EventoResponseDto>> CreateAsync(CrearEventoRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<EventoResponseDto>> UpdateAsync(Guid id, ActualizarEventoRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
