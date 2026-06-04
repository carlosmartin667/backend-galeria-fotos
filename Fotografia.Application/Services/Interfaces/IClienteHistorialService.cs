using Fotografia.Application.DTOs.Clientes;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IClienteHistorialService
{
    Task<ApiResponse<ClienteHistorialResponseDto>> GetHistorialAsync(Guid clienteId, CancellationToken cancellationToken = default);
    Task<ApiResponse<ClienteHistorialResponseDto>> GetMiHistorialAsync(CancellationToken cancellationToken = default);
}
