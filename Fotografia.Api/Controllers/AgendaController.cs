using Fotografia.Api.Helpers;
using Fotografia.Application.DTOs.Agenda;
using Fotografia.Application.Security;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fotografia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AgendaController(IAgendaService agendaService) : ControllerBase
{
    [HttpGet("disponibilidad")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDisponibilidad(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
    {
        var result = await agendaService.GetDisponibilidadAsync(desde, hasta, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> GetAll([FromQuery] AgendaQueryDto query, CancellationToken cancellationToken)
    {
        var result = await agendaService.GetAllAsync(query, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await agendaService.GetByIdAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Create(CrearAgendaItemRequestDto request, CancellationToken cancellationToken)
    {
        var result = await agendaService.CreateAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Update(
        Guid id,
        ActualizarAgendaItemRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await agendaService.UpdateAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await agendaService.DeleteAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }
}
