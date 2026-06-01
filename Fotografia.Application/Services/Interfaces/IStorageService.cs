using Fotografia.Application.DTOs.Descargas;
using Fotografia.Application.DTOs.Fotos;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IStorageService
{
    ApiResponse<StorageKeyResponseDto> GenerateStorageKey(Guid eventoId, string fileName);
    Task<ApiResponse<LinkDescargaResponseDto>> CreateTemporaryDownloadUrlAsync(
        Guid pedidoId,
        Guid fotoId,
        string storageKey,
        string fileName,
        CancellationToken cancellationToken = default);
}
