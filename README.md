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
