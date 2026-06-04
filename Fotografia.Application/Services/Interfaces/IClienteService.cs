using Fotografia.Application.DTOs.Clientes;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IClienteService
{
    Task<ApiResponse<IReadOnlyCollection<ClienteResponseDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<ClienteResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<ClienteResponseDto>> CreateAsync(CrearClienteRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ClienteResponseDto>> UpdateAsync(Guid id, ActualizarClienteRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
