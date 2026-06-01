using Fotografia.Api.Helpers;
using Fotografia.Application.DTOs.Clientes;
using Fotografia.Application.Security;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fotografia.Api.Controllers;

[ApiController]
[Authorize(Roles = SistemaRoles.AdminUsuario)]
[Route("api/[controller]")]
public sealed class ClientesController(IClienteService clienteService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await clienteService.GetAllAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await clienteService.GetByIdAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Create(CrearClienteRequestDto request, CancellationToken cancellationToken)
    {
        var result = await clienteService.CreateAsync(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result)
            : this.ToActionResult(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ActualizarClienteRequestDto request, CancellationToken cancellationToken)
    {
        var result = await clienteService.UpdateAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await clienteService.DeleteAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }
}
