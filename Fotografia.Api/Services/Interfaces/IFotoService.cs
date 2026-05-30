using Fotografia.Api.DTOs.Fotos;
using Fotografia.Api.Helpers;

namespace Fotografia.Api.Services.Interfaces;

public interface IFotoService
{
    Task<ApiResponse<IReadOnlyCollection<FotoResponseDto>>> GetByEventoAsync(Guid eventoId, CancellationToken cancellationToken = default);
    Task<ApiResponse<FotoResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<FotoResponseDto>> CreateMetadataAsync(CrearFotoMetadataRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<FotoResponseDto>> UpdateAsync(Guid id, ActualizarFotoRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<StorageKeyResponseDto>> GenerateStorageKeyAsync(GenerarStorageKeyRequestDto request);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
