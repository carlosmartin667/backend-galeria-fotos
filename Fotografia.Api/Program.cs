using System.Text;
using Fotografia.Api.Helpers;
using Fotografia.Application;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Infrastructure;
using Fotografia.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

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
builder.Services.AddInfrastructure(builder.Configuration);

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("La seccion Jwt no esta configurada.");

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
    string.IsNullOrWhiteSpace(app.Configuration["Pexels:ApiKey"]) ? "no" : "sí");

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

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger")).AllowAnonymous();

app.Run();

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

    return candidatePaths
        .Select(Path.GetFullPath)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();
}
