using Fotografia.Api.Helpers;
using Fotografia.Application.DTOs.Testimonios;
using Fotografia.Application.Security;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fotografia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TestimoniosController(ITestimonioService testimonioService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublic(CancellationToken cancellationToken)
    {
        var result = await testimonioService.GetPublicAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("destacados")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDestacados(CancellationToken cancellationToken)
    {
        var result = await testimonioService.GetDestacadosAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("SensitivePublic")]
    public async Task<IActionResult> Create(CrearTestimonioRequestDto request, CancellationToken cancellationToken)
    {
        var result = await testimonioService.CreateAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("admin")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> GetAdmin(CancellationToken cancellationToken)
    {
        var result = await testimonioService.GetAdminAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("admin/{id:guid}")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> GetAdminById(Guid id, CancellationToken cancellationToken)
    {
        var result = await testimonioService.GetAdminByIdAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Update(Guid id, ActualizarTestimonioAdminRequestDto request, CancellationToken cancellationToken)
    {
        var result = await testimonioService.UpdateAdminAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{id:guid}/publicar")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Publicar(Guid id, CancellationToken cancellationToken)
    {
        var result = await testimonioService.PublicarAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{id:guid}/ocultar")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Ocultar(Guid id, CancellationToken cancellationToken)
    {
        var result = await testimonioService.OcultarAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await testimonioService.DeleteAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }
}
