# Testing

El objetivo de los tests actuales es demostrar una base tecnica defendible: reglas de negocio aisladas, helpers criticos, pipeline ASP.NET Core, autorizacion basica y health checks. La suite todavia puede crecer, pero ya separa unit tests de integration tests.

## Como Ejecutar

Todo el repo:

```powershell
dotnet restore .\fotografia-backend-api.slnx
dotnet build .\fotografia-backend-api.slnx
dotnet test .\fotografia-backend-api.slnx
```

Solo unit tests:

```powershell
dotnet test .\Fotografia.Tests.Unit\Fotografia.Tests.Unit.csproj
```

Solo integration tests:

```powershell
dotnet test .\Fotografia.Tests.Integration\Fotografia.Tests.Integration.csproj
```

## Proyectos

### Fotografia.Tests.Unit

Pruebas rapidas sin levantar el host completo.

Cubre:

- helpers;
- `ApiResponse`;
- reglas de servicios;
- ownership/visibilidad;
- sanitizacion de metadata de auditoria;
- permisos del `BitacoraService`.

### Fotografia.Tests.Integration

Pruebas del pipeline ASP.NET Core con `WebApplicationFactory`.

Cubre:

- endpoints publicos;
- endpoints protegidos sin token;
- autenticacion JWT de prueba;
- autorizacion Admin/Usuario;
- health checks;
- endpoint de bitacora solo Admin.

## Herramientas

- xUnit: framework de pruebas.
- FluentAssertions: assertions legibles.
- NSubstitute: mocks/stubs para interfaces.
- WebApplicationFactory: host de integracion ASP.NET Core.
- EF Core InMemory: base aislada para tests iniciales.
- coverlet.collector: base para medir cobertura.

Moq no es necesario actualmente porque el proyecto ya usa NSubstitute.

## EF InMemory Vs SQLite/Testcontainers

Actualmente se usa EF Core InMemory para tests rapidos y aislados.

Ventajas:

- no requiere SQL Server local;
- corre rapido en CI;
- simple para validar servicios y pipeline;
- bueno para primeros tests de contrato.

Limitaciones:

- no reproduce todas las restricciones relacionales;
- no valida SQL real;
- no valida migraciones;
- no reproduce diferencias de collation, constraints o transacciones SQL Server.

Siguiente paso recomendado:

- mantener InMemory para tests rapidos;
- agregar Testcontainers con SQL Server para integracion critica de EF/migraciones;
- opcionalmente usar SQLite in-memory solo para algunos casos relacionales, entendiendo que no equivale a SQL Server.

## Tests Existentes

Unitarios:

- `ApiResponseTests`: shape basico de respuestas comunes.
- `CuponServiceTests`: reglas de cupones, vencimiento, inactividad, porcentaje invalido y descuentos.
- `ResourceAccessServiceTests`: ownership/visibilidad de recursos.
- `AuditMetadataSanitizerTests`: redaccion de keys sensibles, storage keys y URLs firmadas.
- `BitacoraServiceTests`: persistencia segura de auditoria y restriccion Admin.

Integracion:

- `BasicApiTests`: endpoint publico de eventos, endpoint admin protegido y health checks.
- `BitacoraApiTests`: `401` sin token, `403` para Usuario no admin y `200` para Admin en bitacora.

## Como Agregar Nuevos Tests

1. Elegir nivel:
   - Unit test si se prueba una regla aislada.
   - Integration test si se prueba auth, routing, filtros, middleware o serialization.
2. Evitar tocar base real.
3. Usar datos con IDs deterministas solo dentro del test.
4. No usar secrets reales.
5. Para services, mockear interfaces externas como `IStorageService`, `IEmailService` o `IMercadoPagoService` cuando aplique.
6. Para endpoints, usar `WebApplicationFactory` y JWT de prueba.
7. Validar status code y shape de `ApiResponse<T>`.

## Tests Recomendados Para Ampliar Cobertura

Alta prioridad:

- `DescargaService`: limite de descargas, vencimiento, ownership, pedido no pagado y paquete comprado.
- `PedidoService`: creacion desde carrito, PedidoItems, fallback a PedidoFotos y cambio de estado.
- `CarritoService`: items mixtos, duplicados, cupon aplicado y crear pedido.
- `MercadoPagoService`: webhook aprobado, pendiente, rechazado e idempotencia.
- `ResourceAccessService`: escenarios completos Admin/Usuario/Invitado.
- `Auth`: register siempre crea rol Usuario y login genera claims correctos.
- Endpoints admin protegidos: dashboard, reportes, bitacora, notificaciones.

Media prioridad:

- `NotificacionService`: encolar desde plantilla, reenviar, cancelar y worker.
- `SolicitudPresupuestoService`: alta publica y cambios admin.
- `AgendaService`: disponibilidad y conflictos.
- `TestimonioService`: publicar/ocultar y acceso publico.
- `PexelsService`: API key faltante, header correcto y no descarga de imagenes.
- `CloudflareR2StorageService`: usar fake/local mock para no pegarle a R2 real.

Produccion/CI:

- Testcontainers SQL Server para migraciones.
- Tests de Swagger/OpenAPI si el contrato es parte del entregable.
- Cobertura minima cuando la suite sea mas grande.
- E2E frontend en el repositorio Angular.

## Cobertura

El proyecto ya referencia `coverlet.collector`. Para generar cobertura:

```powershell
dotnet test .\fotografia-backend-api.slnx --collect:"XPlat Code Coverage"
```

Cuando haya una suite mas amplia, conviene definir umbrales por capas criticas, no perseguir 100% de cobertura superficial.

## Que No Probar Con Unit Tests

- Integraciones reales con Mercado Pago, Resend, Pexels o Cloudflare R2.
- SQL Server real con EF InMemory.
- Comportamiento completo del navegador Angular.
- Secrets o credenciales reales.

Esos casos deben cubrirse con integration tests controlados, mocks, ambiente staging o E2E en el repo correspondiente.
