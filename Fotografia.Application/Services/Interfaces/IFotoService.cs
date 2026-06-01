using Fotografia.Application.DTOs.Common;
using Fotografia.Application.DTOs.Fotos;
using Fotografia.Application.DTOs.Pexels;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IFotoService
{
    Task<ApiResponse<IReadOnlyCollection<FotoResponseDto>>> GetByEventoAsync(Guid eventoId, CancellationToken cancellationToken = default);
    Task<ApiResponse<PaginatedResponseDto<FotoResponseDto>>> GetByEventoPaginatedAsync(Guid eventoId, PaginationQueryDto pagination, CancellationToken cancellationToken = default);
    Task<ApiResponse<FotoResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<FotoResponseDto>> CreateMetadataAsync(CrearFotoMetadataRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ImportarFotosPexelsResponseDto>> ImportarDesdePexelsAsync(ImportarFotosPexelsRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<FotoResponseDto>> UpdateAsync(Guid id, ActualizarFotoRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<StorageKeyResponseDto>> GenerateStorageKeyAsync(GenerarStorageKeyRequestDto request);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
