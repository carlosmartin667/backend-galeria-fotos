# Guia Para Entrevista Tecnica

Esta guia ayuda a defender el proyecto ante una revision tecnica de arquitectura, calidad de codigo, seguridad, testing y preparacion para produccion.

## Resumen De 60 Segundos

Este proyecto es un backend .NET 10 para una plataforma de galeria fotografica profesional. La API permite gestionar eventos, fotos, clientes, sesiones privadas, carrito, cupones, pedidos, pagos con Mercado Pago, descargas protegidas con Cloudflare R2, presupuestos, agenda, notificaciones, promociones, testimonios, reportes y bitacora. Esta dividido en capas `Api`, `Application`, `Domain` e `Infrastructure`, usa EF Core con SQL Server, JWT con roles, DTOs, services, ownership, health checks, rate limiting, correlation id, tests unitarios/integracion y CI.

## Explicacion Tecnica De 3 Minutos

El backend esta hecho con ASP.NET Core Web API usando controllers porque el proyecto tiene muchos modulos y conviene mantener agrupacion por recursos. Los controllers son delgados: reciben DTOs, llaman interfaces de services y devuelven `ApiResponse<T>` mapeado a HTTP.

La logica de negocio vive en services de Infrastructure, expuestos por interfaces en Application. EF Core queda encapsulado en Infrastructure mediante `AppDbContext`, que actua como unit of work. Domain contiene entidades y constantes del negocio. Application contiene DTOs, helpers, mapeos, respuestas comunes e interfaces.

La seguridad se basa en JWT y roles. Admin puede administrar todo, Usuario accede a recursos propios, e Invitado es anonimo con endpoints publicos `[AllowAnonymous]`. Las reglas de ownership se validan en services, no en Angular. Las fotos reales se almacenan en Cloudflare R2; SQL Server solo guarda metadata. Las descargas usan URLs temporales o firmadas y validan pedido pagado, ownership y limite de usos.

El sistema integra Mercado Pago, Resend, Pexels demo y R2 desde services. Tambien tiene health checks, rate limiting, correlation id, middleware global de errores, bitacora con metadata sanitizada y tests unitarios/integracion.

## Arquitectura

Puntos para explicar:

- Separacion por capas.
- Controllers delgados.
- Services por dominio.
- EF Core en Infrastructure.
- DTOs para contratos HTTP.
- `ApiResponse<T>` por compatibilidad con frontend Angular.
- `ResourceAccessService` para ownership.
- `BitacoraService` para auditoria.
- Worker de notificaciones para trabajo en background.

Frase util:

> No busque una arquitectura pura por dogma; busque una arquitectura clara para un backend de negocio con muchos modulos, donde las reglas estan en services, los contratos estan en DTOs y EF Core no se filtra a controllers.

## Seguridad

Puntos para explicar:

- JWT Bearer con roles.
- Registro publico no permite crear Admin.
- Invitado no tiene token ni usuario en base.
- Ownership en backend.
- `StorageKey` y `MarcaAguaStorageKey` no se exponen a usuarios no admin.
- Descargas por URL temporal/firmada.
- Secrets por user-secrets/env vars.
- Rate limiting en endpoints sensibles.
- Logs y bitacora sanitizados.

Frase util:

> El frontend mejora la experiencia, pero no es una frontera de seguridad. Todas las reglas importantes se vuelven a validar en backend.

## Testing

Puntos para explicar:

- xUnit como framework.
- FluentAssertions para legibilidad.
- NSubstitute para mocks.
- WebApplicationFactory para pipeline real ASP.NET Core.
- EF InMemory para tests rapidos iniciales.
- coverlet listo para cobertura.
- Separacion Unit/Integration.

Limitacion honesta:

> EF InMemory es util para velocidad, pero no valida migraciones ni SQL real. Para produccion agregaria Testcontainers con SQL Server en CI.

## Auditoria

Puntos para explicar:

- `Bitacora` registra acciones relevantes.
- Los hooks estan en services despues de operaciones confirmadas.
- Captura usuario, rol, accion, entidad, correlation id, IP, ruta y metadata.
- `AuditMetadataSanitizer` redacta datos sensibles.
- Endpoints solo Admin.
- Si falla auditoria, no rompe la operacion principal.

Frase util:

> La bitacora tiene valor operativo, no forense absoluto. Por eso prioriza trazabilidad segura y no guarda payloads sensibles.

## Decisiones Tecnicas

Decisiones defendibles:

- .NET 10 por stack moderno.
- Controllers por organizacion de API extensa.
- EF Core sin repository generico.
- SQL Server para datos relacionales.
- R2 para archivos.
- DTOs y AutoMapper para contratos.
- `ApiResponse<T>` para frontend consistente.
- JWT simple en lugar de Identity completo, por alcance del proyecto.
- Mercado Pago/Resend aislados en services.
- BackgroundService para notificaciones.
- Bitacora propia por necesidades del negocio.

