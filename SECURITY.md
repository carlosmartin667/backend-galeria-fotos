# Seguridad

## Roles Y Acceso

- `Admin`: acceso administrativo completo.
- `Usuario`: acceso a sus propios datos, pedidos, descargas, favoritos, comentarios y sesiones privadas.
- Invitado: acceso anonimo solo a endpoints publicos marcados con `[AllowAnonymous]`.

El rol conceptual Invitado no requiere login ni JWT.

## Ownership

Las reglas de ownership se aplican desde servicios. `ResourceAccessService` centraliza acceso a eventos, fotos y paquetes segun visibilidad, estado, usuario creador y cliente principal.

## Fotos Y Storage

- No se guardan imagenes binarias en SQL Server.
- `StorageKey` y `MarcaAguaStorageKey` no deben exponerse a usuarios no administradores cuando permitan acceder al original.
- Los endpoints publicos deben devolver `PreviewUrl` o DTOs sanitizados.
- Las descargas usan URLs temporales o firmadas desde Cloudflare R2.
- No se loguean URLs firmadas ni tokens.

## Secrets

No commitear API keys, tokens, connection strings productivas ni secretos JWT.

Usar:

```powershell
dotnet user-secrets set "Jwt:SigningKey" "TU_CLAVE_LOCAL_DE_32_CARACTERES_O_MAS" --project .\Fotografia.Api\Fotografia.Api.csproj
dotnet user-secrets set "Pexels:ApiKey" "TU_API_KEY" --project .\Fotografia.Api\Fotografia.Api.csproj
```

O variables de entorno:

```powershell
$env:Jwt__SigningKey="TU_CLAVE_LOCAL_DE_32_CARACTERES_O_MAS"
$env:Pexels__ApiKey="TU_API_KEY"
```

## Rate Limiting

Los endpoints publicos sensibles usan la politica `SensitivePublic`, configurable por ambiente. Se devuelve 429 sin exponer informacion sensible.

## Logs Seguros

- Loguear IDs internos, estados y nombres de operaciones.
- No loguear API keys, JWT, URLs firmadas, passwords ni payloads completos de proveedores externos.
- Los errores 500 devuelven mensaje seguro e incluyen `traceId` y `correlationId`.

## Bitacora Segura

- La bitacora puede guardar `UsuarioId`, `UsuarioEmail`, rol, accion, entidad, IP, user agent, correlation id y metadata operativa.
- No guardar passwords, JWT, API keys, tokens, URLs firmadas, `StorageKey`, `MarcaAguaStorageKey`, payloads crudos de proveedores ni bodies completos.
- `AuditMetadataSanitizer` redacta claves sensibles y URLs con firmas o tokens antes de persistir `MetadataJson`.
- Los endpoints de bitacora son solo Admin. Un usuario sin token debe recibir 401 y un Usuario no admin debe recibir 403.
- El registro de auditoria no debe bloquear pagos, pedidos, descargas, notificaciones ni operaciones principales si falla.

## CORS

CORS se limita a los origenes configurados en `Cors:AllowedOrigins`. En produccion debe apuntar solo al dominio real del frontend.
