using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Fotografia.Tests.Integration;

public sealed class BitacoraApiTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false
    });

    [Fact]
    public async Task Bitacora_requires_admin_role()
    {
        var anonymousResponse = await _client.GetAsync("/api/Bitacora");
        anonymousResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var userToken = await LoginAsync("usuario.tests@example.com", "Usuario123456");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        var userResponse = await _client.GetAsync("/api/Bitacora");
        userResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Bitacora_admin_can_list_login_events()
    {
        var adminToken = await LoginAsync("admin.tests@example.com", "Admin123456");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _client.GetAsync("/api/Bitacora?accion=LoginExitoso");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        document.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        document.RootElement.GetProperty("data").GetProperty("totalItems").GetInt32().Should().BeGreaterThan(0);
    }

    private async Task<string> LoginAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/login", new { email, password });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty("data").GetProperty("token").GetString()
            ?? throw new InvalidOperationException("Login response did not include token.");
    }
}
