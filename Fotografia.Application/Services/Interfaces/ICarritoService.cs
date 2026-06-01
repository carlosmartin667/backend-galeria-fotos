using Fotografia.Application.DTOs.Carrito;
using Fotografia.Application.DTOs.Pedidos;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface ICarritoService
{
    Task<ApiResponse<CarritoResponseDto>> GetActivoAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<CarritoResponseDto>> AddFotoEventoAsync(Guid fotoId, CancellationToken cancellationToken = default);
    Task<ApiResponse<CarritoResponseDto>> AddPaqueteEventoAsync(Guid paqueteId, CancellationToken cancellationToken = default);
    Task<ApiResponse<CarritoResponseDto>> AddFotoPrivadaAsync(Guid fotoPrivadaId, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteItemAsync(Guid itemId, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> VaciarAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<PedidoResponseDto>> CrearPedidoAsync(CancellationToken cancellationToken = default);
}