## Preguntas Dificiles Y Respuestas

### Por que controllers y no Minimal APIs?

Porque la API tiene muchos recursos, permisos, DTOs y grupos funcionales. Controllers facilitan organizar endpoints por dominio, aplicar atributos de autorizacion, documentar Swagger y mantener convenciones conocidas para un equipo .NET. Minimal APIs podrian servir para microservicios o APIs pequenas, pero aca controllers dan estructura.

### Por que no Repository generico?

Porque EF Core ya implementa patrones de repository/unit of work mediante `DbSet` y `DbContext`. Un repository generico ocultaria includes, tracking, proyecciones y optimizaciones por caso. En lugar de eso, las consultas viven cerca del service que contiene la regla de negocio. Si una query se repite, extraeria un query service especifico, no una abstraccion generica.

### Como proteges ownership?

Con validaciones en backend. Los services verifican `currentUser.UserId`, rol y relaciones del recurso. Por ejemplo, pedidos se filtran por cliente asociado al usuario, fotos privadas por cliente/sesion privada y descargas por pedido pagado y propiedad. Admin puede todo; Usuario solo lo propio; Invitado solo publicos.

### Como manejas errores?

Los services devuelven `ApiResponse<T>` con status semantico: 400, 401, 403, 404, 409, 502/503 o 500 segun el caso. Los controllers transforman eso en HTTP. El middleware global captura excepciones no controladas, loguea con correlation id y devuelve mensaje seguro al frontend.

### Como evitas filtrar StorageKey?

SQL guarda `StorageKey` como metadata interna, pero los DTOs publicos/no admin deben sanitizarlo o no incluirlo. Para mostrar imagenes se usa `PreviewUrl`; para descargar originales se genera URL temporal/firmada desde el service despues de validar permisos. No se loguean ni auditan storage keys.

### Como funcionan las descargas?

El usuario pide un link para una foto. El service valida que el pedido este pagado, que el usuario sea owner, que la foto este incluida en el pedido o paquete, y que la descarga no este vencida/bloqueada. Luego pide al storage una URL temporal. La tabla `Descarga` mantiene historial, vencimiento, usos y regeneraciones.

### Como funciona la bitacora?

Los services llaman `IBitacoraService` despues de acciones importantes. El service captura contexto HTTP y usuario actual, sanitiza metadata y guarda en SQL. La consulta es solo Admin. Si el registro falla, se loguea warning y no se rompe la operacion principal.

### Como probarias mas?

Agregaria tests de integracion con Testcontainers SQL Server para migraciones y constraints; tests de DescargaService, PedidoService, CarritoService, webhooks de Mercado Pago y permisos por rol; E2E en el repo Angular; y cobertura en CI cuando la suite sea mas amplia.

### Que mejorarias para produccion?

Docker Compose o IaC, ambientes staging/production, secretos gestionados, observabilidad avanzada, metricas, backups, retencion de logs/bitacora, validacion de firma de webhooks, hardening CORS, Testcontainers en CI y monitoreo de storage/proveedores.

### Como escalarias el sistema?

Separaria almacenamiento de imagenes en R2/CDN, mantendria SQL para datos transaccionales, agregaria cache donde haya lecturas publicas frecuentes, moveria procesos pesados a workers/colas, optimizaria queries/reportes, agregaria paginacion estricta y monitoreo. Si el dominio creciera mucho, separaria bounded contexts, no microservicios prematuros.

### Como manejas integraciones externas caidas?

Los proveedores se llaman desde services. Se devuelven errores de dependencia externa seguros, se loguean IDs/estado sin secretos y se evita romper datos locales. Para emails, el worker puede reintentar. Para pagos, el webhook puede reintentarse y el estado local se actualiza solo tras consulta al proveedor.

### Que pasa si alguien llama un endpoint publico sin token?

Si el endpoint esta marcado `[AllowAnonymous]`, funciona. Si es protegido, devuelve 401. Si tiene token valido pero no tiene rol/ownership, devuelve 403.

### Que decision cambiarias si tuvieras mas tiempo?

Agregaria Testcontainers SQL Server y mas integration tests antes de refactorizar services grandes. Tambien evaluaria versionado de API y una capa de errores de dominio mas tipada si el frontend empieza a depender de codigos de error mas granulares.

## Cierre Para Entrevista

Mensaje final:

> El proyecto muestra criterio pragmatico: no intenta ser una arquitectura perfecta, sino un backend mantenible, seguro y testeable para un negocio real. Tiene capas claras, integraciones aisladas, ownership en backend, storage correcto para imagenes, auditoria, health checks, tests y un roadmap realista para produccion.
