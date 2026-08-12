using Fotografia.Application.Services.Interfaces;
using Fotografia.Application.Security;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Fotografia.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Fotografia.Tests.Integration;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string TestIssuer = "Fotografia.Api.Tests";
    private const string TestAudience = "Fotografia.Angular.Tests";
    private const string TestSigningKey = "tests-local-signing-key-with-more-than-32-chars";
    private readonly string _environment;

    public CustomWebApplicationFactory()
        : this("Development")
    {
    }

    internal CustomWebApplicationFactory(string environment)
    {
        _environment = environment;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(_environment);

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:",
                ["Jwt:Issuer"] = TestIssuer,
                ["Jwt:Audience"] = TestAudience,
                ["Jwt:SigningKey"] = TestSigningKey,
                ["Jwt:ExpirationMinutes"] = "60",
                ["Cors:AllowedOrigins:0"] = "http://localhost:5173",
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
            var databaseName = $"fotografia-integration-{Guid.NewGuid():N}";

            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.RemoveAll<IDatabaseInitializer>();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(databaseName));

            services.Configure<JwtSettings>(settings =>
            {
                settings.Issuer = TestIssuer;
                settings.Audience = TestAudience;
                settings.SigningKey = TestSigningKey;
                settings.ExpirationMinutes = 60;
            });
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters.ValidIssuer = TestIssuer;
                options.TokenValidationParameters.ValidAudience = TestAudience;
                options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSigningKey));
            });

            services.AddScoped<IDatabaseInitializer, NoopDatabaseInitializer>();
        });
    }

    private sealed class NoopDatabaseInitializer(AppDbContext dbContext) : IDatabaseInitializer
    {
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);

            if (await dbContext.Usuarios.AnyAsync(cancellationToken))
            {
                return;
            }

            var passwordHasher = new PasswordHasher<Usuario>();
            var admin = new Usuario
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Nombre = "Admin Test",
                Email = "admin.tests@example.com",
                PasswordHash = string.Empty,
                Rol = SistemaRoles.Admin,
                Activo = true
            };
            admin.PasswordHash = passwordHasher.HashPassword(admin, "Admin123456");

            var user = new Usuario
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Nombre = "Usuario Test",
                Email = "usuario.tests@example.com",
                PasswordHash = string.Empty,
                Rol = SistemaRoles.Usuario,
                Activo = true
            };
            user.PasswordHash = passwordHasher.HashPassword(user, "Usuario123456");

            dbContext.Usuarios.AddRange(admin, user);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
