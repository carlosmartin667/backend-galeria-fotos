using AutoMapper;
using Fotografia.Infrastructure.Data;
using Fotografia.Application.DTOs.Eventos;
using Fotografia.Domain.Entities;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Services;

public sealed class EventoService(AppDbContext dbContext, IMapper mapper, ICurrentUserService currentUser) : IEventoService
{
    public async Task<ApiResponse<IReadOnlyCollection<EventoResponseDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var eventos = await dbContext.Eventos
            .AsNoTracking()
            .Include(x => x.Fotos)
            .OrderByDescending(x => x.FechaEventoUtc)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<EventoResponseDto>>.Ok(mapper.Map<List<EventoResponseDto>>(eventos));
    }

    public async Task<ApiResponse<EventoResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var evento = await dbContext.Eventos
            .AsNoTracking()
            .Include(x => x.Fotos)
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

        var evento = mapper.Map<Evento>(request);
        evento.Slug = await CreateUniqueSlugAsync(evento.Nombre, cancellationToken);
        evento.CreadoPorUsuarioId = currentUser.UserId;

        dbContext.Eventos.Add(evento);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<EventoResponseDto>.Ok(mapper.Map<EventoResponseDto>(evento), "Evento creado.");
    }

    public async Task<ApiResponse<EventoResponseDto>> UpdateAsync(Guid id, ActualizarEventoRequestDto request, CancellationToken cancellationToken = default)
    {
        var evento = await dbContext.Eventos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
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

        var previousName = evento.Nombre;
        evento.Nombre = request.Nombre.Trim();
        evento.Descripcion = request.Descripcion;
        evento.FechaEventoUtc = request.FechaEventoUtc;
        evento.Estado = request.Estado.Trim();
        evento.ClientePrincipalId = request.ClientePrincipalId;
        evento.ActualizadoEnUtc = DateTime.UtcNow;

        if (!string.Equals(previousName, evento.Nombre, StringComparison.OrdinalIgnoreCase))
        {
            evento.Slug = await CreateUniqueSlugAsync(evento.Nombre, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<EventoResponseDto>.Ok(mapper.Map<EventoResponseDto>(evento), "Evento actualizado.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var evento = await dbContext.Eventos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (evento is null)
        {
            return ApiResponse<bool>.NotFound("Evento no encontrado.");
        }

        dbContext.Eventos.Remove(evento);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Evento eliminado.");
    }

    private async Task<string> CreateUniqueSlugAsync(string name, CancellationToken cancellationToken)
    {
        var baseSlug = FileHelper.CreateSlug(name);
        var slug = baseSlug;
        var index = 1;

        while (await dbContext.Eventos.AnyAsync(x => x.Slug == slug, cancellationToken))
        {
            slug = $"{baseSlug}-{index++}";
        }

        return slug;
    }
}
