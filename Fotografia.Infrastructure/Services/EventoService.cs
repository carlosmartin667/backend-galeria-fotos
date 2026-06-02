using AutoMapper;
using Fotografia.Application.DTOs.Common;
using Fotografia.Application.DTOs.Eventos;
using Fotografia.Application.DTOs.Notificaciones;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Fotografia.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fotografia.Infrastructure.Services;

public sealed class EventoService(
    AppDbContext dbContext,
    IMapper mapper,
    ICurrentUserService currentUser,
    INotificacionService notificacionService,
    ILogger<EventoService> logger) : IEventoService
{
    public async Task<ApiResponse<IReadOnlyCollection<EventoResponseDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var eventos = await ApplyVisibility(dbContext.Eventos.AsNoTracking())
            .Include(x => x.Fotos)
            .Include(x => x.PortadaFoto)
            .Include(x => x.ClientePrincipal)
            .OrderByDescending(x => x.FechaEventoUtc)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<EventoResponseDto>>.Ok(mapper.Map<List<EventoResponseDto>>(eventos));
    }

    public async Task<ApiResponse<PaginatedResponseDto<EventoResponseDto>>> GetPaginatedAsync(
        PaginationQueryDto pagination,
        CancellationToken cancellationToken = default)
    {
        var validationError = pagination.Validate();
        if (validationError is not null)
        {
            return ApiResponse<PaginatedResponseDto<EventoResponseDto>>.Fail(validationError);
        }

        var query = ApplyVisibility(dbContext.Eventos.AsNoTracking())
            .Include(x => x.Fotos)
            .Include(x => x.PortadaFoto)
            .Include(x => x.ClientePrincipal)
            .OrderByDescending(x => x.FechaEventoUtc);

        var paginated = await query.ToPaginatedResponseAsync(pagination, cancellationToken);

        return ApiResponse<PaginatedResponseDto<EventoResponseDto>>.Ok(new PaginatedResponseDto<EventoResponseDto>
        {
            Items = mapper.Map<List<EventoResponseDto>>(paginated.Items),
            Page = paginated.Page,
            PageSize = paginated.PageSize,
            TotalItems = paginated.TotalItems,
            TotalPages = paginated.TotalPages,
            HasPreviousPage = paginated.HasPreviousPage,
            HasNextPage = paginated.HasNextPage,
            All = paginated.All
        });
    }

    public async Task<ApiResponse<EventoResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var evento = await ApplyVisibility(dbContext.Eventos.AsNoTracking())
            .Include(x => x.Fotos)
            .Include(x => x.PortadaFoto)
            .Include(x => x.ClientePrincipal)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return evento is null
            ? ApiResponse<EventoResponseDto>.NotFound("Evento no encontrado.")
            : ApiResponse<EventoResponseDto>.Ok(mapper.Map<EventoResponseDto>(evento));
    }

    public async Task<ApiResponse<EventoResponseDto>> CreateAsync(CrearEventoRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.ClientePrincipalId is not null)
        {
            var clienteExists = await dbContext.Clientes.AnyAsync(x => x.Id == request.ClientePrincipalId, cancellationToken);
            if (!clienteExists)
            {
                return ApiResponse<EventoResponseDto>.NotFound("Cliente principal no encontrado.");
            }
        }

        if (!TryNormalizeEstado(request.Estado, out var estado))
        {
            return ApiResponse<EventoResponseDto>.Fail("Estado de evento invalido.");
        }

        if (!TryNormalizeVisibilidad(request.Visibilidad, out var visibilidad))
        {
            return ApiResponse<EventoResponseDto>.Fail("Visibilidad de evento invalida.");
        }

        var evento = mapper.Map<Evento>(request);
        evento.Nombre = request.Nombre.Trim();
        evento.Descripcion = request.Descripcion;
        evento.Estado = estado;
        evento.Visibilidad = visibilidad;
        evento.Activo = true;
        evento.Slug = await CreateUniqueSlugAsync(evento.Nombre, null, cancellationToken);
        evento.CreadoPorUsuarioId = currentUser.UserId;
        evento.CreadoEnUtc = DateTime.UtcNow;

        dbContext.Eventos.Add(evento);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (evento.Estado == EventoEstados.Publicado)
        {
            await TryEnqueueEventoPublicadoAsync(evento, cancellationToken);
        }

        return ApiResponse<EventoResponseDto>.Ok(mapper.Map<EventoResponseDto>(evento), "Evento creado.");
    }

    public async Task<ApiResponse<EventoResponseDto>> UpdateAsync(Guid id, ActualizarEventoRequestDto request, CancellationToken cancellationToken = default)
    {
        var evento = await dbContext.Eventos
            .Include(x => x.Fotos)
            .Include(x => x.PortadaFoto)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (evento is null)
        {
            return ApiResponse<EventoResponseDto>.NotFound("Evento no encontrado.");
        }

        if (request.ClientePrincipalId is not null)
        {
            var clienteExists = await dbContext.Clientes.AnyAsync(x => x.Id == request.ClientePrincipalId, cancellationToken);
            if (!clienteExists)
            {
                return ApiResponse<EventoResponseDto>.NotFound("Cliente principal no encontrado.");
            }
        }

        if (!TryNormalizeEstado(request.Estado, out var estado))
        {
            return ApiResponse<EventoResponseDto>.Fail("Estado de evento invalido.");
        }

        if (!TryNormalizeVisibilidad(request.Visibilidad, out var visibilidad))
        {
            return ApiResponse<EventoResponseDto>.Fail("Visibilidad de evento invalida.");
        }

        var previousName = evento.Nombre;
        var previousEstado = evento.Estado;
        evento.Nombre = request.Nombre.Trim();
        evento.Descripcion = request.Descripcion;
        evento.FechaEventoUtc = request.FechaEventoUtc;
        evento.Estado = estado;
        evento.Visibilidad = visibilidad;
        evento.FechaLimiteCompraUtc = request.FechaLimiteCompraUtc;
        evento.Activo = request.Activo;
        evento.ClientePrincipalId = request.ClientePrincipalId;
        evento.ActualizadoEnUtc = DateTime.UtcNow;

        if (!string.Equals(previousName, evento.Nombre, StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(evento.Slug))
        {
            evento.Slug = await CreateUniqueSlugAsync(evento.Nombre, evento.Id, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        if (estado == EventoEstados.Publicado
            && !string.Equals(previousEstado, EventoEstados.Publicado, StringComparison.OrdinalIgnoreCase))
        {
            await TryEnqueueEventoPublicadoAsync(evento, cancellationToken);
        }

        return ApiResponse<EventoResponseDto>.Ok(mapper.Map<EventoResponseDto>(evento), "Evento actualizado.");
    }

    public async Task<ApiResponse<EventoResponseDto>> SetPortadaAsync(
        Guid eventoId,
        Guid fotoId,
        CancellationToken cancellationToken = default)
    {
        var evento = await dbContext.Eventos
            .Include(x => x.Fotos)
            .Include(x => x.PortadaFoto)
            .FirstOrDefaultAsync(x => x.Id == eventoId, cancellationToken);

        if (evento is null)
        {
            return ApiResponse<EventoResponseDto>.NotFound("Evento no encontrado.");
        }

        var foto = await dbContext.Fotos.FirstOrDefaultAsync(
            x => x.Id == fotoId && x.EventoId == eventoId && x.Activa,
            cancellationToken);

        if (foto is null)
        {
            return ApiResponse<EventoResponseDto>.Fail("La foto no existe, esta inactiva o no pertenece al evento.");
        }

        evento.PortadaFotoId = foto.Id;
        evento.PortadaFoto = foto;
        evento.ActualizadoEnUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<EventoResponseDto>.Ok(mapper.Map<EventoResponseDto>(evento), "Portada del evento actualizada.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var evento = await dbContext.Eventos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (evento is null)
        {
            return ApiResponse<bool>.NotFound("Evento no encontrado.");
        }

        evento.Activo = false;
        evento.Estado = EventoEstados.Archivado;
        evento.ActualizadoEnUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Evento archivado.");
    }

    private IQueryable<Evento> ApplyVisibility(IQueryable<Evento> query)
    {
        if (currentUser.IsAdmin)
        {
            return query;
        }

        if (currentUser.IsAuthenticated && currentUser.UserId is Guid userId)
        {
            return query.Where(x =>
                x.Activo
                && ((x.Visibilidad == EventoVisibilidades.Publico
                        && (x.Estado == EventoEstados.Publicado || x.Estado == EventoEstados.LegacyActivo))
                    || x.CreadoPorUsuarioId == userId
                    || (x.ClientePrincipal != null && x.ClientePrincipal.UsuarioId == userId)));
        }

        return query.Where(x =>
            x.Activo
            && x.Visibilidad == EventoVisibilidades.Publico
            && (x.Estado == EventoEstados.Publicado || x.Estado == EventoEstados.LegacyActivo));
    }

    private async Task<string> CreateUniqueSlugAsync(string name, Guid? excludeEventoId, CancellationToken cancellationToken)
    {
        var baseSlug = FileHelper.CreateSlug(name);
        var slug = baseSlug;
        var index = 1;

        while (await dbContext.Eventos.AnyAsync(
            x => x.Slug == slug && (excludeEventoId == null || x.Id != excludeEventoId.Value),
            cancellationToken))
        {
            slug = $"{baseSlug}-{index++}";
        }

        return slug;
    }

    private static bool TryNormalizeEstado(string? value, out string estado)
    {
        estado = EventoEstados.Publicado;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var trimmed = value.Trim();
        if (string.Equals(trimmed, EventoEstados.LegacyActivo, StringComparison.OrdinalIgnoreCase))
        {
            estado = EventoEstados.Publicado;
            return true;
        }

        var allowed = new[]
        {
            EventoEstados.Borrador,
            EventoEstados.Publicado,
            EventoEstados.Finalizado,
            EventoEstados.Archivado
        };

        var match = allowed.FirstOrDefault(x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase));
        if (match is null)
        {
            return false;
        }

        estado = match;
        return true;
    }

    private static bool TryNormalizeVisibilidad(string? value, out string visibilidad)
    {
        visibilidad = EventoVisibilidades.Publico;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var allowed = new[]
        {
            EventoVisibilidades.Publico,
            EventoVisibilidades.Privado,
            EventoVisibilidades.Oculto
        };

        var match = allowed.FirstOrDefault(x => string.Equals(x, value.Trim(), StringComparison.OrdinalIgnoreCase));
        if (match is null)
        {
            return false;
        }

        visibilidad = match;
        return true;
    }

    private async Task TryEnqueueEventoPublicadoAsync(
        Evento evento,
        CancellationToken cancellationToken)
    {
        if (evento.ClientePrincipalId is not Guid clienteId)
        {
            return;
        }

        var cliente = await dbContext.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == clienteId, cancellationToken);

        if (cliente is null || string.IsNullOrWhiteSpace(cliente.Email))
        {
            return;
        }

        await TryEnqueueTemplateAsync(new EnqueueTemplateNotificacionRequestDto
        {
            Codigo = NotificacionTipos.EventoPublicadoCliente,
            DestinatarioEmail = cliente.Email,
            UsuarioId = cliente.UsuarioId,
            EntidadTipo = "Evento",
            EntidadId = evento.Id,
            CorrelationKey = $"evento:{evento.Id}:publicado:cliente",
            Reemplazos = new Dictionary<string, string?>
            {
                ["NombreCliente"] = cliente.Nombre,
                ["EmailCliente"] = cliente.Email,
                ["NombreEvento"] = evento.Nombre,
                ["PedidoId"] = string.Empty,
                ["Total"] = string.Empty,
                ["Estado"] = evento.Estado,
                ["Link"] = "Disponible en tu cuenta",
                ["NombreFotografa"] = "Fotografa",
                ["Fecha"] = evento.FechaEventoUtc.ToString("yyyy-MM-dd")
            }
        }, cancellationToken);
    }

    private async Task TryEnqueueTemplateAsync(
        EnqueueTemplateNotificacionRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await notificacionService.EnqueueFromTemplateAsync(request, cancellationToken);
            if (!result.Success)
            {
                logger.LogWarning(
                    "No se pudo encolar notificacion de evento. Codigo={Codigo} EntidadId={EntidadId} Motivo={Motivo}",
                    request.Codigo,
                    request.EntidadId,
                    result.Message);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(
                ex,
                "No se pudo encolar notificacion de evento. Codigo={Codigo} EntidadId={EntidadId}",
                request.Codigo,
                request.EntidadId);
        }
    }
}
