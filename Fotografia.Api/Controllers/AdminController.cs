using Fotografia.Api.Helpers;
using Fotografia.Application.DTOs.Admin;
using Fotografia.Application.DTOs.Pexels;
using Fotografia.Application.Security;
using Fotografia.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fotografia.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AdminController(
    IAdminPerfilService adminPerfilService,
    IAdminDashboardService dashboardService,
    IFotoService fotoService) : ControllerBase
{
    [HttpGet("dashboard")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var result = await dashboardService.GetDashboardAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("perfil-publico")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPerfilPublico(CancellationToken cancellationToken)
    {
        var result = await adminPerfilService.GetPerfilPublicoAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("mi-perfil")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> GetMiPerfil(CancellationToken cancellationToken)
    {
        var result = await adminPerfilService.GetMiPerfilAsync(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("mi-perfil")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> UpdateMiPerfil(ActualizarAdminPerfilRequestDto request, CancellationToken cancellationToken)
    {
        var result = await adminPerfilService.UpdateMiPerfilAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("demo/pexels/importar-fotos")]
    [Authorize(Roles = SistemaRoles.Admin)]
    public async Task<IActionResult> ImportarFotosDesdePexels(
        ImportarFotosPexelsRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await fotoService.ImportarDesdePexelsAsync(request, cancellationToken);
        return this.ToActionResult(result);
    }
}
