using Fotografia.Application.DTOs.Fotos;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fotografia.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class FotosController(IFotoService fotoService) : ControllerBase
{
    [HttpGet("evento/{eventoId:guid}")]
    public async Task<IActionResult> GetByEvento(Guid eventoId, CancellationToken cancellationToken)
    {
        var result = await fotoService.GetByEventoAsync(eventoId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await fotoService.GetByIdAsync(id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("storage-key")]
    public async Task<IActionResult> GenerateStorageKey(GenerarStorageKeyRequestDto request)
    {
        var result = await fotoService.GenerateStorageKeyAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("metadata")]
    public async Task<IActionResult> CreateMetadata(CrearFotoMetadataRequestDto request, CancellationToken cancellationToken)
    {
        var result = await fotoService.CreateMetadataAsync(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ActualizarFotoRequestDto request, CancellationToken cancellationToken)
    {
        var result = await fotoService.UpdateAsync(id, request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await fotoService.DeleteAsync(id, cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
