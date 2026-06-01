using Fotografia.Api.Helpers;
using Fotografia.Application.DTOs.SesionesPrivadas;
using Fotografia.Application.Security;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fotografia.Api.Controllers;

[ApiController]
[Authorize(Roles = SistemaRoles.AdminUsuario)]
[Route("api/[controller]")]
public sealed class SesionesPrivadasController(ISesionPrivadaService sesionPrivadaService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await sesionPrivadaService.GetAllAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sesionPrivadaService.GetByIdAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Create(CrearSesionPrivadaRequestDto request, CancellationToken cancellationToken)
    {
        var result = await sesionPrivadaService.CreateAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Update(Guid id, ActualizarSesionPrivadaRequestDto request, CancellationToken cancellationToken)
    {
        var result = await sesionPrivadaService.UpdateAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await sesionPrivadaService.DeleteAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}/fotos")]
    public async Task<IActionResult> GetFotos(Guid id, CancellationToken cancellationToken)
    {
        var result = await sesionPrivadaService.GetFotosAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{id:guid}/fotos/storage-key")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> GenerateStorageKey(Guid id, GenerarStorageKeyFotoPrivadaRequestDto request, CancellationToken cancellationToken)
    {
        var result = await sesionPrivadaService.GenerateStorageKeyAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{id:guid}/fotos/metadata")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> CreateFotoMetadata(Guid id, CrearFotoPrivadaMetadataRequestDto request, CancellationToken cancellationToken)
    {
        var result = await sesionPrivadaService.CreateFotoMetadataAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("fotos/{fotoPrivadaId:guid}")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> UpdateFoto(Guid fotoPrivadaId, ActualizarFotoPrivadaRequestDto request, CancellationToken cancellationToken)
    {
        var result = await sesionPrivadaService.UpdateFotoAsync(fotoPrivadaId, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("fotos/{fotoPrivadaId:guid}")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> DeleteFoto(Guid fotoPrivadaId, CancellationToken cancellationToken)
    {
        var result = await sesionPrivadaService.DeleteFotoAsync(fotoPrivadaId, cancellationToken);
        return this.ToActionResult(result);
    }
}
