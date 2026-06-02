using Fotografia.Application.DTOs.Notificaciones;
using Fotografia.Application.DTOs.Testimonios;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fotografia.Infrastructure.Services;

public sealed class TestimonioService(
    AppDbContext dbContext,
    ICurrentUserService currentUser,
    INotificacionService notificacionService,
    ILogger<TestimonioService> logger) : ITestimonioService
{
    public async Task<ApiResponse<IReadOnlyCollection<TestimonioPublicoResponseDto>>> GetPublicAsync(CancellationToken cancellationToken = default)
    {
        var testimonios = await dbContext.Testimonios
            .AsNoTracking()
            .Where(x => x.Activo && x.Publicado)
            .OrderByDescending(x => x.Destacado)
            .ThenByDescending(x => x.FechaCreacionUtc)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<TestimonioPublicoResponseDto>>.Ok(testimonios.Select(MapPublic).ToList());
    }

    public async Task<ApiResponse<IReadOnlyCollection<TestimonioPublicoResponseDto>>> GetDestacadosAsync(CancellationToken cancellationToken = default)
    {
        var testimonios = await dbContext.Testimonios
            .AsNoTracking()
            .Where(x => x.Activo && x.Publicado && x.Destacado)
            .OrderByDescending(x => x.FechaCreacionUtc)
            .Take(12)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<TestimonioPublicoResponseDto>>.Ok(testimonios.Select(MapPublic).ToList());
    }

    public async Task<ApiResponse<TestimonioAdminResponseDto>> CreateAsync(CrearTestimonioRequestDto request, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateCreateAsync(request, cancellationToken);
        if (validation is not null)
        {
            return ApiResponse<TestimonioAdminResponseDto>.Fail(validation);
        }

        var testimonio = new Testimonio
        {
            NombreCliente = request.NombreCliente.Trim(),
            EmailCliente = Normalize(request.EmailCliente),
            Texto = request.Texto.Trim(),
            Calificacion = request.Calificacion,
            ImagenUrl = Normalize(request.ImagenUrl),
            ClienteId = request.ClienteId,
            PedidoId = request.PedidoId,
            ServicioFotografiaId = request.ServicioFotografiaId,
            EventoId = request.EventoId,
            Publicado = false,
            Destacado = false,
            Activo = true,
            FechaCreacionUtc = DateTime.UtcNow
        };

        dbContext.Testimonios.Add(testimonio);
        await dbContext.SaveChangesAsync(cancellationToken);
        await TryEnqueueTestimonioRecibidoAdminAsync(testimonio, cancellationToken);

        return ApiResponse<TestimonioAdminResponseDto>.Ok(MapAdmin(testimonio), "Testimonio recibido.");
    }

    public async Task<ApiResponse<IReadOnlyCollection<TestimonioAdminResponseDto>>> GetAdminAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<IReadOnlyCollection<TestimonioAdminResponseDto>>.Forbidden("Solo un administrador puede consultar testimonios.");
        }

        var testimonios = await dbContext.Testimonios
            .AsNoTracking()
            .OrderByDescending(x => x.FechaCreacionUtc)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<TestimonioAdminResponseDto>>.Ok(testimonios.Select(MapAdmin).ToList());
    }

    public async Task<ApiResponse<TestimonioAdminResponseDto>> GetAdminByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<TestimonioAdminResponseDto>.Forbidden("Solo un administrador puede consultar testimonios.");
        }

        var testimonio = await dbContext.Testimonios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return testimonio is null
            ? ApiResponse<TestimonioAdminResponseDto>.NotFound("Testimonio no encontrado.")
            : ApiResponse<TestimonioAdminResponseDto>.Ok(MapAdmin(testimonio));
    }

    public async Task<ApiResponse<TestimonioAdminResponseDto>> UpdateAdminAsync(Guid id, ActualizarTestimonioAdminRequestDto request, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<TestimonioAdminResponseDto>.Forbidden("Solo un administrador puede editar testimonios.");
        }

        var validation = await ValidateRelationsAsync(request.ClienteId, request.PedidoId, request.ServicioFotografiaId, request.EventoId, cancellationToken);
        if (validation is not null)
        {
            return ApiResponse<TestimonioAdminResponseDto>.Fail(validation);
        }

        var testimonio = await dbContext.Testimonios.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (testimonio is null)
        {
            return ApiResponse<TestimonioAdminResponseDto>.NotFound("Testimonio no encontrado.");
        }

        testimonio.NombreCliente = request.NombreCliente.Trim();
        testimonio.EmailCliente = Normalize(request.EmailCliente);
        testimonio.Texto = request.Texto.Trim();
        testimonio.Calificacion = request.Calificacion;
        testimonio.ImagenUrl = Normalize(request.ImagenUrl);
        testimonio.Publicado = request.Publicado;
        testimonio.Destacado = request.Destacado;
        testimonio.Activo = request.Activo;
        testimonio.ClienteId = request.ClienteId;
        testimonio.PedidoId = request.PedidoId;
        testimonio.ServicioFotografiaId = request.ServicioFotografiaId;
        testimonio.EventoId = request.EventoId;
        testimonio.FechaActualizacionUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<TestimonioAdminResponseDto>.Ok(MapAdmin(testimonio), "Testimonio actualizado.");
    }

    public Task<ApiResponse<TestimonioAdminResponseDto>> PublicarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return SetPublicadoAsync(id, true, "Testimonio publicado.", cancellationToken);
    }

    public Task<ApiResponse<TestimonioAdminResponseDto>> OcultarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return SetPublicadoAsync(id, false, "Testimonio ocultado.", cancellationToken);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<bool>.Forbidden("Solo un administrador puede eliminar testimonios.");
        }

        var testimonio = await dbContext.Testimonios.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (testimonio is null)
        {
            return ApiResponse<bool>.NotFound("Testimonio no encontrado.");
        }

        testimonio.Activo = false;
        testimonio.Publicado = false;
        testimonio.FechaActualizacionUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Testimonio desactivado.");
    }

    private async Task<ApiResponse<TestimonioAdminResponseDto>> SetPublicadoAsync(Guid id, bool publicado, string message, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<TestimonioAdminResponseDto>.Forbidden("Solo un administrador puede publicar testimonios.");
        }

        var testimonio = await dbContext.Testimonios.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (testimonio is null)
        {
            return ApiResponse<TestimonioAdminResponseDto>.NotFound("Testimonio no encontrado.");
        }

        testimonio.Publicado = publicado;
        testimonio.Activo = true;
        testimonio.FechaActualizacionUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<TestimonioAdminResponseDto>.Ok(MapAdmin(testimonio), message);
    }

    private async Task<string?> ValidateCreateAsync(CrearTestimonioRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NombreCliente))
        {
            return "NombreCliente es requerido.";
        }

        if (string.IsNullOrWhiteSpace(request.Texto))
        {
            return "Texto es requerido.";
        }

        if (request.Calificacion is < 1 or > 5)
        {
            return "Calificacion debe estar entre 1 y 5.";
        }

        if (!currentUser.IsAdmin && currentUser.IsAuthenticated && request.ClienteId is Guid clienteId)
        {
            var ownsCliente = await dbContext.Clientes
                .AnyAsync(x => x.Id == clienteId && x.UsuarioId == currentUser.UserId, cancellationToken);
            if (!ownsCliente)
            {
                return "No puede crear testimonios para otro cliente.";
            }
        }

        return await ValidateRelationsAsync(request.ClienteId, request.PedidoId, request.ServicioFotografiaId, request.EventoId, cancellationToken);
    }

    private async Task<string?> ValidateRelationsAsync(
        Guid? clienteId,
        Guid? pedidoId,
        Guid? servicioFotografiaId,
        Guid? eventoId,
        CancellationToken cancellationToken)
    {
        if (clienteId is Guid cid && !await dbContext.Clientes.AnyAsync(x => x.Id == cid, cancellationToken))
        {
            return "Cliente asociado no encontrado.";
        }

        if (pedidoId is Guid pid && !await dbContext.Pedidos.AnyAsync(x => x.Id == pid, cancellationToken))
        {
            return "Pedido asociado no encontrado.";
        }

        if (servicioFotografiaId is Guid sid && !await dbContext.ServiciosFotografia.AnyAsync(x => x.Id == sid, cancellationToken))
        {
            return "Servicio asociado no encontrado.";
        }

        if (eventoId is Guid eid && !await dbContext.Eventos.AnyAsync(x => x.Id == eid, cancellationToken))
        {
            return "Evento asociado no encontrado.";
        }

        return null;
    }

    private async Task TryEnqueueTestimonioRecibidoAdminAsync(Testimonio testimonio, CancellationToken cancellationToken)
    {
        try
        {
            var result = await notificacionService.EnqueueFromTemplateAsync(new EnqueueTemplateNotificacionRequestDto
            {
                Codigo = NotificacionTipos.TestimonioRecibidoAdmin,
                EntidadTipo = "Testimonio",
                EntidadId = testimonio.Id,
                CorrelationKey = $"testimonio:{testimonio.Id}:recibido:admin",
                Reemplazos = new Dictionary<string, string?>
                {
                    ["NombreCliente"] = testimonio.NombreCliente,
                    ["Calificacion"] = testimonio.Calificacion.ToString(),
                    ["Fecha"] = testimonio.FechaCreacionUtc.ToString("yyyy-MM-dd")
                }
            }, cancellationToken);

            if (!result.Success)
            {
                logger.LogWarning("No se pudo encolar notificacion de testimonio recibido. TestimonioId={TestimonioId} Motivo={Motivo}", testimonio.Id, result.Message);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "No se pudo encolar notificacion de testimonio recibido. TestimonioId={TestimonioId}", testimonio.Id);
        }
    }

    private static TestimonioPublicoResponseDto MapPublic(Testimonio testimonio)
    {
        return new TestimonioPublicoResponseDto
        {
            Id = testimonio.Id,
            NombreCliente = testimonio.NombreCliente,
            Texto = testimonio.Texto,
            Calificacion = testimonio.Calificacion,
            ImagenUrl = testimonio.ImagenUrl,
            Destacado = testimonio.Destacado,
            ServicioFotografiaId = testimonio.ServicioFotografiaId,
            EventoId = testimonio.EventoId,
            FechaCreacionUtc = testimonio.FechaCreacionUtc
        };
    }

    private static TestimonioAdminResponseDto MapAdmin(Testimonio testimonio)
    {
        return new TestimonioAdminResponseDto
        {
            Id = testimonio.Id,
            NombreCliente = testimonio.NombreCliente,
            EmailCliente = testimonio.EmailCliente,
            Texto = testimonio.Texto,
            Calificacion = testimonio.Calificacion,
            ImagenUrl = testimonio.ImagenUrl,
            Publicado = testimonio.Publicado,
            Destacado = testimonio.Destacado,
            Activo = testimonio.Activo,
            ClienteId = testimonio.ClienteId,
            PedidoId = testimonio.PedidoId,
            ServicioFotografiaId = testimonio.ServicioFotografiaId,
            EventoId = testimonio.EventoId,
            FechaCreacionUtc = testimonio.FechaCreacionUtc,
            FechaActualizacionUtc = testimonio.FechaActualizacionUtc
        };
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
