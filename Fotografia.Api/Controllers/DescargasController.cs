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
    [HttpGet("mis-descargas")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var result = await descargaService.GetMineAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("admin")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> GetAdmin(CancellationToken cancellationToken)
    {
        var result = await descargaService.GetAdminAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await descargaService.GetByIdAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("link")]
    public async Task<IActionResult> CreateDownloadLink(CrearLinkDescargaRequestDto request, CancellationToken cancellationToken)
    {
        var result = await descargaService.CreateDownloadLinkAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{id:guid}/regenerar")]
    public async Task<IActionResult> Regenerar(Guid id, CancellationToken cancellationToken)
    {
        var result = await descargaService.RegenerarAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }
}
