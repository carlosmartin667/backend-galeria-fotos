using Fotografia.Application.DTOs.Cupones;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface ICuponService
{
    Task<ApiResponse<IReadOnlyCollection<CuponDescuentoResponseDto>>> GetAdminAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<CuponDescuentoResponseDto>> GetAdminByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<CuponDescuentoResponseDto>> CreateAsync(CrearCuponDescuentoRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CuponDescuentoResponseDto>> UpdateAsync(Guid id, ActualizarCuponDescuentoRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<CuponDescuentoResponseDto>> ActivarAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<CuponDescuentoResponseDto>> DesactivarAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyCollection<CuponUsoResponseDto>>> GetUsosAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<CuponValidacionResponseDto>> ValidarAsync(ValidarCuponRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CuponValidacionResponseDto>> ValidarParaClienteAsync(string codigo, decimal subtotal, Guid clienteId, Guid? usuarioId, CancellationToken cancellationToken = default);
    Task<ApiResponse<CuponUsoResponseDto>> RegistrarUsoPendienteAsync(Guid cuponDescuentoId, Guid pedidoId, Guid clienteId, Guid? usuarioId, string codigo, decimal montoDescuento, CancellationToken cancellationToken = default);
    Task ConfirmarUsoPorPedidoAsync(Guid pedidoId, CancellationToken cancellationToken = default);
}
