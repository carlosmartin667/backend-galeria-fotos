using Fotografia.Api.Helpers;
using Fotografia.Application.Security;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fotografia.Api.Controllers;

[ApiController]
[Authorize(Roles = SistemaRoles.AdminUsuario)]
[Route("api/[controller]")]
public sealed class FavoritosController(IFavoritoService favoritoService) : ControllerBase
{
    [HttpGet("eventos")]
    public async Task<IActionResult> GetEventos(CancellationToken cancellationToken)
    {
        var result = await favoritoService.GetEventosAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("eventos/{eventoId:guid}")]
    public async Task<IActionResult> AddEvento(Guid eventoId, CancellationToken cancellationToken)
    {
        var result = await favoritoService.AddEventoAsync(eventoId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("eventos/{eventoId:guid}")]
    public async Task<IActionResult> DeleteEvento(Guid eventoId, CancellationToken cancellationToken)
    {
        var result = await favoritoService.DeleteEventoAsync(eventoId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("fotos")]
    public async Task<IActionResult> GetFotos(CancellationToken cancellationToken)
    {
        var result = await favoritoService.GetFotosAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("fotos/{fotoId:guid}")]
    public async Task<IActionResult> AddFoto(Guid fotoId, CancellationToken cancellationToken)
    {
        var result = await favoritoService.AddFotoAsync(fotoId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("fotos/{fotoId:guid}")]
    public async Task<IActionResult> DeleteFoto(Guid fotoId, CancellationToken cancellationToken)
    {
        var result = await favoritoService.DeleteFotoAsync(fotoId, cancellationToken);
        return this.ToActionResult(result);
    }
}
