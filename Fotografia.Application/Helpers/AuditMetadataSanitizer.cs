using System.Text.Json;
using System.Text.Json.Nodes;

namespace Fotografia.Application.Helpers;

public static class AuditMetadataSanitizer
{
    private const int MaxStringLength = 300;
    private const int MaxJsonLength = 4000;
    private const string Redacted = "[REDACTED]";

    private static readonly string[] SensitiveKeyFragments =
    [
        "password",
        "token",
        "access_token",
        "refreshtoken",
        "refresh_token",
        "apikey",
        "api_key",
        "secret",
        "bearer",
        "signedurl",
        "signed_url",
        "init_point",
        "storagekey",
        "storage_key",
        "marcaaguastoragekey",
        "marca_agua_storage_key",
        "providerbody",
        "rawbody",
        "payload"
    ];

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public static string? SanitizeToJson(object? metadata, string? metadataJson = null)
    {
        JsonNode? node = null;

        if (metadata is not null)
        {
            node = JsonSerializer.SerializeToNode(metadata, JsonOptions);
        }
        else if (!string.IsNullOrWhiteSpace(metadataJson))
        {
            try
            {
                node = JsonNode.Parse(metadataJson);
            }
            catch (JsonException)
            {
                node = metadataJson;
            }
        }

        if (node is null)
        {
            return null;
        }

        var sanitized = SanitizeNode(node, propertyName: null);
        var json = sanitized?.ToJsonString(JsonOptions);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return json.Length <= MaxJsonLength
            ? json
            : json[..MaxJsonLength] + "...";
    }

    private static JsonNode? SanitizeNode(JsonNode? node, string? propertyName)
    {
        if (node is null)
        {
            return null;
        }

        if (IsSensitiveKey(propertyName))
        {
            return Redacted;
        }

        return node switch
        {
            JsonObject jsonObject => SanitizeObject(jsonObject),
            JsonArray jsonArray => SanitizeArray(jsonArray, propertyName),
            JsonValue jsonValue => SanitizeValue(jsonValue),
            _ => null
        };
    }

    private static JsonObject SanitizeObject(JsonObject source)
    {
        var target = new JsonObject();
        foreach (var item in source)
        {
            target[item.Key] = SanitizeNode(item.Value, item.Key);
        }

        return target;
    }

    private static JsonArray SanitizeArray(JsonArray source, string? propertyName)
    {
        if (IsSensitiveKey(propertyName) || source.Count > 50)
        {
            return [Redacted];
        }

        var target = new JsonArray();
        foreach (var item in source)
        {
            target.Add(SanitizeNode(item, propertyName));
        }

        return target;
    }

    private static JsonNode? SanitizeValue(JsonValue source)
    {
        if (source.TryGetValue<string>(out var value))
        {
            return SanitizeStringValue(value);
        }

        return JsonSerializer.SerializeToNode(source, JsonOptions);
    }

    private static string? SanitizeStringValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        if (LooksLikeSensitiveUrl(value) || LooksLikeBearerToken(value))
        {
            return Redacted;
        }

        return value.Length <= MaxStringLength
            ? value
            : value[..MaxStringLength] + "...";
    }

    private static bool IsSensitiveKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        var normalized = key.Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .ToLowerInvariant();

        return SensitiveKeyFragments.Any(fragment =>
            normalized.Contains(
                fragment.Replace("_", string.Empty, StringComparison.Ordinal).ToLowerInvariant(),
                StringComparison.Ordinal));
    }

    private static bool LooksLikeSensitiveUrl(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && !string.IsNullOrWhiteSpace(uri.Query)
            && (uri.Query.Contains("token", StringComparison.OrdinalIgnoreCase)
                || uri.Query.Contains("signature", StringComparison.OrdinalIgnoreCase)
                || uri.Query.Contains("X-Amz-Signature", StringComparison.OrdinalIgnoreCase)
                || uri.Query.Contains("access_token", StringComparison.OrdinalIgnoreCase));
    }

    private static bool LooksLikeBearerToken(string value)
    {
        return value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);
    }
}
