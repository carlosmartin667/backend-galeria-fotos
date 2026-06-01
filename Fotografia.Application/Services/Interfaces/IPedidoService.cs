using Fotografia.Application.DTOs.Common;
using Fotografia.Application.DTOs.Pedidos;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IPedidoService
{
    Task<ApiResponse<IReadOnlyCollection<PedidoResponseDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<PaginatedResponseDto<PedidoResponseDto>>> GetPaginatedAsync(PaginationQueryDto pagination, CancellationToken cancellationToken = default);
    Task<ApiResponse<PedidoResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<PedidoResponseDto>> CreateAsync(CrearPedidoRequestDto request, CancellationToken cancellationToken = default);
}
