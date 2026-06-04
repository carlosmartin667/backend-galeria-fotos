# Demo Tecnica

Esta guia sirve para mostrar el proyecto en una entrevista tecnica o demo de calidad de codigo. El foco es demostrar arquitectura, seguridad, flujos de negocio, pruebas y criterio tecnico.

## Objetivo De La Demo

Mostrar que el backend resuelve un caso real de galeria fotografica profesional:

- publicacion de eventos y fotos;
- venta con carrito, cupones, pedidos y pagos;
- descargas protegidas;
- sesiones privadas;
- sitio publico comercial;
- operacion administrativa;
- notificaciones;
- reportes;
- bitacora/auditoria;
- tests y CI.

## Checklist Previo

Antes de mostrar:

- `dotnet build fotografia-backend-api.slnx` ejecuta OK.
- `dotnet test` ejecuta OK.
- SQL Server local disponible si se usa base real.
- Migraciones aplicadas.
- Datos demo cargados.
- Backend levantado en `http://localhost:5200`.
- Swagger disponible en `http://localhost:5200/swagger`.
- Health checks responden.
- Frontend Angular levantado desde su repo si se va a mostrar UI.
- Secrets configurados con user-secrets/env vars o placeholders seguros para demo.

## Comandos Backend

```powershell
dotnet tool restore
dotnet build .\fotografia-backend-api.slnx
dotnet test
dotnet tool run dotnet-ef database update --project .\Fotografia.Infrastructure\Fotografia.Infrastructure.csproj --startup-project .\Fotografia.Api\Fotografia.Api.csproj
dotnet run --project .\Fotografia.Api\Fotografia.Api.csproj
```

## Health Checks

Abrir:

```http
GET http://localhost:5200/health/live
GET http://localhost:5200/health/ready
```

Explicar:

- live valida que el proceso responde;
- ready valida disponibilidad para recibir trafico, incluyendo base de datos;
- no expone secretos.

## Swagger

Abrir:

```text
http://localhost:5200/swagger
```

Mostrar:

- grupos de endpoints por controller;
- JWT bearer en Swagger;
- endpoints publicos y protegidos;
- shape `ApiResponse<T>`;
- DTOs separados de entidades.

## Guion De 10 A 15 Minutos

### 1. Resumen inicial

Explicar en 60 segundos:

> Es un backend .NET 10 para una fotografa profesional. Permite publicar eventos, vender fotos y paquetes, cobrar con Mercado Pago, generar descargas protegidas en Cloudflare R2, gestionar sesiones privadas, presupuestos, agenda, notificaciones, promociones, testimonios, reportes y auditoria. Esta separado en Api, Application, Domain e Infrastructure, con DTOs, services, EF Core, JWT, ownership, tests y CI.

### 2. Arquitectura

Abrir [ARCHITECTURE.md](ARCHITECTURE.md).

Mostrar:

- capas;
- diagrama Mermaid;
- controllers delgados;
- services con logica;
- EF Core en Infrastructure;
- integraciones externas fuera de controllers;
- motivo para no usar repository generico.

### 3. Login Admin

Desde Swagger:

```http
POST /api/Auth/login
```

Usar credenciales demo solo de Development si el seed esta cargado. Copiar token y autorizar Swagger con Bearer.

Explicar:

- register publico siempre crea Usuario;
- Admin no se crea desde register publico;
- roles via JWT;
- Invitado no requiere token.

### 4. Dashboard Admin

```http
GET /api/Admin/dashboard
GET /api/Admin/operaciones/resumen
GET /api/Admin/ventas/resumen
```

Mostrar que Admin tiene vision operativa. Explicar que las metricas se calculan desde services, no desde controllers.

### 5. Eventos Y Fotos

Publico:

```http
GET /api/Eventos
GET /api/Eventos/paginado
GET /api/Fotos/evento/{eventoId}
```

Admin:

```http
POST /api/Eventos
PUT /api/Eventos/{id}
PUT /api/Eventos/{eventoId}/portada/{fotoId}
POST /api/Fotos/metadata/bulk
```

Explicar:

- invitados ven solo eventos publicos/publicados;
- Admin ve todo;
- SQL guarda metadata, no imagenes;
- Cloudflare R2 guarda archivos reales;
- portada configurable y fallback;
- bulk metadata no guarda binarios.

### 6. Carrito, Cupon Y Pedido

