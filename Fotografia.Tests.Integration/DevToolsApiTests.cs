using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Fotografia.Tests.Integration;

public sealed class DevToolsApiTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false
    });

    [Fact]
    public async Task DevTools_without_token_returns_401()
    {
        var response = await _client.GetAsync("/api/dev-tools/ping");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DevTools_user_without_admin_role_returns_403()
    {
        await AuthorizeAsUserAsync();

        var response = await _client.GetAsync("/api/dev-tools/payloads/null-data");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DevTools_admin_in_development_can_access_ping()
    {
        await AuthorizeAsAdminAsync();

        var response = await _client.GetAsync("/api/dev-tools/ping");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.TryGetValues("X-Correlation-ID", out var values).Should().BeTrue();
        values.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DevTools_in_production_returns_404_before_auth()
    {
        var previousSigningKey = Environment.GetEnvironmentVariable("Jwt__SigningKey");
        Environment.SetEnvironmentVariable("Jwt__SigningKey", "tests-production-signing-key-with-more-than-32-chars");

        try
        {
            using var productionFactory = new CustomWebApplicationFactory("Production");
            var productionClient = productionFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var response = await productionClient.GetAsync("/api/dev-tools/ping");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        finally
        {
            Environment.SetEnvironmentVariable("Jwt__SigningKey", previousSigningKey);
        }
    }

    [Fact]
    public async Task DevTools_throw_endpoint_returns_safe_500()
    {
        await AuthorizeAsAdminAsync();

        var response = await _client.GetAsync("/api/dev-tools/errors/throw");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Ocurrio un error inesperado");
        body.Should().NotContain("DevTools test exception");
        body.Should().NotContain("System.InvalidOperationException");
        body.Should().NotContain(" at ");
    }

    [Fact]
    public async Task DevTools_audit_test_entry_creates_bitacora_entry()
    {
        await AuthorizeAsAdminAsync();

        var createResponse = await _client.PostAsync("/api/dev-tools/audit/test-entry", content: null);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var bitacoraResponse = await _client.GetAsync("/api/Bitacora?accion=AccionAdminSensible");

        bitacoraResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(await bitacoraResponse.Content.ReadAsStringAsync());
        document.RootElement.GetProperty("data").GetProperty("totalItems").GetInt32().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task DevTools_payloads_have_expected_test_shapes()
    {
        await AuthorizeAsAdminAsync();

        using var nullData = await GetJsonAsync("/api/dev-tools/payloads/null-data");
        nullData.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        nullData.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);

        using var wrongShape = await GetJsonAsync("/api/dev-tools/payloads/wrong-shape");
        wrongShape.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
        wrongShape.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);

        using var nullItems = await GetJsonAsync("/api/dev-tools/payloads/null-items");
        var nullItemsData = nullItems.RootElement.GetProperty("data");
        nullItemsData.GetProperty("items").ValueKind.Should().Be(JsonValueKind.Null);
        nullItemsData.GetProperty("totalItems").ValueKind.Should().Be(JsonValueKind.Null);
        nullItemsData.GetProperty("page").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task DevTools_sensitive_metadata_payload_uses_only_fake_values()
    {
        await AuthorizeAsAdminAsync();

        var response = await _client.GetAsync("/api/dev-tools/payloads/sensitive-metadata");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("fake-token-for-testing");
        body.Should().Contain("fake-password");
        body.Should().Contain("fake/storage/key.jpg");
        body.Should().Contain("https://example.com/file.jpg?token=fake");
        body.Should().NotContain("TU_API_KEY");
        body.Should().NotContain("REEMPLAZAR");
        body.Should().NotContain("sk-");
    }

    private async Task<JsonDocument> GetJsonAsync(string uri)
    {
        var response = await _client.GetAsync(uri);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }

    private Task AuthorizeAsAdminAsync()
    {
        return AuthorizeAsync("admin.tests@example.com", "Admin123456");
    }

    private Task AuthorizeAsUserAsync()
    {
        return AuthorizeAsync("usuario.tests@example.com", "Usuario123456");
    }

    private async Task AuthorizeAsync(string email, string password)
    {
        var token = await LoginAsync(email, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<string> LoginAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/login", new { email, password });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").GetProperty("token").GetString()
            ?? throw new InvalidOperationException("Login response did not include token.");
    }
}
