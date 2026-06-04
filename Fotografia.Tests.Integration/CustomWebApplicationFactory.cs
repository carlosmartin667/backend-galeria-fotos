using Fotografia.Application.Services.Interfaces;
using Fotografia.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fotografia.Tests.Integration;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:",
                ["Jwt:Issuer"] = "Fotografia.Api.Tests",
                ["Jwt:Audience"] = "Fotografia.Angular.Tests",
                ["Jwt:SigningKey"] = "tests-local-signing-key-with-more-than-32-chars",
                ["Jwt:ExpirationMinutes"] = "60",
                ["Cors:AllowedOrigins:0"] = "http://localhost:4200",
                ["Database:ResetOnStartup"] = "false",
                ["Database:SeedOnStartup"] = "false",
                ["Database:SeedTestData"] = "false",
                ["Database:ApplyMigrationsOnStartup"] = "false",
                ["Notifications:Enabled"] = "false",
                ["Notifications:WorkerEnabled"] = "false",
                ["Notifications:DefaultFromEmail"] = "no-reply@example.com",
                ["Notifications:Worker:IntervalSeconds"] = "30",
                ["Notifications:Worker:BatchSize"] = "20",
                ["Notifications:Worker:MaxAttempts"] = "3",
                ["Pexels:ApiKey"] = string.Empty,
                ["Pexels:BaseUrl"] = "https://api.pexels.com/v1",
                ["Pexels:DefaultPerPage"] = "80",
                ["Pexels:MaxPerPage"] = "80"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.RemoveAll<IDatabaseInitializer>();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase($"fotografia-integration-{Guid.NewGuid():N}"));

            services.AddScoped<IDatabaseInitializer, NoopDatabaseInitializer>();
        });
    }

    private sealed class NoopDatabaseInitializer(AppDbContext dbContext) : IDatabaseInitializer
    {
        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            return dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }
    }
}
