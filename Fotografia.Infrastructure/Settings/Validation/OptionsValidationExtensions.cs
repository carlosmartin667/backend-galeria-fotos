using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fotografia.Infrastructure.Settings.Validation;

public static class OptionsValidationExtensions
{
    public static IServiceCollection AddValidatedInfrastructureOptions(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment? environment = null)
    {
        var isDevelopment = environment?.IsDevelopment() ?? false;

        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .Validate(settings => ValidateJwt(settings, isDevelopment), "Jwt no esta configurado correctamente.")
            .ValidateOnStart();

        services.AddOptions<CorsSettings>()
            .Bind(configuration.GetSection(CorsSettings.SectionName))
            .Validate(ValidateCors, "Cors:AllowedOrigins debe contener URLs absolutas validas.")
            .ValidateOnStart();

        services.AddOptions<DatabaseSettings>()
            .Bind(configuration.GetSection(DatabaseSettings.SectionName))
            .Validate(_ => true)
            .ValidateOnStart();

        services.AddOptions<MercadoPagoSettings>()
            .Bind(configuration.GetSection(MercadoPagoSettings.SectionName))
            .Validate(ValidateMercadoPago, "MercadoPago tiene una configuracion invalida.")
            .ValidateOnStart();

        services.AddOptions<ResendSettings>()
            .Bind(configuration.GetSection(ResendSettings.SectionName))
            .Validate(ValidateResend, "Resend tiene una configuracion invalida.")
            .ValidateOnStart();

        services.AddOptions<CloudflareR2Settings>()
            .Bind(configuration.GetSection(CloudflareR2Settings.SectionName))
            .Validate(ValidateCloudflareR2, "CloudflareR2 tiene una configuracion invalida.")
            .ValidateOnStart();

        services.AddOptions<PexelsSettings>()
            .Bind(configuration.GetSection(PexelsSettings.SectionName))
            .Validate(ValidatePexels, "Pexels tiene una configuracion invalida.")
            .ValidateOnStart();

        services.AddOptions<NotificationsSettings>()
            .Bind(configuration.GetSection(NotificationsSettings.SectionName))
            .Validate(ValidateNotifications, "Notifications tiene una configuracion invalida.")
            .ValidateOnStart();

        return services;
    }

    private static bool ValidateJwt(JwtSettings settings, bool isDevelopment)
    {
        if (string.IsNullOrWhiteSpace(settings.Issuer)
            || string.IsNullOrWhiteSpace(settings.Audience)
            || settings.ExpirationMinutes is < 5 or > 1440
            || string.IsNullOrWhiteSpace(settings.SigningKey)
            || settings.SigningKey.Length < 32)
        {
            return false;
        }

        return isDevelopment || !IsPlaceholder(settings.SigningKey);
    }

    private static bool ValidateCors(CorsSettings settings)
    {
        return settings.AllowedOrigins.Length > 0
            && settings.AllowedOrigins.All(IsAbsoluteHttpUrl);
    }

    private static bool ValidateMercadoPago(MercadoPagoSettings settings)
    {
        if (!IsConfiguredSecret(settings.AccessToken))
        {
            return true;
        }

        return IsOptionalAbsoluteHttpUrl(settings.SuccessUrl)
            && IsOptionalAbsoluteHttpUrl(settings.FailureUrl)
            && IsOptionalAbsoluteHttpUrl(settings.PendingUrl)
            && IsOptionalAbsoluteHttpUrl(settings.NotificationUrl);
    }

    private static bool ValidateResend(ResendSettings settings)
    {
        if (!IsConfiguredSecret(settings.ApiKey))
        {
            return true;
        }

        return IsEmailLike(settings.FromEmail)
            && !string.IsNullOrWhiteSpace(settings.FromName);
    }

    private static bool ValidateCloudflareR2(CloudflareR2Settings settings)
    {
        if (settings.SignedUrlExpirationMinutes is < 1 or > 10080)
        {
            return false;
        }

        var hasAnySecret = IsConfiguredSecret(settings.AccountId)
            || IsConfiguredSecret(settings.AccessKeyId)
            || IsConfiguredSecret(settings.SecretAccessKey);

        if (!hasAnySecret)
        {
            return true;
        }

        return IsConfiguredSecret(settings.AccessKeyId)
            && IsConfiguredSecret(settings.SecretAccessKey)
            && !string.IsNullOrWhiteSpace(settings.BucketName)
            && (IsConfiguredSecret(settings.AccountId) || IsOptionalAbsoluteHttpUrl(settings.EndpointUrl));
    }

    private static bool ValidatePexels(PexelsSettings settings)
    {
        return IsAbsoluteHttpUrl(settings.BaseUrl)
            && settings.DefaultPerPage is >= 1 and <= 80
            && settings.MaxPerPage is >= 1 and <= 80
            && settings.DefaultPerPage <= settings.MaxPerPage;
    }

    private static bool ValidateNotifications(NotificationsSettings settings)
    {
        return settings.Worker is not null
            && settings.Worker.IntervalSeconds is >= 5 and <= 3600
            && settings.Worker.BatchSize is >= 1 and <= 500
            && settings.Worker.MaxAttempts is >= 1 and <= 20
            && (!settings.Enabled || IsEmailLike(settings.DefaultFromEmail));
    }

    private static bool IsConfiguredSecret(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && !IsPlaceholder(value);
    }

    private static bool IsPlaceholder(string value)
    {
        var normalized = value.Trim();
        return normalized.StartsWith("__", StringComparison.Ordinal)
            || normalized.Contains("REEMPLAZAR", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("TU_API_KEY", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains('<', StringComparison.Ordinal)
            || normalized.Contains('>', StringComparison.Ordinal);
    }

    private static bool IsOptionalAbsoluteHttpUrl(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            || IsPlaceholder(value)
            || IsAbsoluteHttpUrl(value);
    }

    private static bool IsAbsoluteHttpUrl(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static bool IsEmailLike(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.Contains('@', StringComparison.Ordinal)
            && !IsPlaceholder(value);
    }
}
