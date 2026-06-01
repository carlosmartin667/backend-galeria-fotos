using Fotografia.Infrastructure.Data;
using Fotografia.Application.DTOs.Descargas;
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

        var pedidoFoto = pedido.PedidoFotos.FirstOrDefault(x => x.FotoId == request.FotoId);
        if (pedidoFoto?.Foto is null)
        {
            return ApiResponse<LinkDescargaResponseDto>.Fail("La foto no pertenece al pedido.");
        }

        var signedUrl = await storageService.CreateTemporaryDownloadUrlAsync(
            pedido.Id,
            pedidoFoto.FotoId,
            pedidoFoto.Foto.StorageKey,
            pedidoFoto.Foto.NombreArchivo,
            cancellationToken);

        if (!signedUrl.Success || signedUrl.Data is null)
        {
            return signedUrl;
        }

        var descarga = new Descarga
        {
            PedidoId = pedido.Id,
            EventoId = pedido.EventoId,
            ClienteId = pedido.ClienteId,
            FotoId = pedidoFoto.FotoId,
            StorageKey = pedidoFoto.Foto.StorageKey,
            NombreArchivo = pedidoFoto.Foto.NombreArchivo,
            ExpiraEnUtc = signedUrl.Data.ExpiraEnUtc
        };

        dbContext.Descargas.Add(descarga);
        await dbContext.SaveChangesAsync(cancellationToken);

        signedUrl.Data.DescargaId = descarga.Id;
        return signedUrl;
    }
}
