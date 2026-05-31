using Fotografia.Api.Helpers;
using Fotografia.Application.DTOs.Comentarios;
using Fotografia.Application.DTOs.Fotos;
using Fotografia.Application.Security;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fotografia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class FotosController(
    IFotoService fotoService,
    IComentarioFotoService comentarioFotoService) : ControllerBase
{
    [HttpGet("evento/{eventoId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByEvento(Guid eventoId, CancellationToken cancellationToken)
    {
        var result = await fotoService.GetByEventoAsync(eventoId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await fotoService.GetByIdAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("storage-key")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> GenerateStorageKey(GenerarStorageKeyRequestDto request)
    {
        var result = await fotoService.GenerateStorageKeyAsync(request);
        return this.ToActionResult(result);
    }

    [HttpPost("metadata")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> CreateMetadata(CrearFotoMetadataRequestDto request, CancellationToken cancellationToken)
    {
        var result = await fotoService.CreateMetadataAsync(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result)
            : this.ToActionResult(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Update(Guid id, ActualizarFotoRequestDto request, CancellationToken cancellationToken)
    {
        var result = await fotoService.UpdateAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await fotoService.DeleteAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{fotoId:guid}/comentarios")]
    [AllowAnonymous]
    public async Task<IActionResult> GetComentarios(Guid fotoId, CancellationToken cancellationToken)
    {
        var result = await comentarioFotoService.GetByFotoAsync(fotoId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{fotoId:guid}/comentarios")]
    [Authorize(Roles = SistemaRoles.AdminUsuario)]
    public async Task<IActionResult> CreateComentario(Guid fotoId, ComentarioRequestDto request, CancellationToken cancellationToken)
    {
        var result = await comentarioFotoService.CreateAsync(fotoId, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("comentarios/{comentarioId:guid}")]
    [Authorize(Roles = SistemaRoles.AdminUsuario)]
    public async Task<IActionResult> UpdateComentario(Guid comentarioId, ComentarioRequestDto request, CancellationToken cancellationToken)
    {
        var result = await comentarioFotoService.UpdateAsync(comentarioId, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("comentarios/{comentarioId:guid}")]
    [Authorize(Roles = SistemaRoles.AdminUsuario)]
    public async Task<IActionResult> DeleteComentario(Guid comentarioId, CancellationToken cancellationToken)
    {
        var result = await comentarioFotoService.DeleteAsync(comentarioId, cancellationToken);
        return this.ToActionResult(result);
    }
}
