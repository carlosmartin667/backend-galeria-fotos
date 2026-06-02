using Fotografia.Application.DTOs.NotasInternas;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface INotaInternaService
{
    Task<ApiResponse<IReadOnlyCollection<NotaInternaResponseDto>>> GetByEntidadAsync(
        string entidadTipo,
        Guid entidadId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<NotaInternaResponseDto>> CreateAsync(
        string entidadTipo,
        Guid entidadId,
        CrearNotaInternaRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<NotaInternaResponseDto>> UpdateAsync(
        Guid id,
        ActualizarNotaInternaRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
