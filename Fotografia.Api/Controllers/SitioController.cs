using Fotografia.Api.Helpers;
using Fotografia.Application.DTOs.Sitio;
using Fotografia.Application.Security;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fotografia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SitioController(IPerfilFotografaService perfilFotografaService) : ControllerBase
{
    [HttpGet("perfil-fotografa")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPerfilFotografa(CancellationToken cancellationToken)
    {
        var result = await perfilFotografaService.GetPublicoAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("perfil-fotografa/admin")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> GetPerfilFotografaAdmin(CancellationToken cancellationToken)
    {
        var result = await perfilFotografaService.GetAdminAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("perfil-fotografa")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> UpdatePerfilFotografa(ActualizarPerfilFotografaRequestDto request, CancellationToken cancellationToken)
    {
        var result = await perfilFotografaService.UpdateAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }
}
