using AutoMapper;
using Fotografia.Application.DTOs.Agenda;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Services;

public sealed class AgendaService(
    AppDbContext dbContext,
    IMapper mapper,
    ICurrentUserService currentUser) : IAgendaService
{
    public async Task<ApiResponse<IReadOnlyCollection<AgendaItemResponseDto>>> GetAllAsync(
        AgendaQueryDto query,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<IReadOnlyCollection<AgendaItemResponseDto>>.Forbidden("Solo un administrador puede consultar la agenda.");
        }

        if (query.Desde is not null && query.Hasta is not null && query.Hasta <= query.Desde)
        {
            return ApiResponse<IReadOnlyCollection<AgendaItemResponseDto>>.Fail("Hasta debe ser mayor que Desde.");
        }

        var agendaQuery = QueryAgenda().AsNoTracking();

        if (query.Desde is not null)
        {
            agendaQuery = agendaQuery.Where(x => x.FechaFinUtc >= query.Desde.Value);
        }

        if (query.Hasta is not null)
        {
            agendaQuery = agendaQuery.Where(x => x.FechaInicioUtc <= query.Hasta.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Tipo))
        {
            agendaQuery = agendaQuery.Where(x => x.Tipo == query.Tipo.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.Estado))
        {
            agendaQuery = agendaQuery.Where(x => x.Estado == query.Estado.Trim());
        }

        if (query.Activo is not null)
        {
            agendaQuery = agendaQuery.Where(x => x.Activo == query.Activo.Value);
        }

        var items = await agendaQuery
            .OrderBy(x => x.FechaInicioUtc)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<AgendaItemResponseDto>>.Ok(
            mapper.Map<List<AgendaItemResponseDto>>(items));
    }

    public async Task<ApiResponse<AgendaItemResponseDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<AgendaItemResponseDto>.Forbidden("Solo un administrador puede consultar la agenda.");
        }

        var item = await QueryAgenda()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return item is null
            ? ApiResponse<AgendaItemResponseDto>.NotFound("Item de agenda no encontrado.")
            : ApiResponse<AgendaItemResponseDto>.Ok(mapper.Map<AgendaItemResponseDto>(item));
    }

    public async Task<ApiResponse<AgendaItemResponseDto>> CreateAsync(
        CrearAgendaItemRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<AgendaItemResponseDto>.Forbidden("Solo un administrador puede crear items de agenda.");
        }

        var validation = await ValidateAgendaInputAsync(
            request.Tipo,
            request.Estado,
            request.FechaInicioUtc,
            request.FechaFinUtc,
            request.EventoId,
            request.SesionPrivadaId,
            request.ClienteId,
            request.SolicitudPresupuestoId,
            null,
            cancellationToken);

        if (validation.Message is not null)
        {
            return ApiResponse<AgendaItemResponseDto>.Fail(validation.Message);
        }

        var item = new AgendaItem
        {
            Titulo = request.Titulo.Trim(),
            Descripcion = Normalize(request.Descripcion),
            Tipo = validation.Tipo!,
            FechaInicioUtc = request.FechaInicioUtc,
            FechaFinUtc = request.FechaFinUtc,
            Ubicacion = Normalize(request.Ubicacion),
            Estado = validation.Estado!,
            EventoId = request.EventoId,
            SesionPrivadaId = request.SesionPrivadaId,
            ClienteId = request.ClienteId,
            SolicitudPresupuestoId = request.SolicitudPresupuestoId,
            Activo = request.Activo,
            FechaCreacionUtc = DateTime.UtcNow
        };

        dbContext.AgendaItems.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);

        var created = await QueryAgenda()
            .AsNoTracking()
            .FirstAsync(x => x.Id == item.Id, cancellationToken);

        return ApiResponse<AgendaItemResponseDto>.Ok(
            mapper.Map<AgendaItemResponseDto>(created),
            "Item de agenda creado.");
    }

    public async Task<ApiResponse<AgendaItemResponseDto>> UpdateAsync(
        Guid id,
        ActualizarAgendaItemRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<AgendaItemResponseDto>.Forbidden("Solo un administrador puede editar items de agenda.");
        }

        var item = await dbContext.AgendaItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null)
        {
            return ApiResponse<AgendaItemResponseDto>.NotFound("Item de agenda no encontrado.");
        }

        var validation = await ValidateAgendaInputAsync(
            request.Tipo,
            request.Estado,
            request.FechaInicioUtc,
            request.FechaFinUtc,
            request.EventoId,
            request.SesionPrivadaId,
            request.ClienteId,
            request.SolicitudPresupuestoId,
            id,
            cancellationToken);

        if (validation.Message is not null)
        {
            return ApiResponse<AgendaItemResponseDto>.Fail(validation.Message);
        }

        item.Titulo = request.Titulo.Trim();
        item.Descripcion = Normalize(request.Descripcion);
        item.Tipo = validation.Tipo!;
        item.FechaInicioUtc = request.FechaInicioUtc;
        item.FechaFinUtc = request.FechaFinUtc;
        item.Ubicacion = Normalize(request.Ubicacion);
        item.Estado = validation.Estado!;
        item.EventoId = request.EventoId;
        item.SesionPrivadaId = request.SesionPrivadaId;
        item.ClienteId = request.ClienteId;
        item.SolicitudPresupuestoId = request.SolicitudPresupuestoId;
        item.Activo = request.Activo;
        item.FechaActualizacionUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        var updated = await QueryAgenda()
            .AsNoTracking()
            .FirstAsync(x => x.Id == id, cancellationToken);

        return ApiResponse<AgendaItemResponseDto>.Ok(
            mapper.Map<AgendaItemResponseDto>(updated),
            "Item de agenda actualizado.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<bool>.Forbidden("Solo un administrador puede eliminar items de agenda.");
        }

        var item = await dbContext.AgendaItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null)
        {
            return ApiResponse<bool>.NotFound("Item de agenda no encontrado.");
        }

        item.Activo = false;
        item.FechaActualizacionUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Item de agenda desactivado.");
    }

    public async Task<ApiResponse<DisponibilidadAgendaResponseDto>> GetDisponibilidadAsync(
        DateTime? desde,
        DateTime? hasta,
        CancellationToken cancellationToken = default)
    {
        var desdeUtc = desde ?? DateTime.UtcNow.Date;
        var hastaUtc = hasta ?? desdeUtc.AddDays(30);

        if (hastaUtc <= desdeUtc)
        {
            return ApiResponse<DisponibilidadAgendaResponseDto>.Fail("Hasta debe ser mayor que Desde.");
        }

        var items = await dbContext.AgendaItems
            .AsNoTracking()
            .Where(x =>
                x.Activo
                && x.Estado != AgendaItemEstados.Cancelado
                && x.FechaInicioUtc < hastaUtc
                && desdeUtc < x.FechaFinUtc
                && (x.Tipo == AgendaItemTipos.Evento
                    || x.Tipo == AgendaItemTipos.SesionPrivada
                    || x.Tipo == AgendaItemTipos.Bloqueo))
            .OrderBy(x => x.FechaInicioUtc)
            .Select(x => new DisponibilidadAgendaItemDto
            {
                FechaInicioUtc = x.FechaInicioUtc,
                FechaFinUtc = x.FechaFinUtc,
                Ocupado = true,
                Tipo = "Ocupado"
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<DisponibilidadAgendaResponseDto>.Ok(new DisponibilidadAgendaResponseDto
        {
            DesdeUtc = desdeUtc,
            HastaUtc = hastaUtc,
            Items = items
        });
    }

    private IQueryable<AgendaItem> QueryAgenda()
    {
        return dbContext.AgendaItems
            .Include(x => x.Evento)
            .Include(x => x.SesionPrivada)
            .Include(x => x.Cliente)
            .Include(x => x.SolicitudPresupuesto);
    }

    private async Task<AgendaValidationResult> ValidateAgendaInputAsync(
        string? tipoValue,
        string? estadoValue,
        DateTime fechaInicioUtc,
        DateTime fechaFinUtc,
        Guid? eventoId,
        Guid? sesionPrivadaId,
        Guid? clienteId,
        Guid? solicitudPresupuestoId,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        if (fechaFinUtc <= fechaInicioUtc)
        {
            return new AgendaValidationResult(null, null, "FechaFinUtc debe ser mayor que FechaInicioUtc.");
        }

        if (!AgendaItemTipos.TryNormalize(tipoValue, out var tipo))
        {
            return new AgendaValidationResult(null, null, "Tipo de agenda invalido.");
        }

        if (!AgendaItemEstados.TryNormalize(estadoValue, out var estado))
        {
            return new AgendaValidationResult(null, null, "Estado de agenda invalido.");
        }

        var relationValidation = await ValidateRelationsAsync(eventoId, sesionPrivadaId, clienteId, solicitudPresupuestoId, cancellationToken);
        if (relationValidation is not null)
        {
            return new AgendaValidationResult(null, null, relationValidation);
        }

        if (AgendaItemTipos.BloqueaDisponibilidad(tipo) && !AgendaItemEstados.EsCancelado(estado))
        {
            var overlaps = await dbContext.AgendaItems.AnyAsync(x =>
                x.Activo
                && x.Id != excludeId
                && x.Estado != AgendaItemEstados.Cancelado
                && (x.Tipo == AgendaItemTipos.Evento
                    || x.Tipo == AgendaItemTipos.SesionPrivada
                    || x.Tipo == AgendaItemTipos.Bloqueo)
                && x.FechaInicioUtc < fechaFinUtc
                && fechaInicioUtc < x.FechaFinUtc,
                cancellationToken);

            if (overlaps)
            {
                return new AgendaValidationResult(null, null, "Ya existe un item de agenda activo que se superpone con ese rango.");
            }
        }

        return new AgendaValidationResult(tipo, estado, null);
    }

    private async Task<string?> ValidateRelationsAsync(
        Guid? eventoId,
        Guid? sesionPrivadaId,
        Guid? clienteId,
        Guid? solicitudPresupuestoId,
        CancellationToken cancellationToken)
    {
        if (eventoId is Guid evento && !await dbContext.Eventos.AnyAsync(x => x.Id == evento, cancellationToken))
        {
            return "Evento no encontrado.";
        }

        if (sesionPrivadaId is Guid sesion && !await dbContext.SesionesPrivadas.AnyAsync(x => x.Id == sesion, cancellationToken))
        {
            return "Sesion privada no encontrada.";
        }

        if (clienteId is Guid cliente && !await dbContext.Clientes.AnyAsync(x => x.Id == cliente, cancellationToken))
        {
            return "Cliente no encontrado.";
        }

        if (solicitudPresupuestoId is Guid solicitud
            && !await dbContext.SolicitudesPresupuesto.AnyAsync(x => x.Id == solicitud, cancellationToken))
        {
            return "Solicitud de presupuesto no encontrada.";
        }

        return null;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private sealed record AgendaValidationResult(string? Tipo, string? Estado, string? Message);
}
