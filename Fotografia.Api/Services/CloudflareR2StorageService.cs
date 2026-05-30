using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Fotografia.Api.DTOs.Descargas;
using Fotografia.Api.DTOs.Fotos;
using Fotografia.Api.Helpers;
using Fotografia.Api.Services.Interfaces;
using Fotografia.Api.Settings;
using Microsoft.Extensions.Options;

namespace Fotografia.Api.Services;

public sealed class CloudflareR2StorageService(IOptions<CloudflareR2Settings> options) : IStorageService
{
    private const string Algorithm = "AWS4-HMAC-SHA256";
    private const string Region = "auto";
    private const string Service = "s3";
    private readonly CloudflareR2Settings _settings = options.Value;

    public ApiResponse<StorageKeyResponseDto> GenerateStorageKey(Guid eventoId, string fileName)
    {
        return ApiResponse<StorageKeyResponseDto>.Ok(new StorageKeyResponseDto
        {
            StorageKey = FileHelper.CreateStorageKey(eventoId, fileName)
        });
    }

    public Task<ApiResponse<LinkDescargaResponseDto>> CreateTemporaryDownloadUrlAsync(
        Guid pedidoId,
        Guid fotoId,
        string storageKey,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.AccessKeyId)
            || string.IsNullOrWhiteSpace(_settings.SecretAccessKey)
            || string.IsNullOrWhiteSpace(_settings.BucketName))
        {
            return Task.FromResult(ApiResponse<LinkDescargaResponseDto>.Fail(
                "Cloudflare R2 no esta configurado para generar URLs firmadas."));
        }

        var endpoint = ResolveEndpoint();
        if (endpoint is null)
        {
            return Task.FromResult(ApiResponse<LinkDescargaResponseDto>.Fail(
                "Configure CloudflareR2:EndpointUrl o CloudflareR2:AccountId."));
        }

        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(Math.Max(1, _settings.SignedUrlExpirationMinutes));
        var expiresInSeconds = Math.Min((int)(expires - now).TotalSeconds, 604800);
        var host = endpoint.Host;
        var canonicalUri = $"/{AwsEncode(_settings.BucketName)}/{EncodePath(storageKey)}";
        var credentialScope = $"{now:yyyyMMdd}/{Region}/{Service}/aws4_request";
        var credential = $"{_settings.AccessKeyId}/{credentialScope}";
        var disposition = $"attachment; filename=\"{fileName.Replace("\"", string.Empty)}\"";

        var query = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["X-Amz-Algorithm"] = Algorithm,
            ["X-Amz-Credential"] = credential,
            ["X-Amz-Date"] = now.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture),
            ["X-Amz-Expires"] = expiresInSeconds.ToString(CultureInfo.InvariantCulture),
            ["X-Amz-SignedHeaders"] = "host",
            ["response-content-disposition"] = disposition
        };

        var canonicalQuery = CreateCanonicalQueryString(query);
        var canonicalHeaders = $"host:{host}\n";
        var signedHeaders = "host";
        var canonicalRequest = $"GET\n{canonicalUri}\n{canonicalQuery}\n{canonicalHeaders}\n{signedHeaders}\nUNSIGNED-PAYLOAD";
        var hashedRequest = ToHex(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalRequest)));
        var stringToSign = $"{Algorithm}\n{query["X-Amz-Date"]}\n{credentialScope}\n{hashedRequest}";
        var signature = ToHex(HmacSha256(GetSigningKey(now).Span, stringToSign).Span);
        var url = $"{endpoint.Scheme}://{host}{canonicalUri}?{canonicalQuery}&X-Amz-Signature={signature}";

        return Task.FromResult(ApiResponse<LinkDescargaResponseDto>.Ok(new LinkDescargaResponseDto
        {
            PedidoId = pedidoId,
            FotoId = fotoId,
            NombreArchivo = fileName,
            Url = url,
            ExpiraEnUtc = expires
        }));
    }

    private Uri? ResolveEndpoint()
    {
        if (!string.IsNullOrWhiteSpace(_settings.EndpointUrl)
            && Uri.TryCreate(_settings.EndpointUrl, UriKind.Absolute, out var configured))
        {
            return configured;
        }

        if (!string.IsNullOrWhiteSpace(_settings.AccountId))
        {
            return new Uri($"https://{_settings.AccountId}.r2.cloudflarestorage.com");
        }

        return null;
    }

    private ReadOnlyMemory<byte> GetSigningKey(DateTime now)
    {
        var dateKey = HmacSha256(Encoding.UTF8.GetBytes($"AWS4{_settings.SecretAccessKey}"), now.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
        var regionKey = HmacSha256(dateKey.Span, Region);
        var serviceKey = HmacSha256(regionKey.Span, Service);
        return HmacSha256(serviceKey.Span, "aws4_request");
    }

    private static string CreateCanonicalQueryString(SortedDictionary<string, string> query)
    {
        return string.Join("&", query.Select(item => $"{AwsEncode(item.Key)}={AwsEncode(item.Value)}"));
    }

    private static string EncodePath(string path)
    {
        return string.Join("/", path.Split('/', StringSplitOptions.RemoveEmptyEntries).Select(AwsEncode));
    }

    private static string AwsEncode(string value)
    {
        return Uri.EscapeDataString(value).Replace("%7E", "~", StringComparison.Ordinal);
    }

    private static ReadOnlyMemory<byte> HmacSha256(ReadOnlySpan<byte> key, string value)
    {
        return HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(value));
    }

    private static string ToHex(ReadOnlySpan<byte> bytes)
    {
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
