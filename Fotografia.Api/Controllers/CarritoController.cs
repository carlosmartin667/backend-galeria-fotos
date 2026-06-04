using Fotografia.Api.Helpers;
using Fotografia.Application.DTOs.Carrito;
using Fotografia.Application.Security;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fotografia.Api.Controllers;

[ApiController]
[Authorize(Roles = SistemaRoles.AdminUsuario)]
[Route("api/[controller]")]
public sealed class CarritoController(ICarritoService carritoService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetActivo(CancellationToken cancellationToken)
    {
        var result = await carritoService.GetActivoAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("items/foto-evento/{fotoId:guid}")]
    public async Task<IActionResult> AddFotoEvento(Guid fotoId, CancellationToken cancellationToken)
    {
        var result = await carritoService.AddFotoEventoAsync(fotoId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("items/paquete-evento/{paqueteId:guid}")]
    public async Task<IActionResult> AddPaqueteEvento(Guid paqueteId, CancellationToken cancellationToken)
    {
        var result = await carritoService.AddPaqueteEventoAsync(paqueteId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("items/foto-privada/{fotoPrivadaId:guid}")]
    public async Task<IActionResult> AddFotoPrivada(Guid fotoPrivadaId, CancellationToken cancellationToken)
    {
        var result = await carritoService.AddFotoPrivadaAsync(fotoPrivadaId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("items/{itemId:guid}")]
    public async Task<IActionResult> DeleteItem(Guid itemId, CancellationToken cancellationToken)
    {
        var result = await carritoService.DeleteItemAsync(itemId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("vaciar")]
    public async Task<IActionResult> Vaciar(CancellationToken cancellationToken)
    {
        var result = await carritoService.VaciarAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("cupon")]
    public async Task<IActionResult> AplicarCupon(AplicarCuponCarritoRequestDto request, CancellationToken cancellationToken)
    {
        var result = await carritoService.AplicarCuponAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("cupon")]
    public async Task<IActionResult> QuitarCupon(CancellationToken cancellationToken)
    {
        var result = await carritoService.QuitarCuponAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("crear-pedido")]
    public async Task<IActionResult> CrearPedido(CancellationToken cancellationToken)
    {
        var result = await carritoService.CrearPedidoAsync(cancellationToken);
        return this.ToActionResult(result);
    }
}
