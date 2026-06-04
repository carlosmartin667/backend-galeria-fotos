using Fotografia.Application.DTOs.Cupones;
using Fotografia.Application.DTOs.Notificaciones;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fotografia.Infrastructure.Services;

public sealed class CuponService(
    AppDbContext dbContext,
    ICurrentUserService currentUser,
    IBitacoraService bitacoraService,
    INotificacionService notificacionService,
    ILogger<CuponService> logger) : ICuponService
{
    public async Task<ApiResponse<IReadOnlyCollection<CuponDescuentoResponseDto>>> GetAdminAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<IReadOnlyCollection<CuponDescuentoResponseDto>>.Forbidden("Solo un administrador puede consultar cupones.");
        }

        var cupones = await dbContext.CuponesDescuento
            .AsNoTracking()
            .OrderBy(x => x.Codigo)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<CuponDescuentoResponseDto>>.Ok(cupones.Select(MapCupon).ToList());
    }

    public async Task<ApiResponse<CuponDescuentoResponseDto>> GetAdminByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<CuponDescuentoResponseDto>.Forbidden("Solo un administrador puede consultar cupones.");
        }

        var cupon = await dbContext.CuponesDescuento.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return cupon is null
            ? ApiResponse<CuponDescuentoResponseDto>.NotFound("Cupon no encontrado.")
            : ApiResponse<CuponDescuentoResponseDto>.Ok(MapCupon(cupon));
    }

    public async Task<ApiResponse<CuponDescuentoResponseDto>> CreateAsync(CrearCuponDescuentoRequestDto request, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<CuponDescuentoResponseDto>.Forbidden("Solo un administrador puede crear cupones.");
        }

        var codigo = NormalizeCode(request.Codigo);
        var validation = ValidateDefinition(request.TipoDescuento, request.ValorDescuento, request.FechaInicioUtc, request.FechaFinUtc);
        if (validation is not null)
        {
            return ApiResponse<CuponDescuentoResponseDto>.Fail(validation);
        }

        if (await dbContext.CuponesDescuento.AnyAsync(x => x.Codigo == codigo, cancellationToken))
        {
            return ApiResponse<CuponDescuentoResponseDto>.Fail("Ya existe un cupon con ese codigo.");
        }

        CuponTipos.TryNormalize(request.TipoDescuento, out var tipo);
        var cupon = new CuponDescuento
        {
            Codigo = codigo,
            Descripcion = Normalize(request.Descripcion),
            TipoDescuento = tipo,
            ValorDescuento = request.ValorDescuento,
            MontoMinimoCompra = request.MontoMinimoCompra,
            MontoMaximoDescuento = request.MontoMaximoDescuento,
            FechaInicioUtc = request.FechaInicioUtc,
            FechaFinUtc = request.FechaFinUtc,
            UsosMaximos = request.UsosMaximos,
            UsosMaximosPorUsuario = request.UsosMaximosPorUsuario,
            SoloPrimerCompra = request.SoloPrimerCompra,
            Activo = request.Activo,
            FechaCreacionUtc = DateTime.UtcNow
        };

        dbContext.CuponesDescuento.Add(cupon);
        await dbContext.SaveChangesAsync(cancellationToken);

        await bitacoraService.RegistrarInfoAsync(
            BitacoraAcciones.CuponCreado,
            BitacoraEntidades.Cupon,
            cupon.Id,
            "Cupon creado.",
            new
            {
                cupon.Id,
                cupon.Codigo,
                cupon.TipoDescuento,
                cupon.ValorDescuento,
                cupon.Activo,
                cupon.FechaInicioUtc,
                cupon.FechaFinUtc
            },
            cancellationToken);

        return ApiResponse<CuponDescuentoResponseDto>.Ok(MapCupon(cupon), "Cupon creado.");
    }

    public async Task<ApiResponse<CuponDescuentoResponseDto>> UpdateAsync(Guid id, ActualizarCuponDescuentoRequestDto request, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<CuponDescuentoResponseDto>.Forbidden("Solo un administrador puede editar cupones.");
        }

        var validation = ValidateDefinition(request.TipoDescuento, request.ValorDescuento, request.FechaInicioUtc, request.FechaFinUtc);
        if (validation is not null)
        {
            return ApiResponse<CuponDescuentoResponseDto>.Fail(validation);
        }

        var cupon = await dbContext.CuponesDescuento.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (cupon is null)
        {
            return ApiResponse<CuponDescuentoResponseDto>.NotFound("Cupon no encontrado.");
        }

        CuponTipos.TryNormalize(request.TipoDescuento, out var tipo);
        cupon.Descripcion = Normalize(request.Descripcion);
        cupon.TipoDescuento = tipo;
        cupon.ValorDescuento = request.ValorDescuento;
        cupon.MontoMinimoCompra = request.MontoMinimoCompra;
        cupon.MontoMaximoDescuento = request.MontoMaximoDescuento;
        cupon.FechaInicioUtc = request.FechaInicioUtc;
        cupon.FechaFinUtc = request.FechaFinUtc;
        cupon.UsosMaximos = request.UsosMaximos;
        cupon.UsosMaximosPorUsuario = request.UsosMaximosPorUsuario;
        cupon.SoloPrimerCompra = request.SoloPrimerCompra;
        cupon.Activo = request.Activo;
        cupon.FechaActualizacionUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<CuponDescuentoResponseDto>.Ok(MapCupon(cupon), "Cupon actualizado.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<bool>.Forbidden("Solo un administrador puede desactivar cupones.");
        }

        var cupon = await dbContext.CuponesDescuento.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (cupon is null)
        {
            return ApiResponse<bool>.NotFound("Cupon no encontrado.");
        }

        cupon.Activo = false;
        cupon.FechaActualizacionUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Cupon desactivado.");
    }

    public Task<ApiResponse<CuponDescuentoResponseDto>> ActivarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return SetActivoAsync(id, true, "Cupon activado.", cancellationToken);
    }

    public Task<ApiResponse<CuponDescuentoResponseDto>> DesactivarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return SetActivoAsync(id, false, "Cupon desactivado.", cancellationToken);
    }

    public async Task<ApiResponse<IReadOnlyCollection<CuponUsoResponseDto>>> GetUsosAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<IReadOnlyCollection<CuponUsoResponseDto>>.Forbidden("Solo un administrador puede consultar usos.");
        }

        var exists = await dbContext.CuponesDescuento.AnyAsync(x => x.Id == id, cancellationToken);
        if (!exists)
        {
            return ApiResponse<IReadOnlyCollection<CuponUsoResponseDto>>.NotFound("Cupon no encontrado.");
        }

        var usos = await dbContext.CuponUsos
            .AsNoTracking()
            .Where(x => x.CuponDescuentoId == id)
            .OrderByDescending(x => x.FechaUsoUtc)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<CuponUsoResponseDto>>.Ok(usos.Select(MapUso).ToList());
    }

    public async Task<ApiResponse<CuponValidacionResponseDto>> ValidarAsync(ValidarCuponRequestDto request, CancellationToken cancellationToken = default)
    {
        var subtotal = Math.Max(0, request.Subtotal ?? 0);
        Guid? clienteId = null;
        if (currentUser.UserId is Guid userId)
        {
            clienteId = await dbContext.Clientes
                .Where(x => x.UsuarioId == userId)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return await ValidateInternalAsync(request.Codigo, subtotal, clienteId, currentUser.UserId, cancellationToken);
    }

    public Task<ApiResponse<CuponValidacionResponseDto>> ValidarParaClienteAsync(string codigo, decimal subtotal, Guid clienteId, Guid? usuarioId, CancellationToken cancellationToken = default)
    {
        return ValidateInternalAsync(codigo, subtotal, clienteId, usuarioId, cancellationToken);
    }

    public async Task<ApiResponse<CuponUsoResponseDto>> RegistrarUsoPendienteAsync(
        Guid cuponDescuentoId,
        Guid pedidoId,
        Guid clienteId,
        Guid? usuarioId,
        string codigo,
        decimal montoDescuento,
        CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.CuponUsos.FirstOrDefaultAsync(x => x.PedidoId == pedidoId && x.CuponDescuentoId == cuponDescuentoId, cancellationToken);
        if (existing is not null)
        {
            return ApiResponse<CuponUsoResponseDto>.Ok(MapUso(existing), "Uso de cupon ya registrado.");
        }

        var uso = new CuponUso
        {
            CuponDescuentoId = cuponDescuentoId,
            PedidoId = pedidoId,
            ClienteId = clienteId,
            UsuarioId = usuarioId,
            Codigo = NormalizeCode(codigo),
            MontoDescuento = montoDescuento,
            FechaUsoUtc = DateTime.UtcNow,
            Confirmado = false
        };

        dbContext.CuponUsos.Add(uso);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<CuponUsoResponseDto>.Ok(MapUso(uso), "Uso de cupon reservado.");
    }

    public async Task ConfirmarUsoPorPedidoAsync(Guid pedidoId, CancellationToken cancellationToken = default)
    {
        var usos = await dbContext.CuponUsos
            .Include(x => x.CuponDescuento)
            .Where(x => x.PedidoId == pedidoId && !x.Confirmado)
            .ToListAsync(cancellationToken);

        if (usos.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var uso in usos)
        {
            uso.Confirmado = true;
            uso.FechaUsoUtc = now;
            if (uso.CuponDescuento is not null)
            {
                uso.CuponDescuento.UsosActuales++;
                uso.CuponDescuento.FechaActualizacionUtc = now;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var uso in usos)
        {
            await bitacoraService.RegistrarInfoAsync(
                BitacoraAcciones.CuponUsado,
                BitacoraEntidades.Cupon,
                uso.CuponDescuentoId,
                "Uso de cupon confirmado.",
                new
                {
                    CuponUsoId = uso.Id,
                    uso.CuponDescuentoId,
                    uso.PedidoId,
                    uso.ClienteId,
                    uso.UsuarioId,
                    uso.Codigo,
                    uso.MontoDescuento
                },
                cancellationToken);

            await TryEnqueueCuponUsadoAdminAsync(uso, cancellationToken);
        }
    }

    private async Task<ApiResponse<CuponDescuentoResponseDto>> SetActivoAsync(Guid id, bool activo, string message, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<CuponDescuentoResponseDto>.Forbidden("Solo un administrador puede cambiar cupones.");
        }

        var cupon = await dbContext.CuponesDescuento.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (cupon is null)
        {
            return ApiResponse<CuponDescuentoResponseDto>.NotFound("Cupon no encontrado.");
        }

        cupon.Activo = activo;
        cupon.FechaActualizacionUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<CuponDescuentoResponseDto>.Ok(MapCupon(cupon), message);
    }

    private async Task<ApiResponse<CuponValidacionResponseDto>> ValidateInternalAsync(string codigoValue, decimal subtotal, Guid? clienteId, Guid? usuarioId, CancellationToken cancellationToken)
    {
        var codigo = NormalizeCode(codigoValue);
        if (string.IsNullOrWhiteSpace(codigo))
        {
            return ApiResponse<CuponValidacionResponseDto>.Fail("Codigo de cupon requerido.");
        }

        var cupon = await dbContext.CuponesDescuento
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Codigo == codigo, cancellationToken);

        if (cupon is null)
        {
            return ApiResponse<CuponValidacionResponseDto>.NotFound("Cupon no encontrado.");
        }

        var now = DateTime.UtcNow;
        var reason = await GetInvalidReasonAsync(cupon, subtotal, clienteId, usuarioId, now, cancellationToken);
        if (reason is not null)
        {
            return ApiResponse<CuponValidacionResponseDto>.Fail(reason);
        }

        var discount = CalculateDiscount(cupon, subtotal);
        await bitacoraService.RegistrarInfoAsync(
            BitacoraAcciones.CuponAplicado,
            BitacoraEntidades.Cupon,
            cupon.Id,
            "Cupon validado para aplicar.",
            new
            {
                cupon.Id,
                cupon.Codigo,
                clienteId,
                usuarioId,
                Subtotal = subtotal,
                Descuento = discount,
                TotalFinal = Math.Max(0, subtotal - discount)
            },
            cancellationToken);

        return ApiResponse<CuponValidacionResponseDto>.Ok(new CuponValidacionResponseDto
        {
            Valido = true,
            Codigo = cupon.Codigo,
            Subtotal = subtotal,
            Descuento = discount,
            TotalFinal = Math.Max(0, subtotal - discount),
            CuponDescuentoId = cupon.Id,
            Mensaje = "Cupon valido."
        });
    }

    private async Task<string?> GetInvalidReasonAsync(CuponDescuento cupon, decimal subtotal, Guid? clienteId, Guid? usuarioId, DateTime now, CancellationToken cancellationToken)
    {
        if (!cupon.Activo)
        {
            return "El cupon no esta activo.";
        }

        if (cupon.FechaInicioUtc is DateTime inicio && inicio > now)
        {
            return "El cupon todavia no esta vigente.";
        }

        if (cupon.FechaFinUtc is DateTime fin && fin < now)
        {
            return "El cupon esta vencido.";
        }

        if (cupon.UsosMaximos is int maxUsos && cupon.UsosActuales >= maxUsos)
        {
            return "El cupon supero sus usos maximos.";
        }

        if (cupon.MontoMinimoCompra is decimal minimo && subtotal < minimo)
        {
            return "El subtotal no alcanza el monto minimo del cupon.";
        }

        if (cupon.SoloPrimerCompra && clienteId is Guid soloPrimerClienteId)
        {
            var pedidosCliente = await dbContext.Pedidos
                .AsNoTracking()
                .Include(x => x.Pago)
                .Where(x => x.ClienteId == soloPrimerClienteId)
                .Select(x => new { x.Estado, PagoEstado = x.Pago == null ? null : x.Pago.Estado })
                .ToListAsync(cancellationToken);

            var hasPaidOrder = pedidosCliente.Any(x => PedidoEstados.EsPagado(x.Estado, x.PagoEstado));
            if (hasPaidOrder)
            {
                return "El cupon solo aplica a la primera compra.";
            }
        }

        if (cupon.UsosMaximosPorUsuario is int maxPorUsuario)
        {
            var query = dbContext.CuponUsos.AsNoTracking().Where(x => x.CuponDescuentoId == cupon.Id && x.Confirmado);
            if (clienteId is Guid cid)
            {
                query = query.Where(x => x.ClienteId == cid);
            }
            else if (usuarioId is Guid uid)
            {
                query = query.Where(x => x.UsuarioId == uid);
            }
            else
            {
                return "Debe iniciar sesion para usar este cupon.";
            }

            var usos = await query.CountAsync(cancellationToken);
            if (usos >= maxPorUsuario)
            {
                return "El usuario supero los usos permitidos para este cupon.";
            }
        }

        var discount = CalculateDiscount(cupon, subtotal);
        return discount <= 0 ? "El cupon no genera descuento para este subtotal." : null;
    }

    private static decimal CalculateDiscount(CuponDescuento cupon, decimal subtotal)
    {
        var discount = cupon.TipoDescuento == CuponTipos.Porcentaje
            ? subtotal * cupon.ValorDescuento / 100m
            : cupon.ValorDescuento;

        if (cupon.MontoMaximoDescuento is decimal max)
        {
            discount = Math.Min(discount, max);
        }

        return Math.Round(Math.Min(Math.Max(0, discount), subtotal), 2);
    }

    private static string? ValidateDefinition(string tipoValue, decimal valor, DateTime? inicio, DateTime? fin)
    {
        if (!CuponTipos.TryNormalize(tipoValue, out var tipo))
        {
            return "Tipo de descuento invalido.";
        }

        if (tipo == CuponTipos.Porcentaje && (valor < 1 || valor > 100))
        {
            return "El porcentaje debe estar entre 1 y 100.";
        }

        if (tipo == CuponTipos.MontoFijo && valor <= 0)
        {
            return "El monto fijo debe ser mayor a 0.";
        }

        if (inicio is not null && fin is not null && fin <= inicio)
        {
            return "FechaFinUtc debe ser mayor a FechaInicioUtc.";
        }

        return null;
    }

    private async Task TryEnqueueCuponUsadoAdminAsync(CuponUso uso, CancellationToken cancellationToken)
    {
        try
        {
            var result = await notificacionService.EnqueueFromTemplateAsync(new EnqueueTemplateNotificacionRequestDto
            {
                Codigo = NotificacionTipos.CuponUsadoAdmin,
                EntidadTipo = "CuponUso",
                EntidadId = uso.Id,
                CorrelationKey = $"cupon-uso:{uso.Id}:admin",
                Reemplazos = new Dictionary<string, string?>
                {
                    ["Codigo"] = uso.Codigo,
                    ["PedidoId"] = uso.PedidoId.ToString(),
                    ["Descuento"] = uso.MontoDescuento.ToString("0.##"),
                    ["Fecha"] = uso.FechaUsoUtc.ToString("yyyy-MM-dd")
                }
            }, cancellationToken);

            if (!result.Success)
            {
                logger.LogWarning(
                    "No se pudo encolar notificacion de cupon usado. CuponUsoId={CuponUsoId} Motivo={Motivo}",
                    uso.Id,
                    result.Message);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "No se pudo encolar notificacion de cupon usado. CuponUsoId={CuponUsoId}", uso.Id);
        }
    }

    private static CuponDescuentoResponseDto MapCupon(CuponDescuento cupon)
    {
        return new CuponDescuentoResponseDto
        {
            Id = cupon.Id,
            Codigo = cupon.Codigo,
            Descripcion = cupon.Descripcion,
            TipoDescuento = cupon.TipoDescuento,
            ValorDescuento = cupon.ValorDescuento,
            MontoMinimoCompra = cupon.MontoMinimoCompra,
            MontoMaximoDescuento = cupon.MontoMaximoDescuento,
            FechaInicioUtc = cupon.FechaInicioUtc,
            FechaFinUtc = cupon.FechaFinUtc,
            UsosMaximos = cupon.UsosMaximos,
            UsosActuales = cupon.UsosActuales,
            UsosMaximosPorUsuario = cupon.UsosMaximosPorUsuario,
            SoloPrimerCompra = cupon.SoloPrimerCompra,
            Activo = cupon.Activo,
            FechaCreacionUtc = cupon.FechaCreacionUtc,
            FechaActualizacionUtc = cupon.FechaActualizacionUtc
        };
    }

    private static CuponUsoResponseDto MapUso(CuponUso uso)
    {
        return new CuponUsoResponseDto
        {
            Id = uso.Id,
            CuponDescuentoId = uso.CuponDescuentoId,
            PedidoId = uso.PedidoId,
            ClienteId = uso.ClienteId,
            UsuarioId = uso.UsuarioId,
            Codigo = uso.Codigo,
            MontoDescuento = uso.MontoDescuento,
            FechaUsoUtc = uso.FechaUsoUtc,
            Confirmado = uso.Confirmado
        };
    }

    private static string NormalizeCode(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
