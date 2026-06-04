using Fotografia.Infrastructure.Data;
using Fotografia.Application.DTOs.Notificaciones;
using Fotografia.Application.DTOs.Descargas;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fotografia.Infrastructure.Services;

public sealed class DescargaService(
    AppDbContext dbContext,
    IStorageService storageService,
    ICurrentUserService currentUser,
    IBitacoraService bitacoraService,
    INotificacionService notificacionService,
    ILogger<DescargaService> logger) : IDescargaService
{
    private const int DefaultMaxDescargas = 5;

    public async Task<ApiResponse<IReadOnlyCollection<DescargaResponseDto>>> GetMineAsync(
        CancellationToken cancellationToken = default)
    {
        var query = QueryDescargas().AsNoTracking();

        if (!currentUser.IsAdmin)
        {
            if (currentUser.UserId is not Guid userId)
            {
                return ApiResponse<IReadOnlyCollection<DescargaResponseDto>>.Forbidden("Debe iniciar sesion.");
            }

            query = query.Where(x =>
                (x.Cliente != null && x.Cliente.UsuarioId == userId)
                || (x.Pedido != null && x.Pedido.Cliente != null && x.Pedido.Cliente.UsuarioId == userId));
        }

        var descargas = await query
            .OrderByDescending(x => x.CreadoEnUtc)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<DescargaResponseDto>>.Ok(
            descargas.Select(MapDescarga).ToList());
    }

    public async Task<ApiResponse<IReadOnlyCollection<DescargaResponseDto>>> GetAdminAsync(
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<IReadOnlyCollection<DescargaResponseDto>>.Forbidden("Solo un administrador puede consultar todas las descargas.");
        }

        var descargas = await QueryDescargas()
            .AsNoTracking()
            .OrderByDescending(x => x.CreadoEnUtc)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<DescargaResponseDto>>.Ok(
            descargas.Select(MapDescarga).ToList());
    }

    public async Task<ApiResponse<DescargaResponseDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var descarga = await QueryDescargas()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (descarga is null)
        {
            return ApiResponse<DescargaResponseDto>.NotFound("Descarga no encontrada.");
        }

        if (!CanAccessDescarga(descarga))
        {
            logger.LogWarning("Descarga bloqueada por ownership. DescargaId={DescargaId}", descarga.Id);
            return ApiResponse<DescargaResponseDto>.Forbidden("No puede consultar descargas de otro usuario.");
        }

        return ApiResponse<DescargaResponseDto>.Ok(MapDescarga(descarga));
    }

    public async Task<ApiResponse<LinkDescargaResponseDto>> CreateDownloadLinkAsync(
        CrearLinkDescargaRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var pedido = await QueryPedidoForDownload()
            .FirstOrDefaultAsync(x => x.Id == request.PedidoId, cancellationToken);

        if (pedido is null)
        {
            return ApiResponse<LinkDescargaResponseDto>.NotFound("Pedido no encontrado.");
        }

        if (!CanAccessPedido(pedido))
        {
            logger.LogWarning("Descarga bloqueada por ownership. PedidoId={PedidoId}", pedido.Id);
            return ApiResponse<LinkDescargaResponseDto>.Forbidden("No puede generar descargas de pedidos de otro usuario.");
        }

        if (!PedidoEstados.PermiteDescarga(pedido.Estado, pedido.Pago?.Estado))
        {
            logger.LogWarning("Descarga bloqueada por pedido no pagado. PedidoId={PedidoId} Estado={Estado}", pedido.Id, pedido.Estado);
            return ApiResponse<LinkDescargaResponseDto>.Fail("El pedido debe estar pagado para descargar fotos.");
        }

        if ((request.FotoId is null && request.FotoPrivadaId is null)
            || (request.FotoId is not null && request.FotoPrivadaId is not null))
        {
            return ApiResponse<LinkDescargaResponseDto>.Fail("Debe solicitar una foto publica o una foto privada.");
        }

        var target = request.FotoPrivadaId is not null
            ? await ResolvePrivatePhotoAsync(pedido, request.FotoPrivadaId.Value, cancellationToken)
            : await ResolvePublicPhotoAsync(pedido, request.FotoId!.Value, cancellationToken);

        if (!target.Success || target.Data is null)
        {
            return ApiResponse<LinkDescargaResponseDto>.Fail(
                target.Message ?? "No se pudo validar la descarga.",
                target.StatusCode ?? 400,
                target.Errors.ToArray());
        }

        var signedUrl = await storageService.CreateTemporaryDownloadUrlAsync(
            pedido.Id,
            target.Data.DownloadPhotoId,
            target.Data.StorageKey,
            target.Data.NombreArchivo,
            cancellationToken);

        if (!signedUrl.Success || signedUrl.Data is null)
        {
            return signedUrl;
        }

        var now = DateTime.UtcNow;
        var descarga = new Descarga
        {
            PedidoId = pedido.Id,
            EventoId = target.Data.EventoId,
            ClienteId = pedido.ClienteId,
            FotoId = target.Data.FotoId,
            FotoPrivadaId = target.Data.FotoPrivadaId,
            StorageKey = target.Data.StorageKey,
            NombreArchivo = target.Data.NombreArchivo,
            ExpiraEnUtc = signedUrl.Data.ExpiraEnUtc,
            CreadoEnUtc = now,
            MaxDescargas = DefaultMaxDescargas,
            DescargasRealizadas = 0,
            Activa = true
        };

        dbContext.Descargas.Add(descarga);
        await dbContext.SaveChangesAsync(cancellationToken);

        await bitacoraService.RegistrarInfoAsync(
            BitacoraAcciones.DescargaGenerada,
            BitacoraEntidades.Descarga,
            descarga.Id,
            "Link de descarga generado.",
            new
            {
                DescargaId = descarga.Id,
                descarga.PedidoId,
                descarga.EventoId,
                descarga.ClienteId,
                descarga.FotoId,
                descarga.FotoPrivadaId,
                descarga.ExpiraEnUtc,
                descarga.MaxDescargas
            },
            cancellationToken);

        logger.LogInformation("Descarga creada. DescargaId={DescargaId} PedidoId={PedidoId}", descarga.Id, pedido.Id);
        await TryEnqueueDescargaLinkGeneradoAsync(descarga, pedido, cancellationToken);

        signedUrl.Data.DescargaId = descarga.Id;
        signedUrl.Data.FotoId = target.Data.FotoId;
        signedUrl.Data.FotoPrivadaId = target.Data.FotoPrivadaId;
        signedUrl.Data.MaxDescargas = descarga.MaxDescargas;
        signedUrl.Data.DescargasRealizadas = descarga.DescargasRealizadas;
        signedUrl.Data.UltimaDescargaUtc = descarga.UltimaDescargaUtc;
        return signedUrl;
    }

    public async Task<ApiResponse<RegenerarDescargaResponseDto>> RegenerarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var descargaAnterior = await dbContext.Descargas
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (descargaAnterior is null)
        {
            return ApiResponse<RegenerarDescargaResponseDto>.NotFound("Descarga no encontrada.");
        }

        var pedido = await QueryPedidoForDownload()
            .FirstOrDefaultAsync(x => x.Id == descargaAnterior.PedidoId, cancellationToken);

        if (pedido is null)
        {
            return ApiResponse<RegenerarDescargaResponseDto>.NotFound("Pedido no encontrado.");
        }

        if (!CanAccessPedido(pedido))
        {
            logger.LogWarning("Descarga bloqueada por ownership. DescargaId={DescargaId}", descargaAnterior.Id);
            return ApiResponse<RegenerarDescargaResponseDto>.Forbidden("No puede regenerar descargas de otro usuario.");
        }

        if (!PedidoEstados.PermiteDescarga(pedido.Estado, pedido.Pago?.Estado))
        {
            logger.LogWarning("Descarga bloqueada por pedido no pagado. DescargaId={DescargaId} PedidoId={PedidoId}", descargaAnterior.Id, pedido.Id);
            return ApiResponse<RegenerarDescargaResponseDto>.Fail("El pedido debe estar pagado para regenerar una descarga.");
        }

        var target = await ResolveTargetFromDescargaAsync(pedido, descargaAnterior, cancellationToken);
        if (!target.Success || target.Data is null)
        {
            return ApiResponse<RegenerarDescargaResponseDto>.Fail(
                target.Message ?? "No se pudo validar la descarga.",
                target.StatusCode ?? 400,
                target.Errors.ToArray());
        }

        var signedUrl = await storageService.CreateTemporaryDownloadUrlAsync(
            pedido.Id,
            target.Data.DownloadPhotoId,
            target.Data.StorageKey,
            target.Data.NombreArchivo,
            cancellationToken);

        if (!signedUrl.Success || signedUrl.Data is null)
        {
            return ApiResponse<RegenerarDescargaResponseDto>.Fail(
                signedUrl.Message ?? "No se pudo generar el link de descarga.",
                signedUrl.StatusCode ?? 400,
                signedUrl.Errors.ToArray());
        }

        var now = DateTime.UtcNow;
        descargaAnterior.Activa = false;
        descargaAnterior.FechaActualizacionUtc = now;

        var descargaNueva = new Descarga
        {
            PedidoId = pedido.Id,
            EventoId = target.Data.EventoId,
            ClienteId = pedido.ClienteId,
            FotoId = target.Data.FotoId,
            FotoPrivadaId = target.Data.FotoPrivadaId,
            StorageKey = target.Data.StorageKey,
            NombreArchivo = target.Data.NombreArchivo,
            ExpiraEnUtc = signedUrl.Data.ExpiraEnUtc,
            CreadoEnUtc = now,
            MaxDescargas = descargaAnterior.MaxDescargas ?? DefaultMaxDescargas,
            DescargasRealizadas = 0,
            Activa = true
        };

        dbContext.Descargas.Add(descargaNueva);
        await dbContext.SaveChangesAsync(cancellationToken);

        await bitacoraService.RegistrarInfoAsync(
            BitacoraAcciones.DescargaRegenerada,
            BitacoraEntidades.Descarga,
            descargaNueva.Id,
            "Descarga regenerada.",
            new
            {
                DescargaAnteriorId = descargaAnterior.Id,
                DescargaNuevaId = descargaNueva.Id,
                descargaNueva.PedidoId,
                descargaNueva.EventoId,
                descargaNueva.ClienteId,
                descargaNueva.FotoId,
                descargaNueva.FotoPrivadaId,
                descargaNueva.ExpiraEnUtc,
                descargaNueva.MaxDescargas
            },
            cancellationToken);

        logger.LogInformation(
            "Descarga regenerada. DescargaAnteriorId={DescargaAnteriorId} DescargaNuevaId={DescargaNuevaId}",
            descargaAnterior.Id,
            descargaNueva.Id);
        await TryEnqueueDescargaLinkGeneradoAsync(descargaNueva, pedido, cancellationToken);

        return ApiResponse<RegenerarDescargaResponseDto>.Ok(new RegenerarDescargaResponseDto
        {
            DescargaId = descargaNueva.Id,
            DescargaAnteriorId = descargaAnterior.Id,
            Url = signedUrl.Data.Url,
            ExpiraEnUtc = descargaNueva.ExpiraEnUtc,
            MaxDescargas = descargaNueva.MaxDescargas,
            DescargasRealizadas = descargaNueva.DescargasRealizadas
        }, "Descarga regenerada.");
    }

    public async Task<ApiResponse<DescargaResponseDto>> ValidarYRegistrarUsoAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var descarga = await QueryDescargas()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (descarga is null)
        {
            return ApiResponse<DescargaResponseDto>.NotFound("Descarga no encontrada.");
        }

        if (!CanAccessDescarga(descarga))
        {
            logger.LogWarning("Descarga bloqueada por ownership. DescargaId={DescargaId}", descarga.Id);
            return ApiResponse<DescargaResponseDto>.Forbidden("No puede usar descargas de otro usuario.");
        }

        if (!descarga.Activa)
        {
            return ApiResponse<DescargaResponseDto>.Fail("La descarga no esta activa.");
        }

        var now = DateTime.UtcNow;
        if (descarga.ExpiraEnUtc <= now)
        {
            logger.LogWarning("Descarga bloqueada por vencimiento. DescargaId={DescargaId}", descarga.Id);
            return ApiResponse<DescargaResponseDto>.Fail("La descarga esta vencida.");
        }

        if (descarga.MaxDescargas == 0)
        {
            logger.LogWarning("Descarga bloqueada por maximo de descargas. DescargaId={DescargaId}", descarga.Id);
            return ApiResponse<DescargaResponseDto>.Fail("La descarga no tiene usos disponibles.");
        }

        if (descarga.MaxDescargas is int maxDescargas && descarga.DescargasRealizadas >= maxDescargas)
        {
            logger.LogWarning("Descarga bloqueada por maximo de descargas. DescargaId={DescargaId}", descarga.Id);
            return ApiResponse<DescargaResponseDto>.Fail("La descarga supero la cantidad maxima de usos.");
        }

        if (string.IsNullOrWhiteSpace(descarga.StorageKey))
        {
            return ApiResponse<DescargaResponseDto>.Fail("La descarga no tiene storage key valido.");
        }

        var pedido = await QueryPedidoForDownload()
            .FirstOrDefaultAsync(x => x.Id == descarga.PedidoId, cancellationToken);
        if (pedido is null)
        {
            return ApiResponse<DescargaResponseDto>.NotFound("Pedido no encontrado.");
        }

        if (!PedidoEstados.PermiteDescarga(pedido.Estado, pedido.Pago?.Estado))
        {
            logger.LogWarning("Descarga bloqueada por pedido no pagado. DescargaId={DescargaId} PedidoId={PedidoId}", descarga.Id, pedido.Id);
            return ApiResponse<DescargaResponseDto>.Fail("El pedido debe estar pagado para descargar fotos.");
        }

        var target = await ResolveTargetFromDescargaAsync(pedido, descarga, cancellationToken);
        if (!target.Success)
        {
            return ApiResponse<DescargaResponseDto>.Fail(
                target.Message ?? "No se pudo validar la descarga.",
                target.StatusCode ?? 400,
                target.Errors.ToArray());
        }

        descarga.DescargasRealizadas++;
        descarga.UltimaDescargaUtc = now;
        descarga.FechaActualizacionUtc = now;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<DescargaResponseDto>.Ok(MapDescarga(descarga), "Uso de descarga registrado.");
    }

    private async Task<ApiResponse<DownloadTarget>> ResolvePublicPhotoAsync(
        Pedido pedido,
        Guid fotoId,
        CancellationToken cancellationToken)
    {
        var foto = await dbContext.Fotos
            .AsNoTracking()
            .Include(x => x.Evento)
            .FirstOrDefaultAsync(x => x.Id == fotoId && x.Activa, cancellationToken);

        if (foto is null || foto.Evento?.Activo != true)
        {
            return ApiResponse<DownloadTarget>.NotFound("Foto no encontrada o inactiva.");
        }

        var hasDirectPedidoItem = pedido.PedidoItems.Any(x =>
            x.TipoItem == PedidoItemTipos.FotoEvento && x.FotoId == fotoId);
        var hasLegacyPedidoFoto = pedido.PedidoFotos.Any(x => x.FotoId == fotoId);
        var hasPackage = pedido.PedidoItems.Any(x =>
            x.TipoItem == PedidoItemTipos.PaqueteEvento
            && x.PaqueteEvento?.IncluyeTodasLasFotos == true
            && x.PaqueteEvento.EventoId == foto.EventoId);

        if (!hasDirectPedidoItem && !hasLegacyPedidoFoto && !hasPackage)
        {
            return ApiResponse<DownloadTarget>.Fail("La foto no pertenece al pedido.");
        }

        return ApiResponse<DownloadTarget>.Ok(new DownloadTarget(
            foto.Id,
            foto.EventoId,
            foto.Id,
            null,
            foto.StorageKey,
            foto.NombreArchivo));
    }

    private async Task<ApiResponse<DownloadTarget>> ResolvePrivatePhotoAsync(
        Pedido pedido,
        Guid fotoPrivadaId,
        CancellationToken cancellationToken)
    {
        var foto = await dbContext.FotosPrivadas
            .AsNoTracking()
            .Include(x => x.Cliente)
            .FirstOrDefaultAsync(x => x.Id == fotoPrivadaId && x.Activa, cancellationToken);

        if (foto is null)
        {
            return ApiResponse<DownloadTarget>.NotFound("Foto privada no encontrada o inactiva.");
        }

        if (!currentUser.IsAdmin && foto.Cliente?.UsuarioId != currentUser.UserId)
        {
            return ApiResponse<DownloadTarget>.Forbidden("No puede descargar fotos privadas de otro usuario.");
        }

        if (pedido.ClienteId != foto.ClienteId)
        {
            return ApiResponse<DownloadTarget>.Fail("La foto privada no pertenece al cliente del pedido.");
        }

        var hasPedidoItem = pedido.PedidoItems.Any(x =>
            x.TipoItem == PedidoItemTipos.FotoPrivada && x.FotoPrivadaId == fotoPrivadaId);
        if (!hasPedidoItem)
        {
            return ApiResponse<DownloadTarget>.Fail("La foto privada no pertenece al pedido.");
        }

        return ApiResponse<DownloadTarget>.Ok(new DownloadTarget(
            foto.Id,
            null,
            null,
            foto.Id,
            foto.StorageKey,
            foto.NombreArchivo));
    }

    private IQueryable<Descarga> QueryDescargas()
    {
        return dbContext.Descargas
            .Include(x => x.Cliente)
            .Include(x => x.Pedido)
            .ThenInclude(x => x!.Cliente)
            .Include(x => x.Pedido)
            .ThenInclude(x => x!.Pago)
            .Include(x => x.Foto)
            .Include(x => x.FotoPrivada);
    }

    private IQueryable<Pedido> QueryPedidoForDownload()
    {
        return dbContext.Pedidos
            .Include(x => x.Cliente)
            .Include(x => x.Pago)
            .Include(x => x.PedidoItems)
            .ThenInclude(x => x.Foto)
            .Include(x => x.PedidoItems)
            .ThenInclude(x => x.PaqueteEvento)
            .Include(x => x.PedidoItems)
            .ThenInclude(x => x.FotoPrivada)
            .Include(x => x.PedidoFotos)
            .ThenInclude(x => x.Foto);
    }

    private Task<ApiResponse<DownloadTarget>> ResolveTargetFromDescargaAsync(
        Pedido pedido,
        Descarga descarga,
        CancellationToken cancellationToken)
    {
        if (descarga.FotoPrivadaId is Guid fotoPrivadaId)
        {
            return ResolvePrivatePhotoAsync(pedido, fotoPrivadaId, cancellationToken);
        }

        if (descarga.FotoId is Guid fotoId)
        {
            return ResolvePublicPhotoAsync(pedido, fotoId, cancellationToken);
        }

        return Task.FromResult(ApiResponse<DownloadTarget>.Fail("La descarga no tiene foto asociada."));
    }

    private bool CanAccessPedido(Pedido pedido)
    {
        return currentUser.IsAdmin
            || (currentUser.UserId is Guid userId && pedido.Cliente?.UsuarioId == userId);
    }

    private bool CanAccessDescarga(Descarga descarga)
    {
        return currentUser.IsAdmin
            || (currentUser.UserId is Guid userId
                && (descarga.Cliente?.UsuarioId == userId
                    || descarga.Pedido?.Cliente?.UsuarioId == userId));
    }

    private static DescargaResponseDto MapDescarga(Descarga descarga)
    {
        return new DescargaResponseDto
        {
            Id = descarga.Id,
            PedidoId = descarga.PedidoId,
            EventoId = descarga.EventoId,
            ClienteId = descarga.ClienteId,
            FotoId = descarga.FotoId,
            FotoPrivadaId = descarga.FotoPrivadaId,
            NombreArchivo = descarga.NombreArchivo,
            ExpiraEnUtc = descarga.ExpiraEnUtc,
            CreadoEnUtc = descarga.CreadoEnUtc,
            FechaActualizacionUtc = descarga.FechaActualizacionUtc,
            MaxDescargas = descarga.MaxDescargas,
            DescargasRealizadas = descarga.DescargasRealizadas,
            UltimaDescargaUtc = descarga.UltimaDescargaUtc,
            Activa = descarga.Activa
        };
    }

    private sealed record DownloadTarget(
        Guid DownloadPhotoId,
        Guid? EventoId,
        Guid? FotoId,
        Guid? FotoPrivadaId,
        string StorageKey,
        string NombreArchivo);

    private async Task TryEnqueueDescargaLinkGeneradoAsync(
        Descarga descarga,
        Pedido pedido,
        CancellationToken cancellationToken)
    {
        var cliente = pedido.Cliente;
        if (cliente is null)
        {
            cliente = await dbContext.Clientes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == pedido.ClienteId, cancellationToken);
        }

        if (cliente is null)
        {
            return;
        }

        await TryEnqueueTemplateAsync(new EnqueueTemplateNotificacionRequestDto
        {
            Codigo = NotificacionTipos.DescargaLinkGeneradoCliente,
            DestinatarioEmail = cliente.Email,
            UsuarioId = cliente.UsuarioId,
            EntidadTipo = "Descarga",
            EntidadId = descarga.Id,
            CorrelationKey = $"descarga:{descarga.Id}:link-generado:cliente",
            Reemplazos = new Dictionary<string, string?>
            {
                ["NombreCliente"] = cliente.Nombre,
                ["EmailCliente"] = cliente.Email,
                ["NombreEvento"] = descarga.Evento?.Nombre,
                ["PedidoId"] = pedido.Id.ToString(),
                ["Total"] = pedido.Total.ToString("0.##"),
                ["Estado"] = pedido.Estado,
                ["Link"] = "Disponible en tu cuenta",
                ["NombreFotografa"] = "Fotografa",
                ["Fecha"] = descarga.ExpiraEnUtc.ToString("yyyy-MM-dd")
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
                    "No se pudo encolar notificacion de descarga. Codigo={Codigo} EntidadId={EntidadId} Motivo={Motivo}",
                    request.Codigo,
                    request.EntidadId,
                    result.Message);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(
                ex,
                "No se pudo encolar notificacion de descarga. Codigo={Codigo} EntidadId={EntidadId}",
                request.Codigo,
                request.EntidadId);
        }
    }
}
