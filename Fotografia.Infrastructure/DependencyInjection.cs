using Fotografia.Application.Services.Interfaces;
using Fotografia.Infrastructure.Data;
using Fotografia.Infrastructure.Security;
using Fotografia.Infrastructure.Services;
using Fotografia.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fotografia.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<MercadoPagoSettings>(configuration.GetSection(MercadoPagoSettings.SectionName));
        services.Configure<ResendSettings>(configuration.GetSection(ResendSettings.SectionName));
        services.Configure<CloudflareR2Settings>(configuration.GetSection(CloudflareR2Settings.SectionName));
        services.Configure<DatabaseSettings>(configuration.GetSection(DatabaseSettings.SectionName));

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection no esta configurado.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddHttpClient();
        services.AddSingleton<JwtHelper>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEventoService, EventoService>();
        services.AddScoped<IFotoService, FotoService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IPedidoService, PedidoService>();
        services.AddScoped<IMercadoPagoService, MercadoPagoService>();
        services.AddScoped<IDescargaService, DescargaService>();
        services.AddScoped<IStorageService, CloudflareR2StorageService>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
