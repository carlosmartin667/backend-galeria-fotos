using Fotografia.Api.Helpers;
using Fotografia.Application.DTOs.Promociones;
using Fotografia.Application.Security;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fotografia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PromocionesController(IPromocionService promocionService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublic(CancellationToken cancellationToken)
    {
        var result = await promocionService.GetPublicAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicById(Guid id, CancellationToken cancellationToken)
    {
        var result = await promocionService.GetPublicByIdAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("admin")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> GetAdmin(CancellationToken cancellationToken)
    {
        var result = await promocionService.GetAdminAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Create(CrearPromocionRequestDto request, CancellationToken cancellationToken)
    {
        var result = await promocionService.CreateAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Update(Guid id, ActualizarPromocionRequestDto request, CancellationToken cancellationToken)
    {
        var result = await promocionService.UpdateAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await promocionService.DeleteAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{id:guid}/activar")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Activar(Guid id, CancellationToken cancellationToken)
    {
        var result = await promocionService.ActivarAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{id:guid}/desactivar")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Desactivar(Guid id, CancellationToken cancellationToken)
    {
        var result = await promocionService.DesactivarAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }
}
