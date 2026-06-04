# Roadmap

Este roadmap separa el estado actual de los pendientes necesarios para produccion. La idea es mostrar criterio: el proyecto tiene mucho implementado, pero todavia hay trabajo razonable antes de operarlo con usuarios reales.

## Implementado

### Arquitectura Y Base Tecnica

- Solucion por capas: Api, Application, Domain, Infrastructure.
- ASP.NET Core Web API con controllers.
- DTOs para contratos HTTP.
- AutoMapper.
- EF Core con SQL Server/Azure SQL.
- Migraciones EF Core.
- Swagger/OpenAPI.
- CORS para frontend Angular externo.
- `ApiResponse<T>`.
- Middleware global de errores.
- Correlation id.
- Health checks.
- Rate limiting.
- Options pattern y validacion de settings.
- GitHub Actions backend.

### Seguridad

- JWT Bearer.
- Roles `Admin` y `Usuario`.
- Invitado como acceso anonimo sin JWT.
- Ownership en backend.
- `ResourceAccessService`.
- Proteccion de `StorageKey` y `MarcaAguaStorageKey` en endpoints no admin.
- User-secrets/env vars para secretos.
- `.gitignore` para configuracion local.
- Logs seguros.

### Galeria Y Ecommerce

- Eventos con estado, visibilidad, slug, portada y fecha limite de compra.
- Fotos como metadata, sin binarios en SQL.
- Bulk metadata/storage keys.
- Comentarios en eventos y fotos.
- Favoritos de eventos y fotos.
- Paquetes por evento.
- Carrito con items mixtos.
- Cupones.
- Pedidos con `PedidoItems` y compatibilidad `PedidoFotos`.
- Pagos con Mercado Pago.
- Webhook Mercado Pago con consulta al proveedor.
- Descargas con vencimiento, limite de usos y regeneracion.

### Sesiones Privadas

- Sesiones privadas.
- Fotos privadas.
- Ownership estricto.
- Compra y descarga de fotos privadas.

### Web Publica Comercial

- Perfil fotografa.
- Sitio home/contacto.
- Portfolio.
- Servicios.
- FAQ.
- Promociones.
- Testimonios.

### Operacion Admin

- Dashboard.
- Operaciones/resumen.
- Ventas/resumen.
- Clientes e historial.
- Notas internas.
- Presupuestos.
- Agenda.
- Reportes.
- Notificaciones.
- Plantillas de notificacion.
- Carritos abandonados.
- Bitacora/auditoria.

### Calidad

- Tests unitarios.
- Tests de integracion.
- Sanitizacion de metadata de auditoria.
- Documentacion tecnica.

## Pendiente Antes De Produccion

### Infraestructura Y Deploy

- Dockerfile backend.
- Docker Compose local con SQL Server.
- Pipeline de deploy backend.
- Pipeline separado para frontend Angular.
- Ambientes Development, Staging y Production.
- Variables/secrets gestionados en plataforma.
- HTTPS y dominios reales.
- Configuracion CORS productiva estricta.

### Base De Datos

- Backups automatizados.
- Pruebas de restore.
- Politica de migraciones en deploy.
- Testcontainers SQL Server en CI.
- Revision de indices con datos reales.
- Politica de retencion de bitacora y logs.

### Seguridad

- Validacion de firma/secreto de webhooks Mercado Pago si aplica.
- Rotacion de JWT signing key.
- Evaluar refresh tokens o proveedor OIDC si el alcance crece.
- Hardening de headers HTTP.
- Revision completa de endpoints publicos/protegidos.
- Rate limiting por usuario/IP para endpoints criticos.
- Auditoria de exposicion de `StorageKey` en todos los DTOs.

### Observabilidad

- Logs estructurados centralizados.
- Dashboard de errores.
- Alertas para fallas de pagos, emails y storage.
- Metricas de negocio y tecnicas.
- Tracing distribuido si el sistema se separa en mas servicios.

### Testing

- Mas integration tests con auth/roles.
- Tests de webhooks Mercado Pago.
- Tests de descargas con storage mock.
- Tests de carrito y cupones end-to-end backend.
- Tests de notificaciones/worker.
- Tests de agenda y presupuestos.
- Coverage report en CI.
- E2E frontend en repo Angular.

### Storage

- Politicas productivas de buckets.
- CDN si corresponde.
- Estrategia de limpieza de archivos huerfanos.
- Procesamiento real de marca de agua.
- Pipeline de thumbnails/previews.

## Futuras Mejoras

### Producto

- Pantalla frontend para bitacora.
- Panel avanzado de reportes.
- Exportacion CSV/Excel de ventas.
- Segmentacion de clientes.
- Automatizaciones de marketing.
- Mas estados de workflow para sesiones/eventos.
- Integracion calendario externa.
- Facturacion o comprobantes si aplica al negocio.

### Arquitectura

- Query services para reportes complejos.
- Domain errors/codigos de error mas tipados si Angular lo necesita.
- Versionado de API.
- Cache para endpoints publicos de sitio/portfolio.
- Outbox pattern para notificaciones si se requiere consistencia transaccional.
- Background jobs dedicados para procesamiento de imagenes.
- Separacion por bounded contexts si el dominio crece mucho.

### Operacion

- Runbooks de incidentes.
- Politica de soporte y monitoreo.
- Alertas por pagos pendientes, webhooks fallidos y errores del worker.
- Retencion automatica de carritos abandonados.
- Auditoria exportable para Admin.

## Que No Tocaria Ahora

- No meteria un repository generico global.
- No moveria a microservicios prematuramente.
- No reemplazaria `ApiResponse<T>` sin versionar el contrato con Angular.
- No guardaria imagenes en SQL Server.
- No implementaria procesamiento pesado de imagenes dentro del request HTTP.
- No agregaria dependencias externas si el framework ya cubre bien el caso.

## Prioridad Recomendada

1. Docker Compose + Testcontainers SQL Server.
2. Mas tests de integracion para auth, carrito, pedidos, pagos y descargas.
3. Hardening de webhooks, CORS y secrets productivos.
4. Observabilidad centralizada.
5. Pantalla frontend de bitacora y reportes.
6. Procesamiento real de imagenes/marca de agua en background.
