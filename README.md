# fotografia-backend-api

Backend ASP.NET Core Web API para una plataforma profesional de galeria fotografica. El sistema permite publicar eventos, vender fotos digitales, gestionar sesiones privadas, operar pedidos, pagos, descargas, promociones, agenda, presupuestos, notificaciones y auditoria administrativa.

Este repositorio contiene solo el backend. El frontend Angular vive en otro repositorio y consume esta API mediante endpoints REST.

## Objetivo Del Sistema

La API esta pensada para una fotografa profesional que necesita:

- publicar eventos y galerias publicas;
- administrar clientes, sesiones privadas y fotos privadas;
- vender fotos individuales, paquetes y productos digitales;
- gestionar carrito, cupones, pedidos y pagos;
- generar descargas protegidas con links temporales;
- mantener una web publica comercial con portfolio, servicios, FAQ, promociones y testimonios;
- recibir solicitudes de presupuesto y organizar agenda;
- operar notificaciones, reportes y bitacora administrativa.

## Stack Tecnico

- .NET 10 / ASP.NET Core 10 Web API con controllers.
- C# con nullable/implicit usings habilitados por proyecto.
- Entity Framework Core 10.
- SQL Server / Azure SQL.
- JWT Bearer authentication.
- Roles: `Admin`, `Usuario` e Invitado anonimo.
- Swagger / OpenAPI con Swashbuckle.
- Mercado Pago Checkout Pro y webhooks.
- Resend para emails.
- Cloudflare R2 para almacenamiento real de fotos.
- Pexels para carga demo de metadata, sin descargar imagenes.
- xUnit, FluentAssertions, NSubstitute, WebApplicationFactory y coverlet.
- GitHub Actions para build/test backend.

## Arquitectura Resumida

La solucion esta dividida por capas:

- `Fotografia.Api`: host ASP.NET Core, controllers, Swagger, CORS, JWT, rate limiting, health checks y middleware.
- `Fotografia.Application`: DTOs, respuestas comunes, interfaces de servicios, AutoMapper y seguridad compartida.
- `Fotografia.Domain`: entidades y constantes del dominio.
- `Fotografia.Infrastructure`: EF Core, SQL Server, servicios concretos, integraciones externas, JWT, seed y background workers.

Los controllers son delgados y delegan la logica en services. Los contratos HTTP usan DTOs y `ApiResponse<T>`. EF Core queda encerrado en Infrastructure. Las reglas de ownership y visibilidad se centralizan en `ResourceAccessService` y services de dominio.

Mas detalle:

- [Arquitectura](ARCHITECTURE.md)
- [Seguridad](SECURITY.md)
- [Testing](TESTING.md)
- [Demo](DEMO.md)
- [Guia de entrevista](INTERVIEW_GUIDE.md)
- [Decisiones tecnicas](DECISIONS.md)
- [Roadmap](ROADMAP.md)

## Modulos Principales

- Autenticacion y roles.
- Admin dashboard, operaciones y ventas.
- Eventos, fotos, portada, paquetes y comentarios.
- Clientes, historial y notas internas.
- Favoritos de eventos y fotos.
- Carrito mixto, cupones y pedidos.
- Pagos con Mercado Pago.
- Descargas protegidas y regeneracion de links.
- Sesiones privadas y fotos privadas.
- Perfil fotografa, sitio publico, portfolio, servicios y FAQ.
- Presupuestos y agenda.
- Promociones, testimonios y carritos abandonados.
- Notificaciones, plantillas y worker de envio.
- Reportes de ventas.
- Bitacora/auditoria con metadata sanitizada.

## Ejecucion Local Backend

Abrir `fotografia-backend-api.slnx` en Visual Studio 2026 o ejecutar:

```powershell
dotnet tool restore
dotnet restore .\fotografia-backend-api.slnx
dotnet build .\fotografia-backend-api.slnx
dotnet run --project .\Fotografia.Api\Fotografia.Api.csproj
```

La API levanta en:

```text
http://localhost:5200
```

Swagger queda disponible en:

```text
http://localhost:5200/swagger
```

## Frontend Angular

El frontend no esta en este repositorio. Para demo local, levantarlo desde su propio repo apuntando al backend:

```text
API base URL: http://localhost:5200
```

Los comandos exactos dependen del repositorio Angular. Un flujo tipico seria `npm install` y `ng serve`, pero no se versiona ni se modifica nada de Angular aca.

## Configuracion Segura

No guardar secretos reales en Git, `appsettings.json` ni `appsettings.Development.json`.

Usar Secret Manager:

```powershell
dotnet user-secrets set "Jwt:SigningKey" "TU_CLAVE_LOCAL_DE_32_CARACTERES_O_MAS" --project .\Fotografia.Api\Fotografia.Api.csproj
dotnet user-secrets set "Pexels:ApiKey" "TU_API_KEY" --project .\Fotografia.Api\Fotografia.Api.csproj
dotnet user-secrets set "Resend:ApiKey" "TU_RESEND_API_KEY" --project .\Fotografia.Api\Fotografia.Api.csproj
dotnet user-secrets set "MercadoPago:AccessToken" "TU_ACCESS_TOKEN" --project .\Fotografia.Api\Fotografia.Api.csproj
```

