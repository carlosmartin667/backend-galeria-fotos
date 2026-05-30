using Fotografia.Application.DTOs.Descargas;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IDescargaService
{
    Task<ApiResponse<LinkDescargaResponseDto>> CreateDownloadLinkAsync(CrearLinkDescargaRequestDto request, CancellationToken cancellationToken = default);
}
