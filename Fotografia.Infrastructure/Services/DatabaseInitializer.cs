using Fotografia.Application.Services.Interfaces;
using Fotografia.Infrastructure.Data;
using Fotografia.Infrastructure.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Fotografia.Infrastructure.Services;

public sealed class DatabaseInitializer(
    AppDbContext dbContext,
    IWebHostEnvironment environment,
    IConfiguration configuration,
    ILogger<DatabaseInitializer> logger) : IDatabaseInitializer
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var settings = configuration.GetSection(DatabaseSettings.SectionName).Get<DatabaseSettings>() ?? new DatabaseSettings();
        var isDevelopment = environment.EnvironmentName.Equals("Development", StringComparison.OrdinalIgnoreCase);

        try
        {
            if (isDevelopment && settings.ResetOnStartup)
            {
                logger.LogWarning("Database reset is enabled for Development. The database will be deleted and recreated.");
                await dbContext.Database.EnsureDeletedAsync(cancellationToken);
                await dbContext.Database.MigrateAsync(cancellationToken);
                await DbInitializer.SeedTestDataAsync(dbContext, cancellationToken);
                return;
            }

            if (isDevelopment || settings.ApplyMigrationsOnStartup)
            {
                await dbContext.Database.MigrateAsync(cancellationToken);
            }

            var shouldSeed = isDevelopment
                ? settings.SeedOnStartup || settings.SeedTestData
                : settings.SeedTestData;

            if (shouldSeed)
            {
                await DbInitializer.SeedTestDataAsync(dbContext, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initializing database.");
            throw;
        }
    }
}
