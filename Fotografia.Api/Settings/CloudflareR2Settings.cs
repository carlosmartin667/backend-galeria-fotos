namespace Fotografia.Api.Settings;

public sealed class CloudflareR2Settings
{
    public const string SectionName = "CloudflareR2";

    public string AccountId { get; set; } = string.Empty;
    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretAccessKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string EndpointUrl { get; set; } = string.Empty;
    public string PublicBaseUrl { get; set; } = string.Empty;
    public int SignedUrlExpirationMinutes { get; set; } = 15;
}
