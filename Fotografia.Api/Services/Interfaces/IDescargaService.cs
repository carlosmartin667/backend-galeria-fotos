using Fotografia.Api.DTOs.Descargas;
using Fotografia.Api.Helpers;

namespace Fotografia.Api.Services.Interfaces;

public interface IDescargaService
{
    Task<ApiResponse<LinkDescargaResponseDto>> CreateDownloadLinkAsync(CrearLinkDescargaRequestDto request, CancellationToken cancellationToken = default);
}
