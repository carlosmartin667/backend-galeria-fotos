using Fotografia.Api.Helpers;
using Fotografia.Application.DTOs.Presupuestos;
using Fotografia.Application.Security;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fotografia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PresupuestosController(ISolicitudPresupuestoService solicitudPresupuestoService) : ControllerBase
{
    [HttpPost("solicitudes")]
    [AllowAnonymous]
    [EnableRateLimiting("SensitivePublic")]
    public async Task<IActionResult> CreateSolicitud(
        CrearSolicitudPresupuestoRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await solicitudPresupuestoService.CreateAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("solicitudes")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> GetSolicitudes([FromQuery] bool? activa, CancellationToken cancellationToken)
    {
        var result = await solicitudPresupuestoService.GetAllAsync(activa, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("solicitudes/{id:guid}")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> GetSolicitud(Guid id, CancellationToken cancellationToken)
    {
        var result = await solicitudPresupuestoService.GetByIdAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("solicitudes/{id:guid}")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> UpdateSolicitud(
        Guid id,
        ActualizarSolicitudPresupuestoRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await solicitudPresupuestoService.UpdateAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("solicitudes/{id:guid}/estado")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> ChangeEstado(
        Guid id,
        CambiarEstadoSolicitudPresupuestoRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await solicitudPresupuestoService.ChangeEstadoAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("solicitudes/{id:guid}")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> DeleteSolicitud(Guid id, CancellationToken cancellationToken)
    {
        var result = await solicitudPresupuestoService.DeleteAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }
}
