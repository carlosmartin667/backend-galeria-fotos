using Fotografia.Application.DTOs.NotasInternas;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Services;

public sealed class NotaInternaService(
    AppDbContext dbContext,
    ICurrentUserService currentUser) : INotaInternaService
{
    public async Task<ApiResponse<IReadOnlyCollection<NotaInternaResponseDto>>> GetByEntidadAsync(
        string entidadTipo,
        Guid entidadId,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<IReadOnlyCollection<NotaInternaResponseDto>>.Forbidden("Solo un administrador puede consultar notas internas.");
        }

        var validation = await ValidateTargetAsync(entidadTipo, entidadId, cancellationToken);
        if (!validation.Success)
        {
            return ApiResponse<IReadOnlyCollection<NotaInternaResponseDto>>.Fail(validation.Message, validation.StatusCode);
        }

        var notas = await QueryNotas()
            .AsNoTracking()
            .Where(x => x.EntidadTipo == validation.Tipo && x.EntidadId == entidadId && x.Activa)
            .OrderByDescending(x => x.FechaCreacionUtc)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<NotaInternaResponseDto>>.Ok(notas.Select(MapNota).ToList());
    }

    public async Task<ApiResponse<NotaInternaResponseDto>> CreateAsync(
        string entidadTipo,
        Guid entidadId,
        CrearNotaInternaRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin || currentUser.UserId is not Guid userId)
        {
            return ApiResponse<NotaInternaResponseDto>.Forbidden("Solo un administrador puede crear notas internas.");
        }

        var validation = await ValidateTargetAsync(entidadTipo, entidadId, cancellationToken);
        if (!validation.Success)
        {
            return ApiResponse<NotaInternaResponseDto>.Fail(validation.Message, validation.StatusCode);
        }

        var nota = new NotaInterna
        {
            EntidadTipo = validation.Tipo!,
            EntidadId = entidadId,
            Texto = request.Texto.Trim(),
            UsuarioId = userId,
            Activa = true,
            FechaCreacionUtc = DateTime.UtcNow
        };

        dbContext.NotasInternas.Add(nota);
        await dbContext.SaveChangesAsync(cancellationToken);

        var created = await QueryNotas()
            .AsNoTracking()
            .FirstAsync(x => x.Id == nota.Id, cancellationToken);

        return ApiResponse<NotaInternaResponseDto>.Ok(MapNota(created), "Nota interna creada.");
    }

    public async Task<ApiResponse<NotaInternaResponseDto>> UpdateAsync(
        Guid id,
        ActualizarNotaInternaRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<NotaInternaResponseDto>.Forbidden("Solo un administrador puede editar notas internas.");
        }

        var nota = await dbContext.NotasInternas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (nota is null)
        {
            return ApiResponse<NotaInternaResponseDto>.NotFound("Nota interna no encontrada.");
        }

        nota.Texto = request.Texto.Trim();
        nota.Activa = request.Activa;
        nota.FechaActualizacionUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        var updated = await QueryNotas()
            .AsNoTracking()
            .FirstAsync(x => x.Id == nota.Id, cancellationToken);

        return ApiResponse<NotaInternaResponseDto>.Ok(MapNota(updated), "Nota interna actualizada.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<bool>.Forbidden("Solo un administrador puede eliminar notas internas.");
        }

        var nota = await dbContext.NotasInternas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (nota is null)
        {
            return ApiResponse<bool>.NotFound("Nota interna no encontrada.");
        }

        nota.Activa = false;
        nota.FechaActualizacionUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Nota interna desactivada.");
    }

    private IQueryable<NotaInterna> QueryNotas()
    {
        return dbContext.NotasInternas.Include(x => x.Usuario);
    }

    private async Task<TargetValidationResult> ValidateTargetAsync(
        string entidadTipo,
        Guid entidadId,
        CancellationToken cancellationToken)
    {
        if (!NotaInternaTipos.TryNormalize(entidadTipo, out var tipo))
        {
            return TargetValidationResult.Fail("Tipo de entidad invalido.", 400);
        }

        var exists = tipo switch
        {
            NotaInternaTipos.Cliente => await dbContext.Clientes.AnyAsync(x => x.Id == entidadId, cancellationToken),
            NotaInternaTipos.Pedido => await dbContext.Pedidos.AnyAsync(x => x.Id == entidadId, cancellationToken),
            NotaInternaTipos.Evento => await dbContext.Eventos.AnyAsync(x => x.Id == entidadId, cancellationToken),
            NotaInternaTipos.SesionPrivada => await dbContext.SesionesPrivadas.AnyAsync(x => x.Id == entidadId, cancellationToken),
            NotaInternaTipos.SolicitudPresupuesto => await dbContext.SolicitudesPresupuesto.AnyAsync(x => x.Id == entidadId, cancellationToken),
            NotaInternaTipos.AgendaItem => await dbContext.AgendaItems.AnyAsync(x => x.Id == entidadId, cancellationToken),
            _ => false
        };

        return exists
            ? TargetValidationResult.Ok(tipo)
            : TargetValidationResult.Fail("Entidad asociada no encontrada.", 404);
    }

    private static NotaInternaResponseDto MapNota(NotaInterna nota)
    {
        return new NotaInternaResponseDto
        {
            Id = nota.Id,
            EntidadTipo = nota.EntidadTipo,
            EntidadId = nota.EntidadId,
            Texto = nota.Texto,
            UsuarioId = nota.UsuarioId,
            UsuarioNombre = nota.Usuario?.Nombre,
            Activa = nota.Activa,
            FechaCreacionUtc = nota.FechaCreacionUtc,
            FechaActualizacionUtc = nota.FechaActualizacionUtc
        };
    }

    private sealed record TargetValidationResult(bool Success, string? Tipo, string Message, int StatusCode)
    {
        public static TargetValidationResult Ok(string tipo)
        {
            return new TargetValidationResult(true, tipo, string.Empty, 200);
        }

        public static TargetValidationResult Fail(string message, int statusCode)
        {
            return new TargetValidationResult(false, null, message, statusCode);
        }
    }
}
