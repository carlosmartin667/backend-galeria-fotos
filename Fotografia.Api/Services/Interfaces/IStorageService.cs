using Fotografia.Api.DTOs.Descargas;
using Fotografia.Api.DTOs.Fotos;
using Fotografia.Api.Helpers;

namespace Fotografia.Api.Services.Interfaces;

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
