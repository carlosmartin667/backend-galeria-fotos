using Fotografia.Application.DTOs.Descargas;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IDescargaService
{
    Task<ApiResponse<IReadOnlyCollection<DescargaResponseDto>>> GetMineAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyCollection<DescargaResponseDto>>> GetAdminAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<DescargaResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<LinkDescargaResponseDto>> CreateDownloadLinkAsync(CrearLinkDescargaRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<RegenerarDescargaResponseDto>> RegenerarAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<DescargaResponseDto>> ValidarYRegistrarUsoAsync(Guid id, CancellationToken cancellationToken = default);
}
