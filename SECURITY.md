# Seguridad

Este documento resume las decisiones y controles de seguridad actuales del backend. No reemplaza una auditoria formal, pero deja claro que el proyecto evita los riesgos mas importantes para una galeria fotografica con ventas, descargas y datos de clientes.

## Principios

- No guardar secretos en el repositorio.
- No guardar imagenes binarias en SQL Server.
- No exponer originals ni `StorageKey` a usuarios no administradores.
- Validar ownership en services, no solo en frontend.
- Mantener endpoints publicos explicitamente anonimos.
- Registrar auditoria sin guardar datos sensibles.
- Usar errores claros para frontend y seguros para produccion.

## Roles Y Permisos

- `Admin`: administra todo el sistema.
- `Usuario`: accede a sus propios clientes, pedidos, pagos, descargas, favoritos, comentarios, notificaciones y sesiones privadas.
- Invitado: acceso anonimo, sin usuario en base, sin JWT y sin claims.

El invitado no usa `[Authorize(Roles = "Invitado")]`. Los endpoints publicos usan `[AllowAnonymous]`.

Ejemplos publicos:

- `GET /api/Eventos`
- `GET /api/Eventos/{id}`
- `GET /api/Fotos/evento/{eventoId}`
- `GET /api/Fotos/{id}`
- `GET /api/Sitio/home`
- `GET /api/Sitio/perfil-fotografa`
- `GET /api/Portfolio`
- `GET /api/Servicios`
- `GET /api/Faq`
- `GET /api/Testimonios`
- `GET /api/Promociones`
- `GET /api/Agenda/disponibilidad`

Ejemplos protegidos:

- `GET /api/Admin/dashboard`: Admin.
- `GET /api/Bitacora`: Admin.
- `GET /api/Carrito`: Usuario/Admin.
- `GET /api/Descargas/mis-descargas`: Usuario/Admin.
- `GET /api/SesionesPrivadas`: Usuario/Admin con ownership.

## Ownership

Las reglas de ownership viven en services e Infrastructure, no en Angular. Un token valido no alcanza para acceder a recursos ajenos.

Casos protegidos:

- un Usuario no puede ver pedidos de otro cliente;
- un Usuario no puede descargar fotos privadas ajenas;
- un Usuario no puede comprar fotos privadas ajenas;
- un Usuario no puede consultar sesiones privadas ajenas;
- favoritos y comentarios se asocian al usuario autenticado;
- Admin puede acceder a todos los recursos operativos.

`ResourceAccessService` centraliza reglas reutilizables de acceso a eventos, fotos y paquetes segun estado, visibilidad, usuario creador y cliente principal.

## JWT

La API usa JWT Bearer:

- issuer y audience configurables;
- signing key por user-secrets o variable de entorno;
- expiracion configurable;
- roles en claims;
- middleware ASP.NET Core de authentication/authorization.

No guardar `Jwt:SigningKey` real en archivos versionados.

```powershell
dotnet user-secrets set "Jwt:SigningKey" "TU_CLAVE_LOCAL_DE_32_CARACTERES_O_MAS" --project .\Fotografia.Api\Fotografia.Api.csproj
```

## Storage Y Fotos

Reglas:

- SQL Server guarda metadata, no binarios.
- Cloudflare R2 guarda archivos reales.
- `StorageKey` representa la ubicacion interna del original.
- `MarcaAguaStorageKey` representa una version protegida/marcada cuando aplica.
- DTOs publicos o no admin deben evitar exponer claves internas si permiten acceder al original.
- `PreviewUrl` puede usarse para mostrar imagenes sin revelar storage interno.

Datos que no deben exponerse:

- `StorageKey` original;
- `MarcaAguaStorageKey` si revela rutas privadas;
- URLs firmadas;
- keys de R2;
- nombres internos de buckets;
- credenciales de proveedores.

## Descargas

Las descargas se protegen con:

- verificacion de pedido pagado;
- ownership del cliente/usuario;
- validacion de foto individual comprada;
- validacion de foto incluida por paquete comprado;
- validacion de foto privada comprada;
- URLs temporales o firmadas;
- vencimiento (`ExpiraEnUtc`);
- contador de usos;
- limite opcional (`MaxDescargas`);
- regeneracion que mantiene historial y desactiva descarga anterior.

No se debe usar cascade delete desde descargas hacia fotos, pedidos o pagos: el historial importa.

## Mercado Pago

El webhook debe validar el pago consultando a Mercado Pago antes de actualizar estados locales. La bitacora registra resultado local, pedido, pago, monto y estado, pero no guarda access tokens ni payload crudo del proveedor.

