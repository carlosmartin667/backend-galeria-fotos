using Fotografia.Application.DTOs.Fotos;
using Fotografia.Application.DTOs.SesionesPrivadas;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface ISesionPrivadaService
{
    Task<ApiResponse<IReadOnlyCollection<SesionPrivadaResponseDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<SesionPrivadaResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<SesionPrivadaResponseDto>> CreateAsync(CrearSesionPrivadaRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<SesionPrivadaResponseDto>> UpdateAsync(Guid id, ActualizarSesionPrivadaRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<SesionPrivadaResponseDto>> CambiarEstadoAsync(Guid id, CambiarEstadoSesionPrivadaRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyCollection<FotoPrivadaResponseDto>>> GetFotosAsync(Guid sesionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<StorageKeyResponseDto>> GenerateStorageKeyAsync(Guid sesionId, GenerarStorageKeyFotoPrivadaRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<FotoPrivadaResponseDto>> CreateFotoMetadataAsync(Guid sesionId, CrearFotoPrivadaMetadataRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<FotoPrivadaResponseDto>> UpdateFotoAsync(Guid fotoPrivadaId, ActualizarFotoPrivadaRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteFotoAsync(Guid fotoPrivadaId, CancellationToken cancellationToken = default);
}
