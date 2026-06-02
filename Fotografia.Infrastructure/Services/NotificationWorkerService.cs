using Fotografia.Application.Services.Interfaces;
using Fotografia.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fotografia.Infrastructure.Services;

public sealed class NotificationWorkerService(
    IServiceScopeFactory scopeFactory,
    IOptions<NotificationsSettings> options,
    ILogger<NotificationWorkerService> logger) : BackgroundService
{
    private readonly NotificationsSettings _settings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_settings.Enabled && _settings.WorkerEnabled)
                {
                    await using var scope = scopeFactory.CreateAsyncScope();
                    var service = scope.ServiceProvider.GetRequiredService<INotificacionService>();
                    var processed = await service.ProcesarPendientesAsync(stoppingToken);
                    if (processed > 0)
                    {
                        logger.LogInformation("Worker de notificaciones proceso {Cantidad} notificaciones.", processed);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Worker de notificaciones fallo en un ciclo de procesamiento.");
            }

            var delaySeconds = Math.Clamp(_settings.Worker.IntervalSeconds, 5, 3600);
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
        }
    }
}
