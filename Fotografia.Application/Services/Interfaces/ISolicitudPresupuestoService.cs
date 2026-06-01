using Fotografia.Application.DTOs.Presupuestos;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface ISolicitudPresupuestoService
{
    Task<ApiResponse<SolicitudPresupuestoResponseDto>> CreateAsync(CrearSolicitudPresupuestoRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyCollection<SolicitudPresupuestoResponseDto>>> GetAllAsync(bool? activa = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<SolicitudPresupuestoResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<SolicitudPresupuestoResponseDto>> UpdateAsync(Guid id, ActualizarSolicitudPresupuestoRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<SolicitudPresupuestoResponseDto>> ChangeEstadoAsync(Guid id, CambiarEstadoSolicitudPresupuestoRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
