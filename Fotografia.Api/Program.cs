using System.Text;
using Fotografia.Api.Helpers;
using Fotografia.Api.Middleware;
using Fotografia.Application;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Infrastructure;
using Fotografia.Infrastructure.Health;
using Fotografia.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
ApplyLocalSettingsFile(builder.Configuration, builder.Environment.ContentRootPath, Directory.GetCurrentDirectory());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fotografia Backend API",
        Version = "v1",
        Description = "API REST para gestion de eventos, fotos, pedidos, pagos y descargas."
    });

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Ingrese el token JWT obtenido en /api/auth/login."
    });

    options.OperationFilter<AuthorizeOperationFilter>();
});
builder.Services.AddProblemDetails();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddHealthChecks()
    .AddCheck<AppDbContextHealthCheck>("database", tags: ["ready"]);

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("La seccion Jwt no esta configurada.");
ValidateJwtSettingsForAuthentication(jwtSettings, builder.Environment.IsDevelopment());

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    var permitLimit = Math.Max(1, builder.Configuration.GetValue<int?>("RateLimiting:SensitivePublic:PermitLimit") ?? 60);
    var windowSeconds = Math.Max(1, builder.Configuration.GetValue<int?>("RateLimiting:SensitivePublic:WindowSeconds") ?? 60);
    var queueLimit = Math.Max(0, builder.Configuration.GetValue<int?>("RateLimiting:SensitivePublic:QueueLimit") ?? 0);

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("SensitivePublic", limiterOptions =>
    {
        limiterOptions.PermitLimit = permitLimit;
        limiterOptions.Window = TimeSpan.FromSeconds(windowSeconds);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = queueLimit;
        limiterOptions.AutoReplenishment = true;
    });
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(
            ApiResponse<object>.Fail("Demasiadas solicitudes. Intente nuevamente en unos minutos.", StatusCodes.Status429TooManyRequests),
            cancellationToken);
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularFrontend", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
startupLogger.LogInformation(
    "Pexels API Key configurada: {Configurada}",
    string.IsNullOrWhiteSpace(app.Configuration["Pexels:ApiKey"]) ? "no" : "si");

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        var logger = context.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("GlobalExceptionHandler");
        var correlationId = GetCorrelationId(context);

        logger.LogError(
            exceptionFeature?.Error,
            "Unhandled exception. TraceIdentifier={TraceIdentifier} CorrelationId={CorrelationId}",
            context.TraceIdentifier,
            correlationId);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(
            new
            {
                Success = false,
                Message = "Ocurrio un error inesperado.",
                Data = (object?)null,
                Errors = Array.Empty<string>(),
                StatusCode = StatusCodes.Status500InternalServerError,
                TraceId = context.TraceIdentifier,
                CorrelationId = correlationId
            },
            context.RequestAborted);
    });
});

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/dev-tools")
        && !IsDevToolsEnvironment(app.Environment))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    await next(context);
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Fotografia Backend API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Fotografia Backend API";
    });
}

await using (var scope = app.Services.CreateAsyncScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
    await initializer.InitializeAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AngularFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
}).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready", StringComparer.OrdinalIgnoreCase)
}).AllowAnonymous();
app.MapGet("/", () => Results.Redirect("/swagger")).AllowAnonymous();

app.Run();

static void ValidateJwtSettingsForAuthentication(JwtSettings settings, bool isDevelopment)
{
    var signingKey = settings.SigningKey?.Trim();
    if (string.IsNullOrWhiteSpace(settings.Issuer)
        || string.IsNullOrWhiteSpace(settings.Audience)
        || string.IsNullOrWhiteSpace(signingKey)
        || signingKey.Length < 32)
    {
        throw new InvalidOperationException("Jwt no esta configurado correctamente. Configure Jwt:Issuer, Jwt:Audience y Jwt:SigningKey de al menos 32 caracteres.");
    }

    if (!isDevelopment && signingKey.StartsWith("__", StringComparison.Ordinal))
    {
        throw new InvalidOperationException("Jwt:SigningKey no puede usar placeholders en ambientes productivos.");
    }
}

static string? GetCorrelationId(HttpContext context)
{
    return context.Items.TryGetValue(CorrelationIdMiddleware.ItemName, out var value)
        ? value as string
        : null;
}

static bool IsDevToolsEnvironment(IHostEnvironment environment)
{
    return environment.IsDevelopment() || environment.IsStaging();
}

static void ApplyLocalSettingsFile(IConfiguration configuration, params string[] basePaths)
{
    var candidatePaths = GetLocalSettingsCandidatePaths(basePaths);

    foreach (var candidatePath in candidatePaths)
    {
        if (!File.Exists(candidatePath))
        {
            continue;
        }

        var directory = Path.GetDirectoryName(candidatePath);
        var fileName = Path.GetFileName(candidatePath);

        if (string.IsNullOrWhiteSpace(directory) || string.IsNullOrWhiteSpace(fileName))
        {
            continue;
        }

        var localConfiguration = new ConfigurationBuilder()
            .SetBasePath(directory)
            .AddJsonFile(fileName, optional: false, reloadOnChange: false)
            .Build();

        foreach (var localValue in localConfiguration.AsEnumerable().Where(value => value.Value is not null))
        {
            if (string.IsNullOrWhiteSpace(configuration[localValue.Key]))
            {
                configuration[localValue.Key] = localValue.Value;
            }
        }
    }
}

static List<string> GetLocalSettingsCandidatePaths(params string[] basePaths)
{
    var candidatePaths = new List<string>();

    foreach (var basePath in basePaths.Where(path => !string.IsNullOrWhiteSpace(path)))
    {
        candidatePaths.Add(Path.Combine(basePath, "appsettings.Local.json"));
        candidatePaths.Add(Path.Combine(basePath, "Fotografia.Api", "appsettings.Local.json"));
    }

    return [.. candidatePaths
        .Select(Path.GetFullPath)
        .Distinct(StringComparer.OrdinalIgnoreCase)];
}

public partial class Program;
