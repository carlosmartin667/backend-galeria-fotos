using Fotografia.Application.DTOs.CarritosAbandonados;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface ICarritoAbandonadoService
{
    Task<ApiResponse<IReadOnlyCollection<CarritoAbandonadoResponseDto>>> GetAdminAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<CarritosAbandonadosResumenDto>> GetResumenAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyCollection<CarritoAbandonadoResponseDto>>> DetectarAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<CarritoAbandonadoResponseDto>> NotificarAsync(Guid id, CancellationToken cancellationToken = default);
    Task MarcarRecuperadoPorCarritoAsync(Guid carritoCompraId, CancellationToken cancellationToken = default);
}
