using FluentAssertions;
using Fotografia.Application.Helpers;

namespace Fotografia.Tests.Unit;

public sealed class ApiResponseTests
{
    [Fact]
    public void Conflict_sets_409_status_code()
    {
        var response = ApiResponse<object>.Conflict("Duplicado.");

        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(409);
    }

    [Fact]
    public void Unauthorized_sets_401_status_code()
    {
        var response = ApiResponse<object>.Unauthorized("Debe iniciar sesion.");

        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(401);
    }

    [Theory]
    [InlineData(502)]
    [InlineData(503)]
    public void ExternalDependency_uses_requested_status_code(int statusCode)
    {
        var response = ApiResponse<object>.ExternalDependency("Proveedor externo no disponible.", statusCode);

        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(statusCode);
    }
}