```http
GET /api/Carrito
POST /api/Carrito/items/foto-evento/{fotoId}
POST /api/Carrito/items/paquete-evento/{paqueteId}
POST /api/Carrito/cupon
POST /api/Carrito/crear-pedido
```

Explicar:

- carrito requiere Usuario/Admin;
- evita duplicados;
- soporta items mixtos;
- pedidos nuevos usan `PedidoItems`;
- se mantiene compatibilidad con `PedidoFotos`.

### 7. Pago

```http
POST /api/Pagos/checkout-pro/preferencias
POST /api/Pagos/webhooks/mercado-pago
```

Explicar:

- Checkout Pro devuelve URL de pago;
- webhook valida consultando Mercado Pago;
- actualiza `Pago` y `Pedido`;
- no se loguean tokens ni payload crudo.

Para demo sin proveedor real, explicar el flujo y mostrar codigo/documentacion. No usar tokens reales.

### 8. Descargas

```http
POST /api/Descargas/link
GET /api/Descargas/mis-descargas
POST /api/Descargas/{id}/regenerar
```

Explicar:

- valida pedido pagado;
- valida ownership;
- valida foto individual, paquete completo o foto privada;
- genera URL temporal/firmada;
- limita usos y vencimiento;
- no expone storage interno.

### 9. Sesiones Privadas

```http
GET /api/SesionesPrivadas
GET /api/SesionesPrivadas/{id}/fotos
POST /api/SesionesPrivadas/{id}/fotos/metadata
```

Explicar:

- Admin administra sesiones;
- Usuario solo ve sus sesiones;
- fotos privadas no se pueden comprar ni descargar si son ajenas.

### 10. Web Publica Comercial

```http
GET /api/Sitio/home
GET /api/Sitio/perfil-fotografa
GET /api/Portfolio
GET /api/Servicios
GET /api/Faq
GET /api/Testimonios
GET /api/Promociones
```

Explicar:

- frontend puede consumir contenido publico;
- Admin administra portfolio, servicios, FAQ, promociones y testimonios;
- no requiere Angular dentro de este repo.

### 11. Presupuestos Y Agenda

```http
POST /api/Presupuestos/solicitudes
GET /api/Agenda/disponibilidad
GET /api/Agenda
```

Explicar:

- solicitud publica para potenciales clientes;
- Admin gestiona estado;
- agenda permite disponibilidad y planificacion.

### 12. Notificaciones

```http
GET /api/Notificaciones/mis-notificaciones
GET /api/Notificaciones/admin
GET /api/Notificaciones/plantillas
POST /api/Notificaciones/admin/{id}/reenviar
```

Explicar:

- plantillas;
- cola de notificaciones;
- worker en background;
- Resend como proveedor;
- envio configurable por ambiente.

### 13. Reportes

```http
GET /api/Reportes/ventas/resumen
GET /api/Admin/ventas/resumen
```

Explicar que los reportes son operativos y que se podrian extender para BI o dashboards.

### 14. Bitacora

```http
GET /api/Bitacora
GET /api/Bitacora/resumen
```

Mostrar:

- eventos de login;
- acciones de negocio;
- filtros;
- correlation id;
- metadata sanitizada;
- endpoint solo Admin.

Explicar:

- si falla auditoria no rompe la operacion principal;
- no guarda tokens, passwords, URLs firmadas ni storage keys.

### 15. Tests Y CI

Mostrar:

```powershell
dotnet test
```

Abrir [TESTING.md](TESTING.md) y `.github/workflows`.

Explicar:

- Unit tests para reglas;
- Integration tests con WebApplicationFactory;
- EF InMemory para tests rapidos;
- siguiente paso: Testcontainers SQL Server.

## Preguntas Que Conviene Provocar

- Como evitas que un usuario descargue fotos ajenas?
- Por que no guardas imagenes en SQL?
- Por que no usas repository generico?
- Como validas webhooks?
- Como evitarias filtrar StorageKey?
- Como harias el deploy productivo?
- Como ampliarias tests?
- Que pasa si falla Resend o Mercado Pago?
- Que guarda la bitacora y que no?

## Cierre De Demo

Terminar con:

- arquitectura por capas;
- seguridad realista;
- flujos de negocio completos;
- integraciones externas aisladas;
- tests y CI;
- roadmap claro para produccion.
