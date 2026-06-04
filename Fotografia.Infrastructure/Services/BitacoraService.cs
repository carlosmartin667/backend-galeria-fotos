using Fotografia.Application.DTOs.Bitacora;
using Fotografia.Application.DTOs.Common;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fotografia.Infrastructure.Services;

public sealed class BitacoraService(
    AppDbContext dbContext,
    ICurrentUserService currentUser,
    IHttpContextAccessor httpContextAccessor,
    ILogger<BitacoraService> logger) : IBitacoraService
{
    private const int DefaultPageSize = 25;
    private const int MaxPageSize = 100;
    private const string CorrelationIdItemName = "CorrelationId";

    public async Task RegistrarAsync(CrearBitacoraRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Accion)
                || string.IsNullOrWhiteSpace(request.EntidadTipo)
                || string.IsNullOrWhiteSpace(request.Descripcion))
            {
                logger.LogWarning("Bitacora omitida por datos requeridos incompletos. Accion={Accion} EntidadTipo={EntidadTipo}", request.Accion, request.EntidadTipo);
                return;
            }

            var context = httpContextAccessor.HttpContext;
            var entry = new Bitacora
            {
                UsuarioId = request.UsuarioId ?? currentUser.UserId,
                UsuarioEmail = Limit(request.UsuarioEmail ?? currentUser.Email, 256),
                Rol = Limit(request.Rol ?? currentUser.Rol, 64),
                Accion = LimitRequired(request.Accion, 120),
                EntidadTipo = LimitRequired(request.EntidadTipo, 80),
                EntidadId = request.EntidadId,
                Descripcion = LimitRequired(request.Descripcion, 1000),
                Ip = Limit(request.Ip ?? context?.Connection.RemoteIpAddress?.ToString(), 64),
                UserAgent = Limit(request.UserAgent ?? context?.Request.Headers.UserAgent.FirstOrDefault(), 500),
                CorrelationId = Limit(request.CorrelationId ?? GetCorrelationId(context), 120),
                RequestPath = Limit(request.RequestPath ?? context?.Request.Path.Value, 300),
                HttpMethod = Limit(request.HttpMethod ?? context?.Request.Method, 16),
                MetadataJson = AuditMetadataSanitizer.SanitizeToJson(request.Metadata, request.MetadataJson),
                Severidad = BitacoraSeveridades.Normalize(request.Severidad),
                FechaUtc = DateTime.UtcNow
            };

            dbContext.Bitacora.Add(entry);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "No se pudo registrar bitacora. Accion={Accion} EntidadTipo={EntidadTipo} EntidadId={EntidadId}", request.Accion, request.EntidadTipo, request.EntidadId);
        }
    }

    public Task RegistrarInfoAsync(
        string accion,
        string entidadTipo,
        Guid? entidadId,
        string descripcion,
        object? metadata = null,
        CancellationToken cancellationToken = default)
    {
        return RegistrarWithSeverityAsync(BitacoraSeveridades.Info, accion, entidadTipo, entidadId, descripcion, metadata, cancellationToken);
    }

    public Task RegistrarWarningAsync(
        string accion,
        string entidadTipo,
        Guid? entidadId,
        string descripcion,
        object? metadata = null,
        CancellationToken cancellationToken = default)
    {
        return RegistrarWithSeverityAsync(BitacoraSeveridades.Warning, accion, entidadTipo, entidadId, descripcion, metadata, cancellationToken);
    }

    public Task RegistrarErrorAsync(
        string accion,
        string entidadTipo,
        Guid? entidadId,
        string descripcion,
        object? metadata = null,
        CancellationToken cancellationToken = default)
    {
        return RegistrarWithSeverityAsync(BitacoraSeveridades.Error, accion, entidadTipo, entidadId, descripcion, metadata, cancellationToken);
    }

    public async Task<ApiResponse<PaginatedResponseDto<BitacoraResponseDto>>> GetAdminAsync(
        BitacoraQueryDto query,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<PaginatedResponseDto<BitacoraResponseDto>>.Forbidden("Solo un administrador puede consultar la bitacora.");
        }

        query.Page = Math.Max(1, query.Page);
        query.PageSize = Math.Clamp(query.PageSize <= 0 ? DefaultPageSize : query.PageSize, 1, MaxPageSize);

        var validation = query.Validate();
        if (validation is not null)
        {
            return ApiResponse<PaginatedResponseDto<BitacoraResponseDto>>.Fail(validation);
        }

        var bitacoraQuery = ApplyFilters(dbContext.Bitacora.AsNoTracking(), query);
        var totalItems = await bitacoraQuery.CountAsync(cancellationToken);
        var totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling(totalItems / (double)query.PageSize);

        var items = await bitacoraQuery
            .OrderByDescending(x => x.FechaUtc)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);

        return ApiResponse<PaginatedResponseDto<BitacoraResponseDto>>.Ok(new PaginatedResponseDto<BitacoraResponseDto>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            HasPreviousPage = query.Page > 1,
            HasNextPage = query.Page < totalPages,
            All = false
        });
    }

    public async Task<ApiResponse<BitacoraResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<BitacoraResponseDto>.Forbidden("Solo un administrador puede consultar la bitacora.");
        }

        var entry = await dbContext.Bitacora
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entry is null
            ? ApiResponse<BitacoraResponseDto>.NotFound("Entrada de bitacora no encontrada.")
            : ApiResponse<BitacoraResponseDto>.Ok(Map(entry));
    }

    public async Task<ApiResponse<BitacoraResumenDto>> GetResumenAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<BitacoraResumenDto>.Forbidden("Solo un administrador puede consultar la bitacora.");
        }

        var total = await dbContext.Bitacora.AsNoTracking().CountAsync(cancellationToken);
        var porSeveridad = await dbContext.Bitacora
            .AsNoTracking()
            .GroupBy(x => x.Severidad)
            .ToDictionaryAsync(x => x.Key, x => x.Count(), cancellationToken);
        var porAccion = await dbContext.Bitacora
            .AsNoTracking()
            .GroupBy(x => x.Accion)
            .OrderByDescending(x => x.Count())
            .Take(10)
            .ToDictionaryAsync(x => x.Key, x => x.Count(), cancellationToken);

        return ApiResponse<BitacoraResumenDto>.Ok(new BitacoraResumenDto
        {
            Total = total,
            PorSeveridad = porSeveridad,
            PorAccion = porAccion
        });
    }

    private Task RegistrarWithSeverityAsync(
        string severidad,
        string accion,
        string entidadTipo,
        Guid? entidadId,
        string descripcion,
        object? metadata,
        CancellationToken cancellationToken)
    {
        return RegistrarAsync(new CrearBitacoraRequestDto
        {
            Accion = accion,
            EntidadTipo = entidadTipo,
            EntidadId = entidadId,
            Descripcion = descripcion,
            Metadata = metadata,
            Severidad = severidad
        }, cancellationToken);
    }

    private static IQueryable<Bitacora> ApplyFilters(IQueryable<Bitacora> query, BitacoraQueryDto filters)
    {
        if (filters.Desde is DateTime desde)
        {
            query = query.Where(x => x.FechaUtc >= desde);
        }

        if (filters.Hasta is DateTime hasta)
        {
            query = query.Where(x => x.FechaUtc <= hasta);
        }

        if (filters.UsuarioId is Guid usuarioId)
        {
            query = query.Where(x => x.UsuarioId == usuarioId);
        }

        if (!string.IsNullOrWhiteSpace(filters.UsuarioEmail))
        {
            var email = filters.UsuarioEmail.Trim();
            query = query.Where(x => x.UsuarioEmail != null && x.UsuarioEmail.Contains(email));
        }

        if (!string.IsNullOrWhiteSpace(filters.Accion))
        {
            var accion = filters.Accion.Trim();
            query = query.Where(x => x.Accion == accion);
        }

        if (!string.IsNullOrWhiteSpace(filters.EntidadTipo))
        {
            var entidadTipo = filters.EntidadTipo.Trim();
            query = query.Where(x => x.EntidadTipo == entidadTipo);
        }

        if (filters.EntidadId is Guid entidadId)
        {
            query = query.Where(x => x.EntidadId == entidadId);
        }

        if (!string.IsNullOrWhiteSpace(filters.Severidad))
        {
            var severidad = BitacoraSeveridades.Normalize(filters.Severidad);
            query = query.Where(x => x.Severidad == severidad);
        }

        if (!string.IsNullOrWhiteSpace(filters.CorrelationId))
        {
            var correlationId = filters.CorrelationId.Trim();
            query = query.Where(x => x.CorrelationId == correlationId);
        }

        return query;
    }

    private static BitacoraResponseDto Map(Bitacora entry)
    {
        return new BitacoraResponseDto
        {
            Id = entry.Id,
            UsuarioId = entry.UsuarioId,
            UsuarioEmail = entry.UsuarioEmail,
            Rol = entry.Rol,
            Accion = entry.Accion,
            EntidadTipo = entry.EntidadTipo,
            EntidadId = entry.EntidadId,
            Descripcion = entry.Descripcion,
            Ip = entry.Ip,
            UserAgent = entry.UserAgent,
            CorrelationId = entry.CorrelationId,
            RequestPath = entry.RequestPath,
            HttpMethod = entry.HttpMethod,
            MetadataJson = entry.MetadataJson,
            Severidad = entry.Severidad,
            FechaUtc = entry.FechaUtc
        };
    }

    private static string? GetCorrelationId(HttpContext? context)
    {
        if (context?.Items.TryGetValue(CorrelationIdItemName, out var value) == true)
        {
            return value as string;
        }

        return context?.TraceIdentifier;
    }

    private static string? Limit(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    private static string LimitRequired(string value, int maxLength)
    {
        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
