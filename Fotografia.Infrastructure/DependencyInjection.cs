using Fotografia.Application.Services.Interfaces;
using Fotografia.Infrastructure.Data;
using Fotografia.Infrastructure.Security;
using Fotografia.Infrastructure.Services;
using Fotografia.Infrastructure.Settings;
using Fotografia.Infrastructure.Settings.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fotografia.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment? environment = null)
    {
        services.AddValidatedInfrastructureOptions(configuration, environment);

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection no esta configurado.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure();
                }));

        services.AddHttpClient();
        services.AddHttpClient<IPexelsService, PexelsService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(20);
        });
        services.AddHttpContextAccessor();
        services.AddSingleton<JwtHelper>();

        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IBitacoraService, BitacoraService>();
        services.AddScoped<IResourceAccessService, ResourceAccessService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEventoService, EventoService>();
        services.AddScoped<IFotoService, FotoService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IClienteHistorialService, ClienteHistorialService>();
        services.AddScoped<IPedidoService, PedidoService>();
        services.AddScoped<IMercadoPagoService, MercadoPagoService>();
        services.AddScoped<IDescargaService, DescargaService>();
        services.AddScoped<IAdminPerfilService, AdminPerfilService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();
        services.AddScoped<IAdminOperacionesService, AdminOperacionesService>();
        services.AddScoped<IAdminVentasService, AdminVentasService>();
        services.AddScoped<IPerfilFotografaService, PerfilFotografaService>();
        services.AddScoped<ISitioPublicoService, SitioPublicoService>();
        services.AddScoped<IPortfolioService, PortfolioService>();
        services.AddScoped<IServicioFotografiaService, ServicioFotografiaService>();
        services.AddScoped<IFaqService, FaqService>();
        services.AddScoped<ISolicitudPresupuestoService, SolicitudPresupuestoService>();
        services.AddScoped<IAgendaService, AgendaService>();
        services.AddScoped<IPaqueteEventoService, PaqueteEventoService>();
        services.AddScoped<ICarritoService, CarritoService>();
        services.AddScoped<ICuponService, CuponService>();
        services.AddScoped<IPromocionService, PromocionService>();
        services.AddScoped<ITestimonioService, TestimonioService>();
        services.AddScoped<ICarritoAbandonadoService, CarritoAbandonadoService>();
        services.AddScoped<IReporteVentasService, ReporteVentasService>();
        services.AddScoped<ISesionPrivadaService, SesionPrivadaService>();
        services.AddScoped<IComentarioEventoService, ComentarioEventoService>();
        services.AddScoped<IComentarioFotoService, ComentarioFotoService>();
        services.AddScoped<IFavoritoService, FavoritoService>();
        services.AddScoped<INotaInternaService, NotaInternaService>();
        services.AddScoped<INotificacionService, NotificacionService>();
        services.AddScoped<IPlantillaNotificacionService, PlantillaNotificacionService>();
        services.AddScoped<IStorageService, CloudflareR2StorageService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddHostedService<NotificationWorkerService>();

        return services;
    }
}
