using Fotografia.Api.Helpers;
using Fotografia.Api.Middleware;
using Fotografia.Application.DTOs.Bitacora;
using Fotografia.Application.Helpers;
using Fotografia.Application.Security;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fotografia.Api.Controllers;

[ApiController]
[Authorize(Roles = SistemaRoles.Admin)]
[Route("api/dev-tools")]
[Tags("DevTools")]
public sealed class DevToolsController(
    IHostEnvironment environment,
    ICurrentUserService currentUser,
    IBitacoraService bitacoraService) : ControllerBase
{
    private const string NotAvailableMessage = "DevTools no disponible en este ambiente.";

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        if (!IsDevToolsEnvironment())
        {
            return NotFound();
        }

        return this.ToActionResult(ApiResponse<object>.Ok(new
        {
            Module = "DevTools",
            Environment = environment.EnvironmentName,
            Utc = DateTime.UtcNow,
            CorrelationId = GetCorrelationId()
        }, "DevTools activo."));
    }

    [HttpGet("current-user")]
    public IActionResult CurrentUser()
    {
        if (!IsDevToolsEnvironment())
        {
            return NotFound();
        }

        return this.ToActionResult(ApiResponse<object>.Ok(new
        {
            currentUser.IsAuthenticated,
            currentUser.UserId,
            currentUser.Email,
            currentUser.Rol,
            currentUser.IsAdmin,
            currentUser.IsUsuario,
            currentUser.IsInvitado
        }, "Usuario actual de prueba."));
    }

    [HttpGet("correlation-id")]
    public IActionResult CorrelationId()
    {
        if (!IsDevToolsEnvironment())
        {
            return NotFound();
        }

        return this.ToActionResult(ApiResponse<object>.Ok(new
        {
            CorrelationIdMiddleware.HeaderName,
            CorrelationId = GetCorrelationId(),
            TraceId = HttpContext.TraceIdentifier
        }, "CorrelationId actual."));
    }

    [HttpGet("errors/bad-request")]
    public IActionResult BadRequestProbe()
    {
        return ToDevToolsActionResult(ApiResponse<object>.Fail("DevTools: bad request controlado."));
    }

    [HttpGet("errors/unauthorized")]
    public IActionResult UnauthorizedProbe()
    {
        return ToDevToolsActionResult(ApiResponse<object>.Unauthorized("DevTools: unauthorized controlado."));
    }

    [HttpGet("errors/forbidden")]
    public IActionResult ForbiddenProbe()
    {
        return ToDevToolsActionResult(ApiResponse<object>.Forbidden("DevTools: forbidden controlado."));
    }

    [HttpGet("errors/not-found")]
    public IActionResult NotFoundProbe()
    {
        return ToDevToolsActionResult(ApiResponse<object>.NotFound("DevTools: not found controlado."));
    }

    [HttpGet("errors/conflict")]
    public IActionResult ConflictProbe()
    {
        return ToDevToolsActionResult(ApiResponse<object>.Conflict("DevTools: conflict controlado."));
    }

    [HttpGet("errors/external-dependency")]
    public IActionResult ExternalDependencyProbe()
    {
        return ToDevToolsActionResult(ApiResponse<object>.ExternalDependency("DevTools: dependencia externa simulada."));
    }

    [HttpGet("errors/internal-controlled")]
    public IActionResult InternalControlledProbe()
    {
        return ToDevToolsActionResult(ApiResponse<object>.InternalError("DevTools: error interno controlado."));
    }

    [HttpGet("errors/throw")]
    public IActionResult ThrowProbe()
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        throw new InvalidOperationException("DevTools test exception.");
    }

    [HttpPost("audit/test-entry")]
    public async Task<IActionResult> CreateAuditTestEntry(CancellationToken cancellationToken)
    {
        if (!IsDevToolsEnvironment())
        {
            return NotFound();
        }

        var correlationId = GetCorrelationId();
        await bitacoraService.RegistrarAsync(new CrearBitacoraRequestDto
        {
            Accion = BitacoraAcciones.AccionAdminSensible,
            EntidadTipo = BitacoraEntidades.Sistema,
            Descripcion = "Entrada de bitacora generada desde DevTools.",
            Metadata = new
            {
                Source = "DevTools",
                Scenario = "AuditTestEntry",
                Environment = environment.EnvironmentName,
                CorrelationId = correlationId,
                UsesRealData = false
            },
            Severidad = BitacoraSeveridades.Info
        }, cancellationToken);

        return this.ToActionResult(ApiResponse<object>.Ok(new
        {
            Created = true,
            Action = BitacoraAcciones.AccionAdminSensible,
            CorrelationId = correlationId
        }, "Entrada de bitacora de prueba creada."));
    }

    [HttpGet("rate-limit/probe")]
    [EnableRateLimiting("SensitivePublic")]
    public IActionResult RateLimitProbe()
    {
        if (!IsDevToolsEnvironment())
        {
            return NotFound();
        }

        return this.ToActionResult(ApiResponse<object>.Ok(new
        {
            Policy = "SensitivePublic",
            Probe = "DevTools rate limit probe",
            Utc = DateTime.UtcNow
        }, "Probe de rate limit ejecutado."));
    }

    [HttpGet("payloads/null-data")]
    public IActionResult NullDataPayload()
    {
        return ToDevToolsActionResult(ApiResponse<object?>.Ok(null, "Payload de prueba: data null."));
    }

    [HttpGet("payloads/missing-fields")]
    public IActionResult MissingFieldsPayload()
    {
        return ToDevToolsActionResult(ApiResponse<object>.Ok(new
        {
            Id = "demo-missing-fields-001",
            Type = "DevToolsPayload"
        }, "Payload de prueba: campos clave faltantes."));
    }

    [HttpGet("payloads/wrong-shape")]
    public IActionResult WrongShapePayload()
    {
        return ToDevToolsActionResult(ApiResponse<object>.Ok(new[]
        {
            "esto",
            "no",
            "es",
            "el",
            "objeto",
            "esperado"
        }, "Payload de prueba: estructura inesperada."));
    }

    [HttpGet("payloads/null-items")]
    public IActionResult NullItemsPayload()
    {
        return ToDevToolsActionResult(ApiResponse<object>.Ok(new
        {
            Items = (object?)null,
            TotalItems = (int?)null,
            Page = 1,
            PageSize = 10
        }, "Payload de prueba: items null."));
    }

    [HttpGet("payloads/invalid-date")]
    public IActionResult InvalidDatePayload()
    {
        return ToDevToolsActionResult(ApiResponse<object>.Ok(new
        {
            Id = "demo-invalid-date-001",
            FechaEventoUtc = "fecha-invalida-demo",
            FechaCreacionUtc = "not-a-date",
            SafeValue = "Payload demo sin datos reales"
        }, "Payload de prueba: fecha invalida."));
    }

    [HttpGet("payloads/sensitive-metadata")]
    public IActionResult SensitiveMetadataPayload()
    {
        return ToDevToolsActionResult(ApiResponse<object>.Ok(new
        {
            Token = "fake-token-for-testing",
            Password = "fake-password",
            StorageKey = "fake/storage/key.jpg",
            MarcaAguaStorageKey = "fake/watermark/key.jpg",
            SignedUrl = "https://example.com/file.jpg?token=fake&expires=999999",
            SafeValue = "Este campo si se puede mostrar"
        }, "Payload de prueba: metadata sensible falsa."));
    }

    [HttpGet("payloads/empty-list")]
    public IActionResult EmptyListPayload()
    {
        return ToDevToolsActionResult(ApiResponse<object>.Ok(new
        {
            Items = Array.Empty<object>(),
            TotalItems = 0,
            Page = 1,
            PageSize = 10
        }, "Payload de prueba: lista vacia."));
    }

    [HttpGet("payloads/large-list")]
    public IActionResult LargeListPayload()
    {
        var items = Enumerable.Range(1, 250)
            .Select(index => new
            {
                Id = $"demo-large-list-{index:000}",
                Nombre = $"Item demo {index:000}",
                Tipo = "DevToolsPayload",
                EsDatoReal = false
            })
            .ToList();

        return ToDevToolsActionResult(ApiResponse<object>.Ok(new
        {
            Items = items,
            TotalItems = items.Count,
            Page = 1,
            PageSize = items.Count
        }, "Payload de prueba: lista grande."));
    }

    private IActionResult ToDevToolsActionResult<T>(ApiResponse<T> response)
    {
        return IsDevToolsEnvironment()
            ? this.ToActionResult(response)
            : NotFound();
    }

    private bool IsDevToolsEnvironment()
    {
        return environment.IsDevelopment() || environment.IsStaging();
    }

    private string? GetCorrelationId()
    {
        return HttpContext.Items.TryGetValue(CorrelationIdMiddleware.ItemName, out var value)
            ? value as string
            : null;
    }
}
