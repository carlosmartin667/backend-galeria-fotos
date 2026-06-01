using Fotografia.Application.DTOs.Agenda;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IAgendaService
{
    Task<ApiResponse<IReadOnlyCollection<AgendaItemResponseDto>>> GetAllAsync(AgendaQueryDto query, CancellationToken cancellationToken = default);
    Task<ApiResponse<AgendaItemResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<AgendaItemResponseDto>> CreateAsync(CrearAgendaItemRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<AgendaItemResponseDto>> UpdateAsync(Guid id, ActualizarAgendaItemRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<DisponibilidadAgendaResponseDto>> GetDisponibilidadAsync(DateTime? desde, DateTime? hasta, CancellationToken cancellationToken = default);
}
