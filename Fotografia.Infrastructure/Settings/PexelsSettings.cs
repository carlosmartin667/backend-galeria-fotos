namespace Fotografia.Infrastructure.Settings;

public sealed class PexelsSettings
{
    public const string SectionName = "Pexels";

    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.pexels.com/v1";
    public int DefaultPerPage { get; set; } = 80;
    public int MaxPerPage { get; set; } = 80;
}
