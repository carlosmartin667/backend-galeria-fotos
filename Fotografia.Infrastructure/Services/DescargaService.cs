using Fotografia.Infrastructure.Data;
using Fotografia.Application.DTOs.Descargas;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Services;

public sealed class DescargaService(
    AppDbContext dbContext,
    IStorageService storageService,
    ICurrentUserService currentUser) : IDescargaService
{
    public async Task<ApiResponse<LinkDescargaResponseDto>> CreateDownloadLinkAsync(
        CrearLinkDescargaRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var pedido = await dbContext.Pedidos
            .Include(x => x.Cliente)
            .Include(x => x.Pago)
            .Include(x => x.PedidoItems)
            .ThenInclude(x => x.Foto)
            .Include(x => x.PedidoItems)
            .ThenInclude(x => x.PaqueteEvento)
            .Include(x => x.PedidoItems)
            .ThenInclude(x => x.FotoPrivada)
            .Include(x => x.PedidoFotos)
            .ThenInclude(x => x.Foto)
            .FirstOrDefaultAsync(x => x.Id == request.PedidoId, cancellationToken);

        if (pedido is null)
        {
            return ApiResponse<LinkDescargaResponseDto>.NotFound("Pedido no encontrado.");
        }

        if (!currentUser.IsAdmin && pedido.Cliente?.UsuarioId != currentUser.UserId)
        {
            return ApiResponse<LinkDescargaResponseDto>.Forbidden("No puede generar descargas de pedidos de otro usuario.");
        }

        if (!string.Equals(pedido.Estado, "Pagado", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(pedido.Pago?.Estado, "approved", StringComparison.OrdinalIgnoreCase))
        {
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

        var descarga = new Descarga
        {
            PedidoId = pedido.Id,
            EventoId = target.Data.EventoId,
            ClienteId = pedido.ClienteId,
            FotoId = target.Data.FotoId,
            FotoPrivadaId = target.Data.FotoPrivadaId,
            StorageKey = target.Data.StorageKey,
            NombreArchivo = target.Data.NombreArchivo,
            ExpiraEnUtc = signedUrl.Data.ExpiraEnUtc
        };

        dbContext.Descargas.Add(descarga);
        await dbContext.SaveChangesAsync(cancellationToken);

        signedUrl.Data.DescargaId = descarga.Id;
        signedUrl.Data.FotoId = target.Data.FotoId;
        signedUrl.Data.FotoPrivadaId = target.Data.FotoPrivadaId;
        return signedUrl;
    }

    private async Task<ApiResponse<DownloadTarget>> ResolvePublicPhotoAsync(
        Pedido pedido,
        Guid fotoId,
        CancellationToken cancellationToken)
    {
        var foto = await dbContext.Fotos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == fotoId && x.Activa, cancellationToken);

        if (foto is null)
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

    private sealed record DownloadTarget(
        Guid DownloadPhotoId,
        Guid? EventoId,
        Guid? FotoId,
        Guid? FotoPrivadaId,
        string StorageKey,
        string NombreArchivo);
}
