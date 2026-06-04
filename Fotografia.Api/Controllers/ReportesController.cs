using Fotografia.Api.Helpers;
using Fotografia.Application.Security;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fotografia.Api.Controllers;

[ApiController]
[Authorize(Roles = SistemaRoles.Admin)]
[Route("api/[controller]")]
public sealed class ReportesController(IReporteVentasService reporteVentasService) : ControllerBase
{
    [HttpGet("ventas/resumen")]
    public async Task<IActionResult> GetVentasResumen(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
    {
        var result = await reporteVentasService.GetVentasResumenAsync(desde, hasta, cancellationToken);
        return this.ToActionResult(result);
    }
}
