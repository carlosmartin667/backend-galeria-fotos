using Fotografia.Api.Helpers;
using Fotografia.Application.Security;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fotografia.Api.Controllers;

[ApiController]
[Authorize(Roles = SistemaRoles.Admin)]
[Route("api/[controller]")]
public sealed class CarritosController(ICarritoAbandonadoService carritoAbandonadoService) : ControllerBase
{
    [HttpGet("abandonados")]
    public async Task<IActionResult> GetAbandonados(CancellationToken cancellationToken)
    {
        var result = await carritoAbandonadoService.GetAdminAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("abandonados/resumen")]
    public async Task<IActionResult> GetResumen(CancellationToken cancellationToken)
    {
        var result = await carritoAbandonadoService.GetResumenAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("abandonados/detectar")]
    public async Task<IActionResult> Detectar(CancellationToken cancellationToken)
    {
        var result = await carritoAbandonadoService.DetectarAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("abandonados/{id:guid}/notificar")]
    public async Task<IActionResult> Notificar(Guid id, CancellationToken cancellationToken)
    {
        var result = await carritoAbandonadoService.NotificarAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }
}
