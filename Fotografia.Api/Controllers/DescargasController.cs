using Fotografia.Application.DTOs.Descargas;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fotografia.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class DescargasController(IDescargaService descargaService) : ControllerBase
{
    [HttpPost("link")]
    public async Task<IActionResult> CreateDownloadLink(CrearLinkDescargaRequestDto request, CancellationToken cancellationToken)
    {
        var result = await descargaService.CreateDownloadLinkAsync(request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
