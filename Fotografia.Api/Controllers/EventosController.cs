using Fotografia.Api.Helpers;
using Fotografia.Application.DTOs.Comentarios;
using Fotografia.Application.DTOs.Common;
using Fotografia.Application.DTOs.Eventos;
using Fotografia.Application.Security;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fotografia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class EventosController(
    IEventoService eventoService,
    IComentarioEventoService comentarioEventoService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await eventoService.GetAllAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("paginado")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPaginated([FromQuery] PaginationQueryDto pagination, CancellationToken cancellationToken)
    {
        var result = await eventoService.GetPaginatedAsync(pagination, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await eventoService.GetByIdAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Create(CrearEventoRequestDto request, CancellationToken cancellationToken)
    {
        var result = await eventoService.CreateAsync(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result)
            : this.ToActionResult(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Update(Guid id, ActualizarEventoRequestDto request, CancellationToken cancellationToken)
    {
        var result = await eventoService.UpdateAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await eventoService.DeleteAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{eventoId:guid}/comentarios")]
    [AllowAnonymous]
    public async Task<IActionResult> GetComentarios(Guid eventoId, CancellationToken cancellationToken)
    {
        var result = await comentarioEventoService.GetByEventoAsync(eventoId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{eventoId:guid}/comentarios")]
    [Authorize(Roles = SistemaRoles.AdminUsuario)]
    public async Task<IActionResult> CreateComentario(Guid eventoId, ComentarioRequestDto request, CancellationToken cancellationToken)
    {
        var result = await comentarioEventoService.CreateAsync(eventoId, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("comentarios/{comentarioId:guid}")]
    [Authorize(Roles = SistemaRoles.AdminUsuario)]
    public async Task<IActionResult> UpdateComentario(Guid comentarioId, ComentarioRequestDto request, CancellationToken cancellationToken)
    {
        var result = await comentarioEventoService.UpdateAsync(comentarioId, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("comentarios/{comentarioId:guid}")]
    [Authorize(Roles = SistemaRoles.AdminUsuario)]
    public async Task<IActionResult> DeleteComentario(Guid comentarioId, CancellationToken cancellationToken)
    {
        var result = await comentarioEventoService.DeleteAsync(comentarioId, cancellationToken);
        return this.ToActionResult(result);
    }
}
