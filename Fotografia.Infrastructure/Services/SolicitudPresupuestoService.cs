using AutoMapper;
using Fotografia.Application.DTOs.Presupuestos;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Services;

public sealed class SolicitudPresupuestoService(
    AppDbContext dbContext,
    IMapper mapper,
    ICurrentUserService currentUser) : ISolicitudPresupuestoService
{
    public async Task<ApiResponse<SolicitudPresupuestoResponseDto>> CreateAsync(
        CrearSolicitudPresupuestoRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var servicioValidation = await ValidateServicioAsync(request.ServicioId, cancellationToken);
        if (servicioValidation is not null)
        {
            return ApiResponse<SolicitudPresupuestoResponseDto>.Fail(servicioValidation);
        }

        var solicitud = new SolicitudPresupuesto
        {
            Nombre = request.Nombre.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            WhatsApp = Normalize(request.WhatsApp),
            TipoEvento = Normalize(request.TipoEvento),
            ServicioId = request.ServicioId,
            FechaTentativaUtc = request.FechaTentativaUtc,
            Lugar = Normalize(request.Lugar),
            CantidadInvitados = request.CantidadInvitados,
            Mensaje = request.Mensaje.Trim(),
            Estado = SolicitudPresupuestoEstados.Nuevo,
            Activa = true,
            FechaCreacionUtc = DateTime.UtcNow
        };

        dbContext.SolicitudesPresupuesto.Add(solicitud);
        await dbContext.SaveChangesAsync(cancellationToken);

        var created = await QuerySolicitudes()
            .AsNoTracking()
            .FirstAsync(x => x.Id == solicitud.Id, cancellationToken);

        return ApiResponse<SolicitudPresupuestoResponseDto>.Ok(
            mapper.Map<SolicitudPresupuestoResponseDto>(created),
            "Solicitud de presupuesto creada.");
    }

    public async Task<ApiResponse<IReadOnlyCollection<SolicitudPresupuestoResponseDto>>> GetAllAsync(
        bool? activa = null,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<IReadOnlyCollection<SolicitudPresupuestoResponseDto>>.Forbidden("Solo un administrador puede consultar solicitudes.");
        }

        var query = QuerySolicitudes().AsNoTracking();
        if (activa is not null)
        {
            query = query.Where(x => x.Activa == activa.Value);
        }

        var solicitudes = await query
            .OrderByDescending(x => x.FechaCreacionUtc)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<SolicitudPresupuestoResponseDto>>.Ok(
            mapper.Map<List<SolicitudPresupuestoResponseDto>>(solicitudes));
    }

    public async Task<ApiResponse<SolicitudPresupuestoResponseDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<SolicitudPresupuestoResponseDto>.Forbidden("Solo un administrador puede consultar solicitudes.");
        }

        var solicitud = await QuerySolicitudes()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return solicitud is null
            ? ApiResponse<SolicitudPresupuestoResponseDto>.NotFound("Solicitud de presupuesto no encontrada.")
            : ApiResponse<SolicitudPresupuestoResponseDto>.Ok(mapper.Map<SolicitudPresupuestoResponseDto>(solicitud));
    }

    public async Task<ApiResponse<SolicitudPresupuestoResponseDto>> UpdateAsync(
        Guid id,
        ActualizarSolicitudPresupuestoRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<SolicitudPresupuestoResponseDto>.Forbidden("Solo un administrador puede editar solicitudes.");
        }

        if (!SolicitudPresupuestoEstados.TryNormalize(request.Estado, out var estado))
        {
            return ApiResponse<SolicitudPresupuestoResponseDto>.Fail("Estado de solicitud invalido.");
        }

        var servicioValidation = await ValidateServicioAsync(request.ServicioId, cancellationToken);
        if (servicioValidation is not null)
        {
            return ApiResponse<SolicitudPresupuestoResponseDto>.Fail(servicioValidation);
        }

        var solicitud = await dbContext.SolicitudesPresupuesto.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (solicitud is null)
        {
            return ApiResponse<SolicitudPresupuestoResponseDto>.NotFound("Solicitud de presupuesto no encontrada.");
        }

        solicitud.Nombre = request.Nombre.Trim();
        solicitud.Email = request.Email.Trim().ToLowerInvariant();
        solicitud.WhatsApp = Normalize(request.WhatsApp);
        solicitud.TipoEvento = Normalize(request.TipoEvento);
        solicitud.ServicioId = request.ServicioId;
        solicitud.FechaTentativaUtc = request.FechaTentativaUtc;
        solicitud.Lugar = Normalize(request.Lugar);
        solicitud.CantidadInvitados = request.CantidadInvitados;
        solicitud.Mensaje = request.Mensaje.Trim();
        solicitud.Estado = estado;
        solicitud.Activa = request.Activa;
        solicitud.FechaActualizacionUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        var updated = await QuerySolicitudes()
            .AsNoTracking()
            .FirstAsync(x => x.Id == id, cancellationToken);

        return ApiResponse<SolicitudPresupuestoResponseDto>.Ok(
            mapper.Map<SolicitudPresupuestoResponseDto>(updated),
            "Solicitud de presupuesto actualizada.");
    }

    public async Task<ApiResponse<SolicitudPresupuestoResponseDto>> ChangeEstadoAsync(
        Guid id,
        CambiarEstadoSolicitudPresupuestoRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<SolicitudPresupuestoResponseDto>.Forbidden("Solo un administrador puede cambiar el estado de solicitudes.");
        }

        if (!SolicitudPresupuestoEstados.TryNormalize(request.Estado, out var estado))
        {
            return ApiResponse<SolicitudPresupuestoResponseDto>.Fail("Estado de solicitud invalido.");
        }

        var solicitud = await dbContext.SolicitudesPresupuesto.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (solicitud is null)
        {
            return ApiResponse<SolicitudPresupuestoResponseDto>.NotFound("Solicitud de presupuesto no encontrada.");
        }

        solicitud.Estado = estado;
        solicitud.FechaActualizacionUtc = DateTime.UtcNow;
        if (estado == SolicitudPresupuestoEstados.Cerrado)
        {
            solicitud.Activa = false;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var updated = await QuerySolicitudes()
            .AsNoTracking()
            .FirstAsync(x => x.Id == id, cancellationToken);

        return ApiResponse<SolicitudPresupuestoResponseDto>.Ok(
            mapper.Map<SolicitudPresupuestoResponseDto>(updated),
            "Estado de solicitud actualizado.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<bool>.Forbidden("Solo un administrador puede cerrar solicitudes.");
        }

        var solicitud = await dbContext.SolicitudesPresupuesto.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (solicitud is null)
        {
            return ApiResponse<bool>.NotFound("Solicitud de presupuesto no encontrada.");
        }

        solicitud.Activa = false;
        solicitud.Estado = SolicitudPresupuestoEstados.Cerrado;
        solicitud.FechaActualizacionUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Solicitud de presupuesto cerrada.");
    }

    private IQueryable<SolicitudPresupuesto> QuerySolicitudes()
    {
        return dbContext.SolicitudesPresupuesto.Include(x => x.Servicio);
    }

    private async Task<string?> ValidateServicioAsync(Guid? servicioId, CancellationToken cancellationToken)
    {
        if (servicioId is null)
        {
            return null;
        }

        var exists = await dbContext.ServiciosFotografia
            .AnyAsync(x => x.Id == servicioId.Value && x.Activo, cancellationToken);

        return exists ? null : "El servicio indicado no existe o no esta activo.";
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
