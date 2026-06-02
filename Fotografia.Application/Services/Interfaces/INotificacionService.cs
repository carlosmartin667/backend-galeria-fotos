using Fotografia.Application.DTOs.Notificaciones;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface INotificacionService
{
    Task<ApiResponse<NotificacionResponseDto>> EnqueueAsync(
        EnqueueNotificacionRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<NotificacionResponseDto>> EnqueueFromTemplateAsync(
        EnqueueTemplateNotificacionRequestDto request,
        CancellationToken cancellationToken = default);

    Task<int> ProcesarPendientesAsync(CancellationToken cancellationToken = default);

    Task<ApiResponse<NotificacionResponseDto>> ReenviarAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<NotificacionResponseDto>> CancelarAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<NotificacionResponseDto>> MarcarLeidaAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<int>> MarcarTodasLeidasAsync(CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyCollection<NotificacionResponseDto>>> GetAdminAsync(
        NotificacionesAdminQueryDto query,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<NotificacionResponseDto>> GetAdminByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyCollection<NotificacionResponseDto>>> GetMisNotificacionesAsync(
        CancellationToken cancellationToken = default);
}
