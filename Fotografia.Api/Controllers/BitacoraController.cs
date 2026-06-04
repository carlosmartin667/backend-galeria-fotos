using Fotografia.Api.Helpers;
using Fotografia.Application.DTOs.Bitacora;
using Fotografia.Application.Security;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fotografia.Api.Controllers;

[ApiController]
[Authorize(Roles = SistemaRoles.Admin)]
[Route("api/[controller]")]
public sealed class BitacoraController(IBitacoraService bitacoraService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAdmin([FromQuery] BitacoraQueryDto query, CancellationToken cancellationToken)
    {
        var result = await bitacoraService.GetAdminAsync(query, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await bitacoraService.GetByIdAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("resumen")]
    public async Task<IActionResult> GetResumen(CancellationToken cancellationToken)
    {
        var result = await bitacoraService.GetResumenAsync(cancellationToken);
        return this.ToActionResult(result);
    }
}
