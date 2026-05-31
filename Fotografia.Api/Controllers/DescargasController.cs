using Fotografia.Api.Helpers;
using Fotografia.Application.DTOs.Descargas;
using Fotografia.Application.Security;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fotografia.Api.Controllers;

[ApiController]
[Authorize(Roles = SistemaRoles.AdminUsuario)]
[Route("api/[controller]")]
public sealed class DescargasController(IDescargaService descargaService) : ControllerBase
{
    [HttpPost("link")]
    public async Task<IActionResult> CreateDownloadLink(CrearLinkDescargaRequestDto request, CancellationToken cancellationToken)
    {
        var result = await descargaService.CreateDownloadLinkAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }
}
