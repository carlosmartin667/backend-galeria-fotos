using Fotografia.Application.DTOs.Bitacora;
using Fotografia.Application.DTOs.Common;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IBitacoraService
{
    Task RegistrarAsync(CrearBitacoraRequestDto request, CancellationToken cancellationToken = default);
    Task RegistrarInfoAsync(string accion, string entidadTipo, Guid? entidadId, string descripcion, object? metadata = null, CancellationToken cancellationToken = default);
    Task RegistrarWarningAsync(string accion, string entidadTipo, Guid? entidadId, string descripcion, object? metadata = null, CancellationToken cancellationToken = default);
    Task RegistrarErrorAsync(string accion, string entidadTipo, Guid? entidadId, string descripcion, object? metadata = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<PaginatedResponseDto<BitacoraResponseDto>>> GetAdminAsync(BitacoraQueryDto query, CancellationToken cancellationToken = default);
    Task<ApiResponse<BitacoraResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<BitacoraResumenDto>> GetResumenAsync(CancellationToken cancellationToken = default);
}
