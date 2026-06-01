using System.Net.Http.Headers;
using System.Net.Http.Json;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fotografia.Infrastructure.Services;

public sealed class EmailService(
    HttpClient httpClient,
    IOptions<ResendSettings> options,
    ILogger<EmailService> logger) : IEmailService
{
    private readonly ResendSettings _settings = options.Value;

    public async Task<ApiResponse<bool>> SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey) || string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            return ApiResponse<bool>.Fail("Resend no esta configurado para enviar emails.");
        }

        var from = string.IsNullOrWhiteSpace(_settings.FromName)
            ? _settings.FromEmail
            : $"{_settings.FromName} <{_settings.FromEmail}>";

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        request.Content = JsonContent.Create(new
        {
            from,
            to = new[] { to },
            subject,
            html = htmlBody
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning("Resend rejected email request with status {StatusCode}: {Body}", response.StatusCode, body);
            return ApiResponse<bool>.Fail("No se pudo enviar el email.");
        }

        return ApiResponse<bool>.Ok(true, "Email enviado.");
    }
}
