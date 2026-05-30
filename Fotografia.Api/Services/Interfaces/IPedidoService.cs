using Fotografia.Api.DTOs.Pedidos;
using Fotografia.Api.Helpers;

namespace Fotografia.Api.Services.Interfaces;

public interface IPedidoService
{
    Task<ApiResponse<IReadOnlyCollection<PedidoResponseDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<PedidoResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<PedidoResponseDto>> CreateAsync(CrearPedidoRequestDto request, CancellationToken cancellationToken = default);
}
