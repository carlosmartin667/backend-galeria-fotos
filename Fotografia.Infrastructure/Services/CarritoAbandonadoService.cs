using Fotografia.Application.DTOs.CarritosAbandonados;
using Fotografia.Application.DTOs.Notificaciones;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fotografia.Infrastructure.Services;

public sealed class CarritoAbandonadoService(
    AppDbContext dbContext,
    ICurrentUserService currentUser,
    INotificacionService notificacionService,
    ILogger<CarritoAbandonadoService> logger) : ICarritoAbandonadoService
{
    private const int HorasParaConsiderarAbandonado = 24;
    private const int MaxRecordatorios = 3;

    public async Task<ApiResponse<IReadOnlyCollection<CarritoAbandonadoResponseDto>>> GetAdminAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<IReadOnlyCollection<CarritoAbandonadoResponseDto>>.Forbidden("Solo un administrador puede consultar carritos abandonados.");
        }

        var registros = await QueryRegistros()
            .AsNoTracking()
            .OrderByDescending(x => x.FechaDetectadoUtc)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<CarritoAbandonadoResponseDto>>.Ok(registros.Select(Map).ToList());
    }

    public async Task<ApiResponse<CarritosAbandonadosResumenDto>> GetResumenAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<CarritosAbandonadosResumenDto>.Forbidden("Solo un administrador puede consultar carritos abandonados.");
        }

        var registros = await dbContext.CarritoAbandonadoRegistros
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return ApiResponse<CarritosAbandonadosResumenDto>.Ok(new CarritosAbandonadosResumenDto
        {
            Detectados = registros.Count(x => x.Estado == CarritoAbandonadoEstados.Detectado),
            Notificados = registros.Count(x => x.Estado == CarritoAbandonadoEstados.Notificado),
            Recuperados = registros.Count(x => x.Estado == CarritoAbandonadoEstados.Recuperado),
            Cerrados = registros.Count(x => x.Estado == CarritoAbandonadoEstados.Cerrado),
            TotalActivos = registros.Count(x => x.Activo)
        });
    }

    public async Task<ApiResponse<IReadOnlyCollection<CarritoAbandonadoResponseDto>>> DetectarAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<IReadOnlyCollection<CarritoAbandonadoResponseDto>>.Forbidden("Solo un administrador puede detectar carritos abandonados.");
        }

        var threshold = DateTime.UtcNow.AddHours(-HorasParaConsiderarAbandonado);
        var carritos = await dbContext.CarritosCompra
            .Include(x => x.Items)
            .Include(x => x.Usuario)
            .Where(x => x.Activo
                && x.Estado == CarritoEstados.Activo
                && x.Items.Any()
                && (x.FechaActualizacionUtc ?? x.FechaCreacionUtc) <= threshold)
            .ToListAsync(cancellationToken);

        var carritoIds = carritos.Select(x => x.Id).ToList();
        var existentes = await dbContext.CarritoAbandonadoRegistros
            .Where(x => carritoIds.Contains(x.CarritoCompraId) && x.Activo)
            .Select(x => x.CarritoCompraId)
            .ToListAsync(cancellationToken);
        var existingSet = existentes.ToHashSet();

        var creados = new List<CarritoAbandonadoRegistro>();
        foreach (var carrito in carritos.Where(x => !existingSet.Contains(x.Id)))
        {
            var cliente = await dbContext.Clientes.FirstOrDefaultAsync(x => x.UsuarioId == carrito.UsuarioId, cancellationToken);
            var registro = new CarritoAbandonadoRegistro
            {
                CarritoCompraId = carrito.Id,
                UsuarioId = carrito.UsuarioId,
                ClienteId = cliente?.Id,
                Estado = CarritoAbandonadoEstados.Detectado,
                Activo = true,
                FechaDetectadoUtc = DateTime.UtcNow
            };

            dbContext.CarritoAbandonadoRegistros.Add(registro);
            creados.Add(registro);
        }

        if (creados.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var createdIds = creados.Select(x => x.Id).ToList();
        var response = await QueryRegistros()
            .AsNoTracking()
            .Where(x => createdIds.Contains(x.Id))
            .OrderByDescending(x => x.FechaDetectadoUtc)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<CarritoAbandonadoResponseDto>>.Ok(
            response.Select(Map).ToList(),
            "Deteccion de carritos abandonados finalizada.");
    }

    public async Task<ApiResponse<CarritoAbandonadoResponseDto>> NotificarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<CarritoAbandonadoResponseDto>.Forbidden("Solo un administrador puede notificar carritos abandonados.");
        }

        var registro = await QueryRegistros().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (registro is null)
        {
            return ApiResponse<CarritoAbandonadoResponseDto>.NotFound("Registro de carrito abandonado no encontrado.");
        }

        if (!registro.Activo || registro.Estado == CarritoAbandonadoEstados.Recuperado || registro.Estado == CarritoAbandonadoEstados.Cerrado)
        {
            return ApiResponse<CarritoAbandonadoResponseDto>.Fail("El carrito abandonado no esta activo para notificar.");
        }

        if (registro.NotificacionesEnviadas >= MaxRecordatorios)
        {
            registro.Estado = CarritoAbandonadoEstados.Cerrado;
            registro.Activo = false;
            await dbContext.SaveChangesAsync(cancellationToken);
            return ApiResponse<CarritoAbandonadoResponseDto>.Fail("El carrito ya alcanzo el maximo de recordatorios.");
        }

        var email = registro.Cliente?.Email ?? registro.Usuario?.Email;
        if (string.IsNullOrWhiteSpace(email))
        {
            return ApiResponse<CarritoAbandonadoResponseDto>.Fail("El carrito abandonado no tiene email de contacto.");
        }

        var result = await TryEnqueueCarritoAbandonadoAsync(registro, email, cancellationToken);
        if (!result)
        {
            return ApiResponse<CarritoAbandonadoResponseDto>.Fail("No se pudo encolar el recordatorio de carrito abandonado.");
        }

        var now = DateTime.UtcNow;
        registro.NotificacionesEnviadas++;
        registro.FechaUltimaNotificacionUtc = now;
        registro.Estado = CarritoAbandonadoEstados.Notificado;
        if (registro.CarritoCompra is not null)
        {
            registro.CarritoCompra.RecordatoriosEnviados++;
            registro.CarritoCompra.FechaUltimoRecordatorioUtc = now;
            registro.CarritoCompra.FechaActualizacionUtc = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<CarritoAbandonadoResponseDto>.Ok(Map(registro), "Recordatorio encolado.");
    }

    public async Task MarcarRecuperadoPorCarritoAsync(Guid carritoCompraId, CancellationToken cancellationToken = default)
    {
        var registros = await dbContext.CarritoAbandonadoRegistros
            .Where(x => x.CarritoCompraId == carritoCompraId && x.Activo)
            .ToListAsync(cancellationToken);

        if (registros.Count == 0)
        {
            return;
        }

        foreach (var registro in registros)
        {
            registro.Estado = CarritoAbandonadoEstados.Recuperado;
            registro.Activo = false;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<CarritoAbandonadoRegistro> QueryRegistros()
    {
        return dbContext.CarritoAbandonadoRegistros
            .Include(x => x.Usuario)
            .Include(x => x.Cliente)
            .Include(x => x.CarritoCompra)
            .ThenInclude(x => x!.Items);
    }

    private async Task<bool> TryEnqueueCarritoAbandonadoAsync(
        CarritoAbandonadoRegistro registro,
        string email,
        CancellationToken cancellationToken)
    {
        try
        {
            var total = registro.CarritoCompra?.Items.Sum(x => x.PrecioUnitario * x.Cantidad) ?? 0;
            var result = await notificacionService.EnqueueFromTemplateAsync(new EnqueueTemplateNotificacionRequestDto
            {
                Codigo = NotificacionTipos.CarritoAbandonadoCliente,
                DestinatarioEmail = email,
                UsuarioId = registro.UsuarioId,
                EntidadTipo = "CarritoAbandonado",
                EntidadId = registro.Id,
                CorrelationKey = $"carrito-abandonado:{registro.Id}:recordatorio:{registro.NotificacionesEnviadas + 1}",
                Reemplazos = new Dictionary<string, string?>
                {
                    ["NombreCliente"] = registro.Cliente?.Nombre ?? registro.Usuario?.Nombre ?? "Cliente",
                    ["CantidadItems"] = (registro.CarritoCompra?.Items.Count ?? 0).ToString(),
                    ["Total"] = total.ToString("0.##"),
                    ["Link"] = "Disponible en tu cuenta",
                    ["Fecha"] = DateTime.UtcNow.ToString("yyyy-MM-dd")
                }
            }, cancellationToken);

            if (!result.Success)
            {
                logger.LogWarning("No se pudo encolar recordatorio de carrito abandonado. RegistroId={RegistroId} Motivo={Motivo}", registro.Id, result.Message);
            }

            return result.Success;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "No se pudo encolar recordatorio de carrito abandonado. RegistroId={RegistroId}", registro.Id);
            return false;
        }
    }

    private static CarritoAbandonadoResponseDto Map(CarritoAbandonadoRegistro registro)
    {
        var items = registro.CarritoCompra?.Items ?? Enumerable.Empty<CarritoItem>();
        return new CarritoAbandonadoResponseDto
        {
            Id = registro.Id,
            CarritoCompraId = registro.CarritoCompraId,
            UsuarioId = registro.UsuarioId,
            UsuarioEmail = registro.Usuario?.Email,
            ClienteId = registro.ClienteId,
            ClienteNombre = registro.Cliente?.Nombre,
            FechaDetectadoUtc = registro.FechaDetectadoUtc,
            FechaUltimaNotificacionUtc = registro.FechaUltimaNotificacionUtc,
            NotificacionesEnviadas = registro.NotificacionesEnviadas,
            Estado = registro.Estado,
            Activo = registro.Activo,
            CantidadItems = items.Count(),
            TotalEstimado = items.Sum(x => x.PrecioUnitario * x.Cantidad)
        };
    }
}
