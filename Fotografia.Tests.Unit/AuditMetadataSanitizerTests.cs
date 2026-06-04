using FluentAssertions;
using Fotografia.Application.Helpers;

namespace Fotografia.Tests.Unit;

public sealed class AuditMetadataSanitizerTests
{
    [Fact]
    public void SanitizeToJson_redacts_sensitive_keys_and_signed_urls()
    {
        var json = AuditMetadataSanitizer.SanitizeToJson(new
        {
            StorageKey = "originales/evento/foto.jpg",
            ApiKey = "pexels-secret",
            SignedUrl = "https://r2.example.com/foto.jpg?X-Amz-Signature=abc",
            PublicValue = "ok"
        });

        json.Should().NotBeNull();
        json.Should().Contain("[REDACTED]");
        json.Should().Contain("ok");
        json.Should().NotContain("pexels-secret");
        json.Should().NotContain("originales/evento/foto.jpg");
        json.Should().NotContain("X-Amz-Signature");
    }
}
