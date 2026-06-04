using Fotografia.Api.Helpers;
using Fotografia.Application.DTOs.Cupones;
using Fotografia.Application.Security;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fotografia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CuponesController(ICuponService cuponService) : ControllerBase
{
    [HttpGet("admin")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> GetAdmin(CancellationToken cancellationToken)
    {
        var result = await cuponService.GetAdminAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("admin/{id:guid}")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> GetAdminById(Guid id, CancellationToken cancellationToken)
    {
        var result = await cuponService.GetAdminByIdAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Create(CrearCuponDescuentoRequestDto request, CancellationToken cancellationToken)
    {
        var result = await cuponService.CreateAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Update(Guid id, ActualizarCuponDescuentoRequestDto request, CancellationToken cancellationToken)
    {
        var result = await cuponService.UpdateAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await cuponService.DeleteAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{id:guid}/activar")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Activar(Guid id, CancellationToken cancellationToken)
    {
        var result = await cuponService.ActivarAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{id:guid}/desactivar")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Desactivar(Guid id, CancellationToken cancellationToken)
    {
        var result = await cuponService.DesactivarAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}/usos")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> GetUsos(Guid id, CancellationToken cancellationToken)
    {
        var result = await cuponService.GetUsosAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("validar")]
    [Authorize(Roles = SistemaRoles.AdminUsuario)]
    public async Task<IActionResult> Validar(ValidarCuponRequestDto request, CancellationToken cancellationToken)
    {
        var result = await cuponService.ValidarAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }
}
