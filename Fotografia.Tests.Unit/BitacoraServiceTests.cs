using FluentAssertions;
using Fotografia.Application.DTOs.Bitacora;
using Fotografia.Application.Security;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Fotografia.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fotografia.Tests.Unit;

public sealed class BitacoraServiceTests
{
    [Fact]
    public async Task RegistrarAsync_persists_sanitized_audit_entry_with_request_context()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var currentUser = CreateCurrentUser(userId, isAdmin: true);
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        httpContext.Request.Method = HttpMethods.Post;
        httpContext.Request.Path = "/api/Auth/login";
        httpContext.Request.Headers.UserAgent = "unit-test";
        httpContext.Items["CorrelationId"] = "corr-test";
        var service = new BitacoraService(
            dbContext,
            currentUser,
            new HttpContextAccessor { HttpContext = httpContext },
            Substitute.For<ILogger<BitacoraService>>());

        await service.RegistrarAsync(new CrearBitacoraRequestDto
        {
            Accion = BitacoraAcciones.LoginExitoso,
            EntidadTipo = BitacoraEntidades.Usuario,
            EntidadId = userId,
            Descripcion = "Login exitoso.",
            Metadata = new
            {
                Token = "Bearer secret-token",
                StorageKey = "original/foto.jpg",
                Safe = "visible"
            },
            Severidad = BitacoraSeveridades.Info
        });

        var entry = await dbContext.Bitacora.SingleAsync();
        entry.UsuarioId.Should().Be(userId);
        entry.UsuarioEmail.Should().Be("admin@example.com");
        entry.Rol.Should().Be(SistemaRoles.Admin);
        entry.CorrelationId.Should().Be("corr-test");
        entry.RequestPath.Should().Be("/api/Auth/login");
        entry.MetadataJson.Should().Contain("visible");
        entry.MetadataJson.Should().Contain("[REDACTED]");
        entry.MetadataJson.Should().NotContain("secret-token");
        entry.MetadataJson.Should().NotContain("original/foto.jpg");
    }

    [Fact]
    public async Task GetAdminAsync_requires_admin_role()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var service = new BitacoraService(
            dbContext,
            CreateCurrentUser(Guid.NewGuid(), isAdmin: false),
            new HttpContextAccessor(),
            Substitute.For<ILogger<BitacoraService>>());

        var result = await service.GetAdminAsync(new BitacoraQueryDto());

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    private static ICurrentUserService CreateCurrentUser(Guid userId, bool isAdmin)
    {
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.IsAuthenticated.Returns(true);
        currentUser.UserId.Returns(userId);
        currentUser.Email.Returns(isAdmin ? "admin@example.com" : "user@example.com");
        currentUser.Rol.Returns(isAdmin ? SistemaRoles.Admin : SistemaRoles.Usuario);
        currentUser.IsAdmin.Returns(isAdmin);
        currentUser.IsUsuario.Returns(!isAdmin);
        return currentUser;
    }
}
