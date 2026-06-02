using System.Net.Mail;
using Fotografia.Application.DTOs.Notificaciones;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Fotografia.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fotografia.Infrastructure.Services;

public sealed class NotificacionService(
    AppDbContext dbContext,
    IEmailService emailService,
    ICurrentUserService currentUser,
    IOptions<NotificationsSettings> options,
    ILogger<NotificacionService> logger) : INotificacionService
{
    private readonly NotificationsSettings _settings = options.Value;

    public async Task<ApiResponse<NotificacionResponseDto>> EnqueueAsync(
        EnqueueNotificacionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!NotificacionCanales.TryNormalize(request.Canal, out var canal))
        {
            return ApiResponse<NotificacionResponseDto>.Fail("Canal de notificacion invalido.");
        }

        if (canal == NotificacionCanales.Email && !IsValidEmail(request.DestinatarioEmail))
        {
            return ApiResponse<NotificacionResponseDto>.Fail("Destinatario de email invalido.");
        }

        if (request.UsuarioId is Guid usuarioId)
        {
            var exists = await dbContext.Usuarios.AnyAsync(x => x.Id == usuarioId, cancellationToken);
            if (!exists)
            {
                return ApiResponse<NotificacionResponseDto>.NotFound("Usuario destinatario no encontrado.");
            }
        }

        var correlationKey = Normalize(request.CorrelationKey);
        if (correlationKey is not null)
        {
            var existing = await QueryNotificaciones()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CorrelationKey == correlationKey && x.Activa, cancellationToken);

            if (existing is not null)
            {
                return ApiResponse<NotificacionResponseDto>.Ok(MapNotificacion(existing), "Notificacion ya encolada.");
            }
        }

        var now = DateTime.UtcNow;
        var isInternal = canal == NotificacionCanales.Interna;
        var maxIntentos = request.MaxIntentos is > 0 ? request.MaxIntentos.Value : Math.Max(1, _settings.Worker.MaxAttempts);
        var notificacion = new Notificacion
        {
            Tipo = NormalizeRequired(request.Tipo).ToUpperInvariant(),
            Canal = canal,
            Estado = isInternal ? NotificacionEstados.Enviada : NotificacionEstados.Pendiente,
            Titulo = NormalizeRequired(request.Titulo),
            Mensaje = NormalizeRequired(request.Mensaje),
            DestinatarioEmail = NormalizeEmail(request.DestinatarioEmail),
            UsuarioId = request.UsuarioId,
            EntidadTipo = Normalize(request.EntidadTipo),
            EntidadId = request.EntidadId,
            CorrelationKey = correlationKey,
            MaxIntentos = maxIntentos,
            ProgramadaParaUtc = request.ProgramadaParaUtc,
            EnviadaEnUtc = isInternal ? now : null,
            FechaCreacionUtc = now,
            Activa = true
        };

        dbContext.Notificaciones.Add(notificacion);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Notificacion encolada. NotificacionId={NotificacionId} Tipo={Tipo} Canal={Canal}",
            notificacion.Id,
            notificacion.Tipo,
            notificacion.Canal);

        var created = await QueryNotificaciones()
            .AsNoTracking()
            .FirstAsync(x => x.Id == notificacion.Id, cancellationToken);

        return ApiResponse<NotificacionResponseDto>.Ok(MapNotificacion(created), "Notificacion encolada.");
    }

    public async Task<ApiResponse<NotificacionResponseDto>> EnqueueFromTemplateAsync(
        EnqueueTemplateNotificacionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var codigo = NormalizeRequired(request.Codigo).ToUpperInvariant();
        var plantilla = await dbContext.PlantillasNotificacion
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Codigo == codigo && x.Activa, cancellationToken);

        if (plantilla is null)
        {
            return ApiResponse<NotificacionResponseDto>.NotFound("Plantilla de notificacion no encontrada o inactiva.");
        }

        var title = RenderTemplate(plantilla.Asunto, request.Reemplazos);
        var html = RenderTemplate(plantilla.CuerpoHtml, request.Reemplazos);

        return await EnqueueAsync(new EnqueueNotificacionRequestDto
        {
            Tipo = plantilla.Codigo,
            Canal = plantilla.Canal,
            Titulo = title,
            Mensaje = html,
            DestinatarioEmail = request.DestinatarioEmail,
            UsuarioId = request.UsuarioId,
            EntidadTipo = request.EntidadTipo,
            EntidadId = request.EntidadId,
            CorrelationKey = request.CorrelationKey,
            ProgramadaParaUtc = request.ProgramadaParaUtc
        }, cancellationToken);
    }

    public async Task<int> ProcesarPendientesAsync(CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            return 0;
        }

        var now = DateTime.UtcNow;
        var batchSize = Math.Clamp(_settings.Worker.BatchSize, 1, 100);
        var pendientes = await dbContext.Notificaciones
            .Where(x =>
                x.Activa
                && x.Canal == NotificacionCanales.Email
                && x.Estado == NotificacionEstados.Pendiente
                && x.Intentos < x.MaxIntentos
                && (x.ProgramadaParaUtc == null || x.ProgramadaParaUtc <= now))
            .OrderBy(x => x.FechaCreacionUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        var processed = 0;
        foreach (var notificacion in pendientes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            notificacion.Estado = NotificacionEstados.Procesando;
            notificacion.FechaActualizacionUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);

            try
            {
                if (!IsValidEmail(notificacion.DestinatarioEmail))
                {
                    throw new InvalidOperationException("Destinatario de email invalido.");
                }

                var result = await emailService.SendAsync(
                    notificacion.DestinatarioEmail!,
                    notificacion.Titulo,
                    notificacion.Mensaje,
                    cancellationToken);

                if (!result.Success)
                {
                    throw new InvalidOperationException(result.Message ?? "No se pudo enviar el email.");
                }

                notificacion.Estado = NotificacionEstados.Enviada;
                notificacion.EnviadaEnUtc = DateTime.UtcNow;
                notificacion.Error = null;
                logger.LogInformation("Notificacion enviada. NotificacionId={NotificacionId}", notificacion.Id);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                notificacion.Intentos++;
                notificacion.Error = SanitizeError(ex.Message);
                notificacion.Estado = notificacion.Intentos >= notificacion.MaxIntentos
                    ? NotificacionEstados.Error
                    : NotificacionEstados.Pendiente;

                logger.LogWarning(
                    "No se pudo enviar notificacion. NotificacionId={NotificacionId} Intentos={Intentos} Estado={Estado}",
                    notificacion.Id,
                    notificacion.Intentos,
                    notificacion.Estado);
            }

            notificacion.FechaActualizacionUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            processed++;
        }

        return processed;
    }

    public async Task<ApiResponse<NotificacionResponseDto>> ReenviarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<NotificacionResponseDto>.Forbidden("Solo un administrador puede reenviar notificaciones.");
        }

        var notificacion = await QueryNotificaciones().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (notificacion is null)
        {
            return ApiResponse<NotificacionResponseDto>.NotFound("Notificacion no encontrada.");
        }

        var now = DateTime.UtcNow;
        notificacion.Activa = true;
        notificacion.Intentos = 0;
        notificacion.Error = null;
        notificacion.ProgramadaParaUtc = null;
        notificacion.FechaActualizacionUtc = now;
        notificacion.Estado = notificacion.Canal == NotificacionCanales.Interna
            ? NotificacionEstados.Enviada
            : NotificacionEstados.Pendiente;
        notificacion.EnviadaEnUtc = notificacion.Canal == NotificacionCanales.Interna ? now : null;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<NotificacionResponseDto>.Ok(MapNotificacion(notificacion), "Notificacion reencolada.");
    }

    public async Task<ApiResponse<NotificacionResponseDto>> CancelarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<NotificacionResponseDto>.Forbidden("Solo un administrador puede cancelar notificaciones.");
        }

        var notificacion = await QueryNotificaciones().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (notificacion is null)
        {
            return ApiResponse<NotificacionResponseDto>.NotFound("Notificacion no encontrada.");
        }

        notificacion.Estado = NotificacionEstados.Cancelada;
        notificacion.Activa = false;
        notificacion.FechaActualizacionUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<NotificacionResponseDto>.Ok(MapNotificacion(notificacion), "Notificacion cancelada.");
    }

    public async Task<ApiResponse<NotificacionResponseDto>> MarcarLeidaAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return ApiResponse<NotificacionResponseDto>.Forbidden("Debe iniciar sesion.");
        }

        var notificacion = await QueryNotificaciones().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (notificacion is null)
        {
            return ApiResponse<NotificacionResponseDto>.NotFound("Notificacion no encontrada.");
        }

        if (!CanRead(notificacion))
        {
            return ApiResponse<NotificacionResponseDto>.Forbidden("No puede marcar notificaciones de otro usuario.");
        }

        notificacion.Leida = true;
        notificacion.FechaLecturaUtc = DateTime.UtcNow;
        notificacion.FechaActualizacionUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<NotificacionResponseDto>.Ok(MapNotificacion(notificacion), "Notificacion marcada como leida.");
    }

    public async Task<ApiResponse<int>> MarcarTodasLeidasAsync(CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is not Guid userId)
        {
            return ApiResponse<int>.Forbidden("Debe iniciar sesion.");
        }

        var query = dbContext.Notificaciones.Where(x => x.Activa && !x.Leida);
        query = currentUser.IsAdmin
            ? query.Where(x => x.UsuarioId == userId || (x.UsuarioId == null && x.Canal == NotificacionCanales.Interna))
            : query.Where(x => x.UsuarioId == userId);

        var notificaciones = await query.ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;
        foreach (var notificacion in notificaciones)
        {
            notificacion.Leida = true;
            notificacion.FechaLecturaUtc = now;
            notificacion.FechaActualizacionUtc = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<int>.Ok(notificaciones.Count, "Notificaciones marcadas como leidas.");
    }

    public async Task<ApiResponse<IReadOnlyCollection<NotificacionResponseDto>>> GetAdminAsync(
        NotificacionesAdminQueryDto query,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<IReadOnlyCollection<NotificacionResponseDto>>.Forbidden("Solo un administrador puede consultar notificaciones.");
        }

        var take = Math.Clamp(query.Take, 1, 500);
        var source = QueryNotificaciones().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Estado))
        {
            source = source.Where(x => x.Estado == query.Estado.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.Canal))
        {
            source = source.Where(x => x.Canal == query.Canal.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.Tipo))
        {
            source = source.Where(x => x.Tipo == query.Tipo.Trim());
        }

        if (query.Activa is not null)
        {
            source = source.Where(x => x.Activa == query.Activa.Value);
        }

        var notificaciones = await source
            .OrderByDescending(x => x.FechaCreacionUtc)
            .Take(take)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<NotificacionResponseDto>>.Ok(
            notificaciones.Select(MapNotificacion).ToList());
    }

    public async Task<ApiResponse<NotificacionResponseDto>> GetAdminByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<NotificacionResponseDto>.Forbidden("Solo un administrador puede consultar notificaciones.");
        }

        var notificacion = await QueryNotificaciones()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return notificacion is null
            ? ApiResponse<NotificacionResponseDto>.NotFound("Notificacion no encontrada.")
            : ApiResponse<NotificacionResponseDto>.Ok(MapNotificacion(notificacion));
    }

    public async Task<ApiResponse<IReadOnlyCollection<NotificacionResponseDto>>> GetMisNotificacionesAsync(
        CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is not Guid userId)
        {
            return ApiResponse<IReadOnlyCollection<NotificacionResponseDto>>.Forbidden("Debe iniciar sesion.");
        }

        var query = QueryNotificaciones()
            .AsNoTracking()
            .Where(x => x.Activa);

        query = currentUser.IsAdmin
            ? query.Where(x => x.UsuarioId == userId || (x.UsuarioId == null && x.Canal == NotificacionCanales.Interna))
            : query.Where(x => x.UsuarioId == userId);

        var notificaciones = await query
            .OrderByDescending(x => x.FechaCreacionUtc)
            .Take(100)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<NotificacionResponseDto>>.Ok(
            notificaciones.Select(MapNotificacion).ToList());
    }

    private IQueryable<Notificacion> QueryNotificaciones()
    {
        return dbContext.Notificaciones.Include(x => x.Usuario);
    }

    private bool CanRead(Notificacion notificacion)
    {
        return currentUser.IsAdmin
            || (currentUser.UserId is Guid userId && notificacion.UsuarioId == userId);
    }

    private static NotificacionResponseDto MapNotificacion(Notificacion notificacion)
    {
        return new NotificacionResponseDto
        {
            Id = notificacion.Id,
            Tipo = notificacion.Tipo,
            Canal = notificacion.Canal,
            Estado = notificacion.Estado,
            Titulo = notificacion.Titulo,
            Mensaje = notificacion.Mensaje,
            DestinatarioEmail = notificacion.DestinatarioEmail,
            UsuarioId = notificacion.UsuarioId,
            UsuarioNombre = notificacion.Usuario?.Nombre,
            EntidadTipo = notificacion.EntidadTipo,
            EntidadId = notificacion.EntidadId,
            CorrelationKey = notificacion.CorrelationKey,
            Intentos = notificacion.Intentos,
            MaxIntentos = notificacion.MaxIntentos,
            Error = notificacion.Error,
            Leida = notificacion.Leida,
            FechaLecturaUtc = notificacion.FechaLecturaUtc,
            ProgramadaParaUtc = notificacion.ProgramadaParaUtc,
            EnviadaEnUtc = notificacion.EnviadaEnUtc,
            FechaCreacionUtc = notificacion.FechaCreacionUtc,
            FechaActualizacionUtc = notificacion.FechaActualizacionUtc,
            Activa = notificacion.Activa
        };
    }

    private static string RenderTemplate(string template, IReadOnlyDictionary<string, string?> replacements)
    {
        var result = template;
        foreach (var replacement in replacements)
        {
            result = result.Replace(
                "{{" + replacement.Key + "}}",
                replacement.Value ?? string.Empty,
                StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }

    private static bool IsValidEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            var address = new MailAddress(value.Trim());
            return string.Equals(address.Address, value.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static string NormalizeRequired(string value)
    {
        return value.Trim();
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? NormalizeEmail(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
    }

    private static string SanitizeError(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "Error al procesar notificacion."
            : value.Length <= 1000
                ? value
                : value[..1000];
    }
}
