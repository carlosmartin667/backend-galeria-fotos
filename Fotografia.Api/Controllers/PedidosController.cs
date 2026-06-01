using Fotografia.Api.Helpers;
using Fotografia.Application.DTOs.Common;
using Fotografia.Application.DTOs.Pedidos;
using Fotografia.Application.Security;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fotografia.Api.Controllers;

[ApiController]
[Authorize(Roles = SistemaRoles.AdminUsuario)]
[Route("api/[controller]")]
public sealed class PedidosController(IPedidoService pedidoService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await pedidoService.GetAllAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("paginado")]
    public async Task<IActionResult> GetPaginated([FromQuery] PaginationQueryDto pagination, CancellationToken cancellationToken)
    {
        var result = await pedidoService.GetPaginatedAsync(pagination, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await pedidoService.GetByIdAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CrearPedidoRequestDto request, CancellationToken cancellationToken)
    {
        var result = await pedidoService.CreateAsync(request, cancellationToken);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result)
            : this.ToActionResult(result);
    }
}
