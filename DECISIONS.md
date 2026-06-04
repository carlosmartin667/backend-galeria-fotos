# Decisiones Tecnicas

Este documento registra decisiones relevantes del proyecto. El formato es intencionalmente liviano para poder defenderlo en entrevista sin convertirlo en burocracia.

## .NET 10 / ASP.NET Core 10

**Decision:** usar .NET 10 y ASP.NET Core Web API.

**Contexto:** el proyecto busca mostrar un backend moderno, mantenible y alineado con practicas actuales.

**Alternativas consideradas:** .NET 8 LTS, .NET 9, Node.js/NestJS.

**Motivo:** .NET 10 permite demostrar uso de stack actual, buen rendimiento, DI nativo, middleware, health checks, rate limiting, JWT y ecosistema maduro para APIs.

**Consecuencias:** requiere SDK/runtime actual en ambientes de desarrollo y CI.

## Angular En Repositorio Separado

**Decision:** mantener frontend Angular fuera de este repositorio.

**Contexto:** backend y frontend evolucionan con ciclos, tooling y despliegues distintos.

**Alternativas consideradas:** monorepo fullstack.

**Motivo:** separa responsabilidades, evita mezclar assets frontend con backend y mantiene la API como contrato REST independiente.

**Consecuencias:** se necesita configurar CORS y documentar la URL base para Angular.

## SQL Server / Azure SQL

**Decision:** usar SQL Server o Azure SQL para datos relacionales.

**Contexto:** el dominio tiene clientes, pedidos, pagos, descargas, carrito, agenda, auditoria y relaciones fuertes.

**Alternativas consideradas:** PostgreSQL, MySQL, NoSQL.

**Motivo:** SQL Server encaja con EF Core, transacciones, integridad referencial y despliegue natural en ecosistema Microsoft/Azure.

**Consecuencias:** conviene sumar Testcontainers SQL Server para validar migraciones en CI.

## Entity Framework Core

**Decision:** usar EF Core como ORM y unit of work.

**Contexto:** el backend necesita productividad, migraciones y consultas relacionales.

**Alternativas consideradas:** Dapper, ADO.NET, repository manual.

**Motivo:** EF Core ofrece tracking, LINQ, migrations, relaciones y unit of work integrado.

**Consecuencias:** hay que cuidar includes, tracking, paginacion y performance de queries.

## Arquitectura Por Capas

**Decision:** dividir en `Fotografia.Api`, `Fotografia.Application`, `Fotografia.Domain` y `Fotografia.Infrastructure`.

**Contexto:** el sistema tiene muchos modulos y reglas de negocio.

**Alternativas consideradas:** API monolitica en un solo proyecto.

**Motivo:** mejora separacion de responsabilidades, testabilidad y lectura del codigo.

**Consecuencias:** hay mas proyectos y referencias, pero el orden de dependencias queda claro.

## Controllers

**Decision:** usar controllers en lugar de Minimal APIs.

**Contexto:** la API tiene muchos recursos, roles, endpoints y DTOs.

**Alternativas consideradas:** Minimal APIs.

**Motivo:** controllers agrupan por recurso, soportan atributos de autorizacion y son conocidos en equipos .NET.

**Consecuencias:** algo mas de boilerplate, compensado por estructura.

## Service Layer

**Decision:** poner reglas de negocio en services.

**Contexto:** controllers no deben tener logica pesada ni acceso directo a proveedores.

**Alternativas consideradas:** logica en controllers, handlers CQRS completos.

**Motivo:** services son suficientes para el alcance actual y evitan sobreingenieria.

**Consecuencias:** algunos services crecieron y podrian dividirse despues de ampliar tests.

## DTO Pattern Y AutoMapper

**Decision:** usar DTOs para contratos HTTP y AutoMapper cuando corresponde.

**Contexto:** no se deben exponer entidades EF directamente.

**Alternativas consideradas:** devolver entidades, mapeo manual en todos lados.

**Motivo:** DTOs protegen contratos y AutoMapper reduce repeticion en mapeos simples.

**Consecuencias:** hay que mantener DTOs alineados con contratos Angular.

## ApiResponse

**Decision:** mantener `ApiResponse<T>` como envelope de respuesta.

**Contexto:** el frontend consume una forma uniforme con `success`, `message`, `data`, `errors` y `statusCode`.

**Alternativas consideradas:** ProblemDetails puro, resultados HTTP sin envelope.

**Motivo:** consistencia para Angular y compatibilidad con contratos existentes.

**Consecuencias:** convive con middleware global de errores; no se debe romper el contrato sin versionar API.

## Swagger / OpenAPI

**Decision:** usar Swagger/OpenAPI con Swashbuckle.

**Contexto:** la demo y frontend necesitan explorar contrato HTTP.

**Alternativas consideradas:** Microsoft.AspNetCore.OpenApi first-party.

