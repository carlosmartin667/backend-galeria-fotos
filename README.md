# fotografia-backend-api

Backend ASP.NET Core Web API para gestion de eventos fotograficos, clientes, fotos, pedidos, pagos y descargas.

## Estructura

- `Fotografia.Api`: controllers, Swagger, autenticacion HTTP, CORS y configuracion del host.
- `Fotografia.Application`: DTOs, respuestas comunes, interfaces de servicios y AutoMapper.
- `Fotografia.Domain`: entidades principales del negocio.
- `Fotografia.Infrastructure`: EF Core, SQL Server/Azure SQL, Mercado Pago, Resend, Cloudflare R2 y JWT.

## Ejecucion local

Abrir `fotografia-backend-api.slnx` en Visual Studio 2026 o ejecutar:

```powershell
dotnet run --project Fotografia.Api/Fotografia.Api.csproj
```

La API levanta en `http://localhost:5200` y Swagger queda disponible en `http://localhost:5200/swagger`.

## Datos de prueba

En `Development`, la API aplica migraciones y carga datos de prueba cuando `Database:SeedTestData` esta en `true`.

Usuario administrador de prueba:

```text
Email: carloscornejomoscoso@gmail.com
Password: 12345678
```

El seed crea clientes, eventos, fotos como metadata de R2, pedidos, pagos y descargas de ejemplo. No guarda imagenes binarias en SQL Server.

## Base de datos

La connection string local configurada apunta a SQL Server:

```text
Data Source=.;Initial Catalog=Db_fotografia;Integrated Security=True;Trust Server Certificate=True
```

Las migraciones de Entity Framework Core viven en `Fotografia.Infrastructure/Migrations`.

Comandos utiles:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef migrations add NombreMigracion --project Fotografia.Infrastructure/Fotografia.Infrastructure.csproj --startup-project Fotografia.Api/Fotografia.Api.csproj --context AppDbContext --output-dir Migrations
dotnet tool run dotnet-ef database update --project Fotografia.Infrastructure/Fotografia.Infrastructure.csproj --startup-project Fotografia.Api/Fotografia.Api.csproj --context AppDbContext
```

## Configuracion de Pexels en desarrollo

No guardes la API Key real en archivos versionados.

Opcion simple automatizada con archivo local ignorado por Git:

```powershell
.\scripts\local\create-appsettings-local.ps1
.\scripts\local\run-api-dev.ps1
```

`run-api-dev.ps1` tambien crea `Fotografia.Api/appsettings.Local.json` automaticamente si todavia no existe.
El archivo `Fotografia.Api/appsettings.Local.json` esta ignorado por Git.
Si la API ya estaba corriendo cuando creaste o cambiaste este archivo, detenela y volve a levantarla.

La prioridad de configuracion queda:

```text
appsettings.json
appsettings.Development.json
appsettings.Local.json
user-secrets
variables de entorno
```

Opcion con Secret Manager:

```powershell
.\scripts\local\set-pexels-secret.ps1
.\scripts\local\run-api-dev.ps1
```

Luego proba desde Swagger o Angular:

```http
POST /api/Admin/demo/pexels/importar-fotos
```

Tambien puede configurarse por variable de entorno antes de ejecutar la API:

```powershell
$env:Pexels__ApiKey="TU_API_KEY"
dotnet run --project .\Fotografia.Api\Fotografia.Api.csproj
```

El script real `scripts/local/set-pexels-secret.ps1` esta ignorado por Git. El archivo `scripts/local/set-pexels-secret.example.ps1` queda como referencia sin secretos reales.
