using Fotografia.Application.DTOs.Notificaciones;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IPlantillaNotificacionService
{
    Task<ApiResponse<IReadOnlyCollection<PlantillaNotificacionResponseDto>>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PlantillaNotificacionResponseDto>> CreateAsync(
        CrearPlantillaNotificacionRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PlantillaNotificacionResponseDto>> UpdateAsync(
        Guid id,
        ActualizarPlantillaNotificacionRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PlantillaNotificacionResponseDto>> ActivarAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<PlantillaNotificacionResponseDto>> DesactivarAsync(Guid id, CancellationToken cancellationToken = default);
}