**Motivo:** Swashbuckle sigue siendo practico por Swagger UI y soporte conocido.

**Consecuencias:** se puede evaluar migracion futura si aporta valor.

## JWT

**Decision:** usar JWT Bearer con roles.

**Contexto:** frontend Angular externo consume API REST.

**Alternativas consideradas:** cookies, ASP.NET Core Identity completo, OAuth/OIDC externo.

**Motivo:** JWT es simple para SPA/API y suficiente para demo/alcance actual.

**Consecuencias:** en produccion conviene revisar rotacion de keys, expiracion, refresh tokens y proveedor de identidad.

## Roles Admin, Usuario E Invitado

**Decision:** Admin y Usuario son roles autenticados; Invitado es acceso anonimo.

**Contexto:** el publico debe navegar eventos/fotos sin login.

**Alternativas consideradas:** crear usuario Invitado en base.

**Motivo:** no hace falta un token ni usuario para acceso publico.

**Consecuencias:** endpoints publicos deben usar `[AllowAnonymous]` y no claims de Invitado.

## ResourceAccessService

**Decision:** centralizar reglas reutilizables de ownership/visibilidad.

**Contexto:** eventos, fotos y paquetes se consultan desde varios flujos.

**Alternativas consideradas:** repetir reglas en cada service.

**Motivo:** reduce inconsistencias y facilita tests.

**Consecuencias:** no todas las reglas estan ahi; algunas quedan en services especificos por contexto.

## Cloudflare R2

**Decision:** guardar fotos reales en Cloudflare R2.

**Contexto:** SQL Server no debe almacenar imagenes binarias.

**Alternativas consideradas:** guardar binarios en SQL, filesystem local, Azure Blob Storage.

**Motivo:** R2 es storage de objetos adecuado para imagenes, URLs firmadas y separacion de metadata/archivos.

**Consecuencias:** se necesita configurar secrets y politicas de acceso por ambiente.

## No Guardar Binarios En SQL

**Decision:** SQL Server solo guarda metadata de fotos.

**Contexto:** imagenes pueden ser grandes y muchas.

**Alternativas consideradas:** `varbinary(max)`.

**Motivo:** performance, backups, costos y separacion correcta de responsabilidades.

**Consecuencias:** la consistencia entre DB y storage debe cuidarse en procesos operativos.

## Mercado Pago Checkout Pro

**Decision:** integrar Mercado Pago Checkout Pro mediante service.

**Contexto:** se necesitan pagos online.

**Alternativas consideradas:** pagos manuales, Stripe, transferencia.

**Motivo:** Mercado Pago es relevante para el mercado local y Checkout Pro reduce alcance PCI.

**Consecuencias:** webhooks deben validarse consultando al proveedor y no guardar tokens/logs sensibles.

## Resend

**Decision:** usar Resend para emails.

**Contexto:** el sistema necesita notificaciones.

**Alternativas consideradas:** SMTP directo, SendGrid, Mailgun.

**Motivo:** API simple y buen encaje con `EmailService`.

**Consecuencias:** requiere API key segura y manejo de fallos/reintentos.

## Pexels Demo

**Decision:** usar Pexels solo para importar metadata demo.

**Contexto:** se necesitan datos visuales de prueba sin subir archivos reales.

**Alternativas consideradas:** assets locales, subir fotos reales, datos manuales.

**Motivo:** permite poblar metadata de galerias sin guardar binarios.

**Consecuencias:** no reemplaza storage productivo. La API key no debe exponerse.

## Tests Unitarios E Integracion

**Decision:** crear proyectos separados `Fotografia.Tests.Unit` y `Fotografia.Tests.Integration`.

**Contexto:** se necesita demostrar calidad sin depender siempre de infraestructura real.

**Alternativas consideradas:** solo manual testing, solo integration tests.

**Motivo:** separa velocidad de profundidad y permite CI basico.

**Consecuencias:** falta ampliar coverage y sumar SQL Server real con Testcontainers.

## Bitacora

**Decision:** implementar auditoria propia con `BitacoraService`.

**Contexto:** Admin necesita trazabilidad de acciones sensibles.

**Alternativas consideradas:** solo logs, tabla generica de eventos sin metadata, proveedor externo de audit.

**Motivo:** bitacora en DB permite consulta administrativa y relacion con usuarios/entidades.

**Consecuencias:** se debe sanitizar metadata y definir retencion antes de produccion.

## No Guardar Secretos En Repo

**Decision:** usar placeholders, user-secrets, variables de entorno y archivos locales ignorados.

**Contexto:** el repo puede compartirse o revisarse en entrevista.

**Alternativas consideradas:** appsettings con valores reales.

**Motivo:** reduce riesgo de exposicion accidental.

**Consecuencias:** setup local requiere pasos documentados.
