# Testing

## Como Ejecutar

```powershell
dotnet restore .\fotografia-backend-api.slnx
dotnet build .\fotografia-backend-api.slnx
dotnet test .\fotografia-backend-api.slnx
```

Tambien se puede ejecutar por proyecto:

```powershell
dotnet test .\Fotografia.Tests.Unit\Fotografia.Tests.Unit.csproj
dotnet test .\Fotografia.Tests.Integration\Fotografia.Tests.Integration.csproj
```

## Proyectos

- `Fotografia.Tests.Unit`: pruebas rapidas de servicios, helpers y reglas de negocio aisladas.
- `Fotografia.Tests.Integration`: pruebas del pipeline ASP.NET Core con `WebApplicationFactory`.

## Stack

- xUnit.
- FluentAssertions.
- NSubstitute.
- coverlet.collector.
- WebApplicationFactory.
- EF Core InMemory para la primera base de integracion, evitando SQL Server real en CI.

## Estrategia Actual

Los unit tests iniciales cubren:

- `ApiResponse`.
- reglas basicas de `CuponService`.
- ownership/visibilidad en `ResourceAccessService`.

Los integration tests iniciales cubren:

- endpoint publico `/api/Eventos`.
- endpoint admin protegido sin token.
- `/health/live`.
- `/health/ready`.

## Pendientes

- Agregar Testcontainers con SQL Server para validar migraciones y comportamiento relacional real.
- Cubrir `DescargaService`, `PedidoService`, carrito con cupones, notificaciones y webhooks.
- Medir cobertura en CI y definir un umbral cuando haya una base mas amplia.
