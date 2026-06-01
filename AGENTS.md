# AGENTS.md

Guia para Codex y otros agentes que trabajen en este repositorio backend.

## Alcance del repositorio

- Este repositorio contiene solo el backend ASP.NET Core Web API.
- No agregar codigo Angular ni assets del frontend.
- El frontend Angular vive en otro repositorio y consume esta API por endpoints REST.

## Stack esperado

- ASP.NET Core Web API con controllers.
- C# y Entity Framework Core.
- SQL Server o Azure SQL para datos relacionales.
- OpenAPI para contrato HTTP.
- Mercado Pago Checkout Pro para pagos y webhooks.
- Resend para emails.
- Cloudflare R2 para almacenamiento real de fotos.

## Reglas de arquitectura

- Los controllers deben permanecer delgados.
- La solucion esta dividida en capas:
  - `Fotografia.Api`: host ASP.NET Core, controllers, Swagger, CORS y autenticacion HTTP.
  - `Fotografia.Application`: DTOs, respuestas comunes, interfaces de servicios y mapeos.
  - `Fotografia.Domain`: entidades del dominio.
  - `Fotografia.Infrastructure`: EF Core, SQL Server, servicios concretos, Mercado Pago, Resend, Cloudflare R2 y JWT.
- La logica de negocio va en servicios registrados desde `Fotografia.Infrastructure`.
- Los contratos HTTP deben usar DTOs. No devolver entidades EF desde controllers.
- Usar AutoMapper para mapear entidades a DTOs cuando corresponda.
- Registrar dependencias por DI en extensiones claras como `AddApplication` y `AddInfrastructure`.
- Mantener CORS preparado para un frontend Angular externo.

## Fotos y almacenamiento

- No guardar fotos en SQL Server como binario.
- No agregar propiedades `byte[]`, `varbinary(max)` ni equivalentes para imagenes.
- SQL Server solo guarda metadata: nombres, content type, tamanio, claves de R2, precio y relaciones.
- Las fotos reales se guardan en Cloudflare R2.
- Las descargas deben usar URLs temporales o firmadas.
- La integracion con Pexels vive solo en el backend y se usa para cargar fotos demo.
- No exponer la API Key de Pexels al frontend ni devolverla en responses.
- Las fotos de Pexels se guardan como metadata en `Fotos`; no descargar imagenes ni guardar binarios en SQL Server.
- No subir fotos reales a Pexels ni usar Pexels como storage productivo final.
- Cloudflare R2 o storage propio sigue siendo el destino real para produccion.

## Pagos y emails

- Mercado Pago se integra desde servicios, no desde controllers.
- Los webhooks deben validar el evento consultando a Mercado Pago antes de actualizar estados locales.
- Resend se usa desde `EmailService`.

## Seguridad y configuracion

- No commitear secretos, tokens, API keys ni connection strings reales.
- Usar `appsettings.example.json` como plantilla.
- Para desarrollo, preferir Secret Manager o variables de entorno.
- Revisar `.gitignore` antes de agregar archivos de configuracion locales.
- Configurar Pexels en desarrollo con Secret Manager:
  `dotnet user-secrets init --project .\Fotografia.Api\Fotografia.Api.csproj`
  `dotnet user-secrets set "Pexels:ApiKey" "TU_API_KEY" --project .\Fotografia.Api\Fotografia.Api.csproj`
  `dotnet user-secrets list --project .\Fotografia.Api\Fotografia.Api.csproj`
- Alternativamente usar variable de entorno `Pexels__ApiKey`.

## Convenciones de cambios

- Mantener namespaces bajo la capa correspondiente: `Fotografia.Api`, `Fotografia.Application`, `Fotografia.Domain` o `Fotografia.Infrastructure`.
- Evitar refactors no relacionados con la tarea.
- Agregar migraciones EF en `Fotografia.Infrastructure` solo cuando el cambio de modelo lo requiera.
- Antes de cerrar una tarea, ejecutar `dotnet build` y, si existen tests, `dotnet test`.
