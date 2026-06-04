using Fotografia.Application.DTOs.Notificaciones;
using Fotografia.Application.DTOs.Promociones;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fotografia.Infrastructure.Services;

public sealed class PromocionService(
    AppDbContext dbContext,
    ICurrentUserService currentUser,
    IBitacoraService bitacoraService,
    INotificacionService notificacionService,
    ILogger<PromocionService> logger) : IPromocionService
{
    public async Task<ApiResponse<IReadOnlyCollection<PromocionResponseDto>>> GetPublicAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var promociones = await QueryPromociones()
            .AsNoTracking()
            .Where(x => x.Activa
                && (x.FechaInicioUtc == null || x.FechaInicioUtc <= now)
                && (x.FechaFinUtc == null || x.FechaFinUtc >= now))
            .OrderByDescending(x => x.Destacada)
            .ThenBy(x => x.Orden)
            .ThenByDescending(x => x.FechaCreacionUtc)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<PromocionResponseDto>>.Ok(promociones.Select(Map).ToList());
    }

    public async Task<ApiResponse<PromocionResponseDto>> GetPublicByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var promocion = await QueryPromociones()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id
                && x.Activa
                && (x.FechaInicioUtc == null || x.FechaInicioUtc <= now)
                && (x.FechaFinUtc == null || x.FechaFinUtc >= now),
                cancellationToken);

        return promocion is null
            ? ApiResponse<PromocionResponseDto>.NotFound("Promocion no encontrada.")
            : ApiResponse<PromocionResponseDto>.Ok(Map(promocion));
    }

    public async Task<ApiResponse<IReadOnlyCollection<PromocionResponseDto>>> GetAdminAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<IReadOnlyCollection<PromocionResponseDto>>.Forbidden("Solo un administrador puede consultar promociones.");
        }

        var promociones = await QueryPromociones()
            .AsNoTracking()
            .OrderByDescending(x => x.FechaCreacionUtc)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<PromocionResponseDto>>.Ok(promociones.Select(Map).ToList());
    }

    public async Task<ApiResponse<PromocionResponseDto>> CreateAsync(CrearPromocionRequestDto request, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<PromocionResponseDto>.Forbidden("Solo un administrador puede crear promociones.");
        }

        var validation = await ValidateAsync(request, cancellationToken);
        if (validation is not null)
        {
            return ApiResponse<PromocionResponseDto>.Fail(validation);
        }

        PromocionTipos.TryNormalize(request.Tipo, out var tipo);
        var promocion = new Promocion
        {
            Titulo = request.Titulo.Trim(),
            Descripcion = Normalize(request.Descripcion),
            ImagenUrl = Normalize(request.ImagenUrl),
            Tipo = tipo,
            FechaInicioUtc = request.FechaInicioUtc,
            FechaFinUtc = request.FechaFinUtc,
            Activa = request.Activa,
            Destacada = request.Destacada,
            Orden = request.Orden,
            CuponDescuentoId = request.CuponDescuentoId,
            ServicioFotografiaId = request.ServicioFotografiaId,
            EventoId = request.EventoId,
            FechaCreacionUtc = DateTime.UtcNow
        };

        dbContext.Promociones.Add(promocion);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (promocion.Activa)
        {
            await RegistrarPromocionActivadaAsync(promocion, cancellationToken);
            await TryEnqueuePromocionActivaAdminAsync(promocion, cancellationToken);
        }

        var created = await QueryPromociones().AsNoTracking().FirstAsync(x => x.Id == promocion.Id, cancellationToken);
        return ApiResponse<PromocionResponseDto>.Ok(Map(created), "Promocion creada.");
    }

    public async Task<ApiResponse<PromocionResponseDto>> UpdateAsync(Guid id, ActualizarPromocionRequestDto request, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<PromocionResponseDto>.Forbidden("Solo un administrador puede editar promociones.");
        }

        var validation = await ValidateAsync(request, cancellationToken);
        if (validation is not null)
        {
            return ApiResponse<PromocionResponseDto>.Fail(validation);
        }

        var promocion = await dbContext.Promociones.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (promocion is null)
        {
            return ApiResponse<PromocionResponseDto>.NotFound("Promocion no encontrada.");
        }

        var wasInactive = !promocion.Activa;
        PromocionTipos.TryNormalize(request.Tipo, out var tipo);
        promocion.Titulo = request.Titulo.Trim();
        promocion.Descripcion = Normalize(request.Descripcion);
        promocion.ImagenUrl = Normalize(request.ImagenUrl);
        promocion.Tipo = tipo;
        promocion.FechaInicioUtc = request.FechaInicioUtc;
        promocion.FechaFinUtc = request.FechaFinUtc;
        promocion.Activa = request.Activa;
        promocion.Destacada = request.Destacada;
        promocion.Orden = request.Orden;
        promocion.CuponDescuentoId = request.CuponDescuentoId;
        promocion.ServicioFotografiaId = request.ServicioFotografiaId;
        promocion.EventoId = request.EventoId;
        promocion.FechaActualizacionUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        if (wasInactive && promocion.Activa)
        {
            await RegistrarPromocionActivadaAsync(promocion, cancellationToken);
            await TryEnqueuePromocionActivaAdminAsync(promocion, cancellationToken);
        }

        var updated = await QueryPromociones().AsNoTracking().FirstAsync(x => x.Id == promocion.Id, cancellationToken);
        return ApiResponse<PromocionResponseDto>.Ok(Map(updated), "Promocion actualizada.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<bool>.Forbidden("Solo un administrador puede desactivar promociones.");
        }

        var promocion = await dbContext.Promociones.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (promocion is null)
        {
            return ApiResponse<bool>.NotFound("Promocion no encontrada.");
        }

        promocion.Activa = false;
        promocion.FechaActualizacionUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Promocion desactivada.");
    }

    public Task<ApiResponse<PromocionResponseDto>> ActivarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return SetActivoAsync(id, true, "Promocion activada.", cancellationToken);
    }

    public Task<ApiResponse<PromocionResponseDto>> DesactivarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return SetActivoAsync(id, false, "Promocion desactivada.", cancellationToken);
    }

    private async Task<ApiResponse<PromocionResponseDto>> SetActivoAsync(Guid id, bool activa, string message, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<PromocionResponseDto>.Forbidden("Solo un administrador puede cambiar promociones.");
        }

        var promocion = await dbContext.Promociones.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (promocion is null)
        {
            return ApiResponse<PromocionResponseDto>.NotFound("Promocion no encontrada.");
        }

        var wasInactive = !promocion.Activa;
        promocion.Activa = activa;
        promocion.FechaActualizacionUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        if (wasInactive && activa)
        {
            await RegistrarPromocionActivadaAsync(promocion, cancellationToken);
            await TryEnqueuePromocionActivaAdminAsync(promocion, cancellationToken);
        }

        var updated = await QueryPromociones().AsNoTracking().FirstAsync(x => x.Id == id, cancellationToken);
        return ApiResponse<PromocionResponseDto>.Ok(Map(updated), message);
    }

    private async Task<string?> ValidateAsync(CrearPromocionRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo))
        {
            return "Titulo es requerido.";
        }

        if (!PromocionTipos.TryNormalize(request.Tipo, out _))
        {
            return "Tipo de promocion invalido.";
        }

        if (request.FechaInicioUtc is not null && request.FechaFinUtc is not null && request.FechaFinUtc <= request.FechaInicioUtc)
        {
            return "FechaFinUtc debe ser mayor a FechaInicioUtc.";
        }

        if (request.CuponDescuentoId is Guid cuponId
            && !await dbContext.CuponesDescuento.AnyAsync(x => x.Id == cuponId, cancellationToken))
        {
            return "Cupon asociado no encontrado.";
        }

        if (request.ServicioFotografiaId is Guid servicioId
            && !await dbContext.ServiciosFotografia.AnyAsync(x => x.Id == servicioId, cancellationToken))
        {
            return "Servicio asociado no encontrado.";
        }

        if (request.EventoId is Guid eventoId
            && !await dbContext.Eventos.AnyAsync(x => x.Id == eventoId, cancellationToken))
        {
            return "Evento asociado no encontrado.";
        }

        return null;
    }

    private IQueryable<Promocion> QueryPromociones()
    {
        return dbContext.Promociones
            .Include(x => x.CuponDescuento)
            .Include(x => x.ServicioFotografia)
            .Include(x => x.Evento);
    }

    private async Task TryEnqueuePromocionActivaAdminAsync(Promocion promocion, CancellationToken cancellationToken)
    {
        try
        {
            var result = await notificacionService.EnqueueFromTemplateAsync(new EnqueueTemplateNotificacionRequestDto
            {
                Codigo = NotificacionTipos.PromocionActivaAdmin,
                EntidadTipo = "Promocion",
                EntidadId = promocion.Id,
                CorrelationKey = $"promocion:{promocion.Id}:activa:admin",
                Reemplazos = new Dictionary<string, string?>
                {
                    ["Titulo"] = promocion.Titulo,
                    ["Tipo"] = promocion.Tipo,
                    ["Fecha"] = DateTime.UtcNow.ToString("yyyy-MM-dd")
                }
            }, cancellationToken);

            if (!result.Success)
            {
                logger.LogWarning("No se pudo encolar notificacion de promocion activa. PromocionId={PromocionId} Motivo={Motivo}", promocion.Id, result.Message);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "No se pudo encolar notificacion de promocion activa. PromocionId={PromocionId}", promocion.Id);
        }
    }

    private Task RegistrarPromocionActivadaAsync(Promocion promocion, CancellationToken cancellationToken)
    {
        return bitacoraService.RegistrarInfoAsync(
            BitacoraAcciones.PromocionActivada,
            BitacoraEntidades.Promocion,
            promocion.Id,
            "Promocion activada.",
            new
            {
                promocion.Id,
                promocion.Tipo,
                promocion.FechaInicioUtc,
                promocion.FechaFinUtc,
                promocion.CuponDescuentoId,
                promocion.ServicioFotografiaId,
                promocion.EventoId
            },
            cancellationToken);
    }

    private static PromocionResponseDto Map(Promocion promocion)
    {
        return new PromocionResponseDto
        {
            Id = promocion.Id,
            Titulo = promocion.Titulo,
            Descripcion = promocion.Descripcion,
            ImagenUrl = promocion.ImagenUrl,
            Tipo = promocion.Tipo,
            FechaInicioUtc = promocion.FechaInicioUtc,
            FechaFinUtc = promocion.FechaFinUtc,
            Activa = promocion.Activa,
            Destacada = promocion.Destacada,
            Orden = promocion.Orden,
            CuponDescuentoId = promocion.CuponDescuentoId,
            CuponCodigo = promocion.CuponDescuento?.Codigo,
            ServicioFotografiaId = promocion.ServicioFotografiaId,
            ServicioNombre = promocion.ServicioFotografia?.Nombre,
            EventoId = promocion.EventoId,
            EventoNombre = promocion.Evento?.Nombre,
            FechaCreacionUtc = promocion.FechaCreacionUtc,
            FechaActualizacionUtc = promocion.FechaActualizacionUtc
        };
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