O variables de entorno:

```powershell
$env:Jwt__SigningKey="TU_CLAVE_LOCAL_DE_32_CARACTERES_O_MAS"
$env:Pexels__ApiKey="TU_API_KEY"
$env:Resend__ApiKey="TU_RESEND_API_KEY"
$env:MercadoPago__AccessToken="TU_ACCESS_TOKEN"
```

Para desarrollo local tambien existe soporte opcional para `Fotografia.Api/appsettings.Local.json`, ignorado por Git:

```powershell
.\scripts\local\create-appsettings-local.ps1
.\scripts\local\run-api-dev.ps1
```

Prioridad esperada de configuracion:

```text
appsettings.json
appsettings.Development.json
appsettings.Local.json
user-secrets
variables de entorno
```

## Base De Datos Y Migraciones

Las migraciones de EF Core viven en `Fotografia.Infrastructure/Migrations`.

Aplicar migraciones:

```powershell
dotnet tool run dotnet-ef database update --project .\Fotografia.Infrastructure\Fotografia.Infrastructure.csproj --startup-project .\Fotografia.Api\Fotografia.Api.csproj --context AppDbContext
```

Crear una migracion nueva cuando haya cambios de modelo:

```powershell
dotnet tool run dotnet-ef migrations add NombreMigracion --project .\Fotografia.Infrastructure\Fotografia.Infrastructure.csproj --startup-project .\Fotografia.Api\Fotografia.Api.csproj --context AppDbContext --output-dir Migrations
```

En `Development`, el seed puede cargar datos demo si `Database:SeedOnStartup` esta en `true`. `Database:ResetOnStartup` debe quedar en `false` por defecto y solo puede usarse en Development.

## Datos Demo

El README ya documentaba un usuario administrador de desarrollo creado por seed:

```text
Email: carloscornejomoscoso@gmail.com
Password: 12345678
```

Usar solo en entorno local/demo. No reutilizar estas credenciales en staging ni produccion.

## Tests

Ejecutar todo:

```powershell
dotnet test .\fotografia-backend-api.slnx
```

Por proyecto:

```powershell
dotnet test .\Fotografia.Tests.Unit\Fotografia.Tests.Unit.csproj
dotnet test .\Fotografia.Tests.Integration\Fotografia.Tests.Integration.csproj
```

Los tests cubren helpers, servicios, ownership, respuestas comunes, health checks, endpoints publicos/protegidos y bitacora. Ver [TESTING.md](TESTING.md).

## Health Checks

```http
GET /health
GET /health/live
GET /health/ready
```

- `/health/live`: proceso vivo.
- `/health/ready`: readiness con verificacion de base de datos.
- Ningun health check expone secretos.

## Endpoints Principales

- `POST /api/Auth/login`
- `POST /api/Auth/register`
- `GET /api/Admin/dashboard`
- `GET /api/Eventos`
- `GET /api/Eventos/paginado`
- `GET /api/Fotos/evento/{eventoId}`
- `GET /api/Sitio/home`
- `GET /api/Sitio/perfil-fotografa`
- `GET /api/Portfolio`
- `GET /api/Servicios`
- `GET /api/Faq`
- `GET /api/Testimonios`
- `GET /api/Promociones`
- `GET /api/Agenda/disponibilidad`
- `POST /api/Carrito/crear-pedido`
- `POST /api/Pagos/checkout-pro/preferencias`
- `POST /api/Pagos/webhooks/mercado-pago`
- `POST /api/Descargas/link`
- `GET /api/Descargas/mis-descargas`
- `GET /api/Bitacora`
- `GET /api/Reportes/ventas/resumen`

Swagger muestra el contrato completo y los requisitos de Authorization.

## Estado Actual De Fases

- Fase base: estructura por capas, entidades, EF Core, SQL Server, Swagger, CORS y JWT.
- Roles y ownership: Admin, Usuario e Invitado anonimo para endpoints publicos.
- Galerias: eventos, fotos, comentarios, favoritos, portada, visibilidad y bulk metadata.
- Ecommerce: carrito, paquetes, pedidos, cupones, promociones, pagos y descargas.
- Sesiones privadas: clientes, fotos privadas, compras y descargas protegidas.
- Sitio publico: perfil fotografa, portfolio, servicios, FAQ, testimonios y promociones.
- Operacion admin: dashboard, ventas, agenda, presupuestos, notas, notificaciones y reportes.
- Calidad tecnica: health checks, rate limiting, correlation id, tests, CI y bitacora/auditoria.

## Seguridad Basica

- No se guardan imagenes binarias en SQL Server.
- Las fotos reales se almacenan en Cloudflare R2.
- `StorageKey` y `MarcaAguaStorageKey` se protegen en DTOs publicos/no admin.
- Las descargas usan URLs temporales o firmadas.
- Los logs y bitacora no deben guardar secrets, tokens ni URLs firmadas.
- Los endpoints publicos usan `[AllowAnonymous]` solo donde corresponde.
- Los endpoints protegidos usan JWT y roles.

## CI

El repo incluye GitHub Actions para backend con restore, build y tests. El frontend debe tener su propio pipeline en su repositorio.
