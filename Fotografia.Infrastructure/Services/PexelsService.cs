using System.Net.Http.Json;
using System.Text.Json;
using Fotografia.Application.DTOs.Pexels;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Infrastructure.Services.Pexels;
using Fotografia.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fotografia.Infrastructure.Services;

public sealed class PexelsService(
    HttpClient httpClient,
    IOptions<PexelsSettings> options,
    ILogger<PexelsService> logger) : IPexelsService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly PexelsSettings _settings = options.Value;

    public async Task<IReadOnlyList<PexelsPhotoDto>> BuscarFotosAsync(
        string query,
        int cantidad,
        CancellationToken cancellationToken = default)
    {
        var isApiKeyConfigured = IsApiKeyConfigured(_settings.ApiKey);
        logger.LogInformation("Pexels API Key configurada: {Configurada}", isApiKeyConfigured ? "sí" : "no");

        if (!isApiKeyConfigured)
        {
            throw new InvalidOperationException(
                "Pexels API Key no configurada. Configurá Pexels:ApiKey con appsettings.Local.json, user-secrets o variable de entorno.");
        }

        var baseUrl = string.IsNullOrWhiteSpace(_settings.BaseUrl)
            ? "https://api.pexels.com/v1"
            : _settings.BaseUrl.TrimEnd('/');

        var maxPerPage = Math.Clamp(_settings.MaxPerPage <= 0 ? 80 : _settings.MaxPerPage, 1, 80);
        var defaultPerPage = Math.Clamp(_settings.DefaultPerPage <= 0 ? maxPerPage : _settings.DefaultPerPage, 1, maxPerPage);
        var results = new List<PexelsPhotoDto>(cantidad);
        var seenPhotoIds = new HashSet<long>();
        var page = 1;

        while (results.Count < cantidad)
        {
            var remaining = cantidad - results.Count;
            var perPage = Math.Min(defaultPerPage, remaining);
            var url = $"{baseUrl}/search?query={Uri.EscapeDataString(query)}&per_page={perPage}&page={page}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.TryAddWithoutValidation("Authorization", _settings.ApiKey);

            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Pexels returned status {StatusCode} while searching photos.", response.StatusCode);
                throw new HttpRequestException("Pexels no pudo procesar la busqueda de fotos.");
            }

            var searchResponse = await response.Content.ReadFromJsonAsync<PexelsSearchResponse>(JsonOptions, cancellationToken);
            if (searchResponse?.Photos is null || searchResponse.Photos.Count == 0)
            {
                break;
            }

            foreach (var photo in searchResponse.Photos)
            {
                if (!seenPhotoIds.Add(photo.Id))
                {
                    continue;
                }

                results.Add(new PexelsPhotoDto
                {
                    Id = photo.Id,
                    Width = photo.Width,
                    Height = photo.Height,
                    OriginalUrl = photo.Src?.Original,
                    PreviewUrl = photo.Src?.Large ?? photo.Src?.Medium ?? photo.Src?.Large2x ?? photo.Src?.Original,
                    Photographer = photo.Photographer,
                    PhotographerUrl = photo.PhotographerUrl
                });

                if (results.Count == cantidad)
                {
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(searchResponse.NextPage))
            {
                break;
            }

            page++;
        }

        return results;
    }

    private static bool IsApiKeyConfigured(string? apiKey)
    {
        return !string.IsNullOrWhiteSpace(apiKey)
            && !apiKey.StartsWith("__", StringComparison.Ordinal)
            && !apiKey.Equals("REEMPLAZAR_POR_API_KEY", StringComparison.OrdinalIgnoreCase);
    }
}
