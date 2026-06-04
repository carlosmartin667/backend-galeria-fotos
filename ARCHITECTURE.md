# Arquitectura

Este repositorio contiene solo el backend ASP.NET Core Web API. El frontend Angular vive en otro repositorio y consume esta API por endpoints REST.

## Capas

- `Fotografia.Api`: host ASP.NET Core, controllers, Swagger, CORS, autenticacion, rate limiting, health checks y middleware HTTP.
- `Fotografia.Application`: DTOs, respuestas comunes, interfaces de servicios, seguridad compartida y mapeos de AutoMapper.
- `Fotografia.Domain`: entidades y constantes del dominio.
- `Fotografia.Infrastructure`: EF Core, SQL Server/Azure SQL, servicios concretos, integraciones externas, JWT, seed y background workers.

## Patrones Usados

- Service Layer: la logica de negocio vive en servicios concretos de Infrastructure, expuestos por interfaces en Application.
- DTO pattern: los controllers reciben y devuelven DTOs, no entidades EF.
- Dependency Injection: controllers, services, workers e integraciones se resuelven por DI.
- Options pattern: settings de JWT, base de datos, proveedores externos, notificaciones y CORS se leen desde configuracion.
- BackgroundService: `NotificationWorkerService` procesa notificaciones pendientes.
- Unit of Work: `AppDbContext` coordina cambios y transacciones EF Core por request o scope.
- ResourceAccessService: centraliza reglas de ownership y visibilidad para eventos, fotos y paquetes.

## Bitacora Y Auditoria

- La entidad `Bitacora` vive en `Fotografia.Domain` y se persiste con EF Core en `Fotografia.Infrastructure`.
- `IBitacoraService` expone registro y consultas administrativas desde `Fotografia.Application`.
- `BitacoraService` captura usuario actual, rol, IP, user agent, correlation id, ruta, metodo HTTP, accion, entidad, severidad y metadata sanitizada.
- Los controllers no registran auditoria directamente; los hooks viven en services despues de operaciones confirmadas.
- La consulta es solo Admin mediante `GET /api/Bitacora`, `GET /api/Bitacora/{id}` y `GET /api/Bitacora/resumen`.
- Si falla el registro de auditoria, se loguea un warning seguro y no se rompe la operacion principal.

## Por Que No Hay Repository Generico

No se usa Repository generico porque EF Core ya provee un repositorio/unit-of-work suficientemente expresivo con `DbSet` y `DbContext`. Agregar uno generico ocultaria capacidades utiles de EF, duplicaria abstracciones y no mejoraria la mantenibilidad actual. Si en el futuro se repiten consultas complejas, conviene extraer query services o specifications puntuales.

## Reglas De Datos

- SQL Server guarda metadata de fotos, no binarios.
- Cloudflare R2 es el storage real.
- Las descargas usan URLs temporales o firmadas.
- Los deletes sensibles usan borrado logico cuando hay historial o relaciones.
- Las integraciones externas no se invocan desde controllers.

## Deuda Tecnica Controlada

- Algunos services son grandes y deberian dividirse solo despues de ampliar tests.
- `ApiResponse` se mantiene por compatibilidad con Angular.
- Swashbuckle se mantiene para Swagger UI; migrar a OpenAPI first-party puede evaluarse despues.
