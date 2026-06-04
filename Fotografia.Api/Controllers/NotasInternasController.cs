using Fotografia.Api.Helpers;
using Fotografia.Application.DTOs.NotasInternas;
using Fotografia.Application.Security;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fotografia.Api.Controllers;

[ApiController]
[Authorize(Roles = SistemaRoles.Admin)]
[Route("api/[controller]")]
public sealed class NotasInternasController(INotaInternaService notaInternaService) : ControllerBase
{
    [HttpGet("{entidadTipo}/{entidadId:guid}")]
    public async Task<IActionResult> GetByEntidad(
        string entidadTipo,
        Guid entidadId,
        CancellationToken cancellationToken)
    {
        var result = await notaInternaService.GetByEntidadAsync(entidadTipo, entidadId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{entidadTipo}/{entidadId:guid}")]
    public async Task<IActionResult> Create(
        string entidadTipo,
        Guid entidadId,
        CrearNotaInternaRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await notaInternaService.CreateAsync(entidadTipo, entidadId, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        ActualizarNotaInternaRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await notaInternaService.UpdateAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await notaInternaService.DeleteAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }
}
