using Fotografia.Api.Helpers;
using Fotografia.Application.DTOs.Servicios;
using Fotografia.Application.Security;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fotografia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ServiciosController(IServicioFotografiaService servicioFotografiaService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublic(CancellationToken cancellationToken)
    {
        var result = await servicioFotografiaService.GetPublicAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicById(Guid id, CancellationToken cancellationToken)
    {
        var result = await servicioFotografiaService.GetPublicByIdAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("admin")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> GetAdmin(CancellationToken cancellationToken)
    {
        var result = await servicioFotografiaService.GetAdminAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Create(CrearServicioFotografiaRequestDto request, CancellationToken cancellationToken)
    {
        var result = await servicioFotografiaService.CreateAsync(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetPublicById), new { id = result.Data!.Id }, result)
            : this.ToActionResult(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Update(Guid id, ActualizarServicioFotografiaRequestDto request, CancellationToken cancellationToken)
    {
        var result = await servicioFotografiaService.UpdateAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await servicioFotografiaService.DeleteAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }
}
