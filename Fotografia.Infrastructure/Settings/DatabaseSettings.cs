namespace Fotografia.Infrastructure.Settings;

public sealed class DatabaseSettings
{
    public const string SectionName = "Database";

    public bool ApplyMigrationsOnStartup { get; set; }
    public bool SeedTestData { get; set; }
}