Pendiente recomendado antes de produccion:

- verificar firma/secreto del webhook si el proveedor y configuracion lo permiten;
- registrar eventos rechazados con datos minimos;
- definir estrategia de idempotencia adicional para webhooks repetidos.

## Resend

Resend se usa desde `EmailService`. La API key debe configurarse por user-secrets o variable de entorno.

```powershell
dotnet user-secrets set "Resend:ApiKey" "TU_RESEND_API_KEY" --project .\Fotografia.Api\Fotografia.Api.csproj
```

No guardar cuerpos de emails con datos sensibles en logs. Los errores del worker deben guardarse sanitizados.

## Pexels

Pexels se usa para cargar fotos demo como metadata. No se descargan imagenes fisicamente y no se guardan binarios en SQL.

La API key se configura con:

```powershell
dotnet user-secrets set "Pexels:ApiKey" "TU_API_KEY" --project .\Fotografia.Api\Fotografia.Api.csproj
```

El header correcto es `Authorization: API_KEY`, no `Bearer`.

## Secrets Y Configuracion

No commitear:

- API keys;
- JWT signing keys;
- tokens de Mercado Pago;
- API keys de Resend;
- credenciales R2;
- connection strings productivas;
- URLs firmadas;
- archivos `appsettings.Local.json`.

Opciones seguras:

- user-secrets;
- variables de entorno;
- `appsettings.Local.json` solo local e ignorado por Git;
- valores placeholder en `appsettings.example.json`.

## Rate Limiting

Los endpoints publicos sensibles usan la politica `SensitivePublic`. El objetivo es reducir abuso en login, registro y otros flujos anonimos.

Respuesta esperada ante exceso:

```http
429 Too Many Requests
```

El mensaje no debe filtrar informacion interna.

## Health Checks

Endpoints:

- `GET /health`
- `GET /health/live`
- `GET /health/ready`

`/health/ready` valida base de datos. Los health checks no exponen secretos, connection strings ni informacion sensible de infraestructura.

## Correlation Id

El middleware de correlation id permite rastrear una solicitud entre:

- logs;
- errores globales;
- bitacora;
- respuesta al frontend.

El frontend puede enviar o recibir `X-Correlation-ID` segun el flujo configurado.

## Logs Seguros

Permitido:

- IDs internos;
- nombres de operaciones;
- estados;
- codigos HTTP;
- provider name;
- correlation id;
- error resumido/sanitizado.

No permitido:

- passwords;
- JWT;
- API keys;
- bearer tokens;
- URLs firmadas;
- `StorageKey`;
- `MarcaAguaStorageKey`;
- payload crudo de proveedores;
- datos de tarjetas o pagos sensibles;
- cuerpos completos de requests.

## Bitacora Y Auditoria

La tabla `Bitacora` registra actividad administrativa y de negocio:

- login exitoso/fallido;
- registro de usuario;
- pedidos creados y cambios de estado;
- pagos aprobados/rechazados;
- descargas generadas/regeneradas;
- eventos creados/publicados/cambios de visibilidad;
- sesiones privadas;
- presupuestos;
- cupones;
- promociones;
- testimonios;
- notificaciones.

`AuditMetadataSanitizer` redacta metadata sensible antes de persistir `MetadataJson`.

Endpoints solo Admin:

```http
GET /api/Bitacora
GET /api/Bitacora/{id}
GET /api/Bitacora/resumen
```

Si falla el registro de auditoria, se loguea un warning seguro y no se bloquea la operacion principal.

## CORS

CORS se configura con `Cors:AllowedOrigins`.

En Development puede incluir:

```text
http://localhost:4200
```

En produccion debe limitarse al dominio real del frontend. No usar `AllowAnyOrigin` con credenciales.

## Datos Que Nunca Se Guardan

- Imagenes binarias en SQL Server.
- Passwords en texto plano.
- API keys reales en Git.
- JWT reales.
- Tokens de Mercado Pago.
- Credenciales Cloudflare R2.
- URLs firmadas.
- Datos de tarjeta.
- Payloads crudos de proveedores.

## Pendientes Antes De Produccion

- Configurar secrets en plataforma segura.
- Revisar CORS con dominio real.
- Validar firma de webhooks de Mercado Pago si aplica.
- Usar HTTPS extremo a extremo.
- Configurar backups y restore drills.
- Agregar monitoreo y alertas.
- Agregar Testcontainers/SQL Server en CI para migraciones.
- Revisar politicas de retencion de bitacora y logs.
- Definir estrategia de rotacion de keys.
- Hacer prueba de permisos completa para endpoints admin/usuario/invitado.
