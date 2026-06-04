using Fotografia.Api.Helpers;
using Fotografia.Application.DTOs.Notificaciones;
using Fotografia.Application.Security;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fotografia.Api.Controllers;

[ApiController]
[Authorize(Roles = SistemaRoles.AdminUsuario)]
[Route("api/[controller]")]
public sealed class NotificacionesController(
    INotificacionService notificacionService,
    IPlantillaNotificacionService plantillaNotificacionService) : ControllerBase
{
    [HttpGet("mis-notificaciones")]
    public async Task<IActionResult> GetMisNotificaciones(CancellationToken cancellationToken)
    {
        var result = await notificacionService.GetMisNotificacionesAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPatch("{id:guid}/leer")]
    public async Task<IActionResult> MarcarLeida(Guid id, CancellationToken cancellationToken)
    {
        var result = await notificacionService.MarcarLeidaAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPatch("marcar-todas-leidas")]
    public async Task<IActionResult> MarcarTodasLeidas(CancellationToken cancellationToken)
    {
        var result = await notificacionService.MarcarTodasLeidasAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("admin")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> GetAdmin([FromQuery] NotificacionesAdminQueryDto query, CancellationToken cancellationToken)
    {
        var result = await notificacionService.GetAdminAsync(query, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("admin/{id:guid}")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> GetAdminById(Guid id, CancellationToken cancellationToken)
    {
        var result = await notificacionService.GetAdminByIdAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("admin/{id:guid}/reenviar")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Reenviar(Guid id, CancellationToken cancellationToken)
    {
        var result = await notificacionService.ReenviarAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPatch("admin/{id:guid}/cancelar")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> Cancelar(Guid id, CancellationToken cancellationToken)
    {
        var result = await notificacionService.CancelarAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("plantillas")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> GetPlantillas(CancellationToken cancellationToken)
    {
        var result = await plantillaNotificacionService.GetAllAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("plantillas")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> CreatePlantilla(
        CrearPlantillaNotificacionRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await plantillaNotificacionService.CreateAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("plantillas/{id:guid}")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> UpdatePlantilla(
        Guid id,
        ActualizarPlantillaNotificacionRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await plantillaNotificacionService.UpdateAsync(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPatch("plantillas/{id:guid}/activar")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> ActivarPlantilla(Guid id, CancellationToken cancellationToken)
    {
        var result = await plantillaNotificacionService.ActivarAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPatch("plantillas/{id:guid}/desactivar")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> DesactivarPlantilla(Guid id, CancellationToken cancellationToken)
    {
        var result = await plantillaNotificacionService.DesactivarAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }
}
