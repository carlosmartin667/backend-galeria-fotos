using Fotografia.Application.Security;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fotografia.Infrastructure.Data;

public static class DbInitializer
{
    private const string AdminEmail = "carloscornejomoscoso@gmail.com";
    private const string AdminPassword = "12345678";

    public static async Task ApplyMigrationsAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (dbContext.Database.IsRelational())
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
    }

    public static async Task SeedTestDataAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("Fotografia.Infrastructure.Data.DbInitializer");
        await SeedTestDataAsync(dbContext, logger, cancellationToken);
    }

    public static async Task SeedTestDataAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await SeedTestDataAsync(dbContext, logger: null, cancellationToken);
    }

    public static async Task SeedTestDataAsync(
        AppDbContext dbContext,
        ILogger? logger,
        CancellationToken cancellationToken = default)
    {
        var passwordHasher = new PasswordHasher<Usuario>();

        LogSeedBlock(logger, "Seed: usuarios demo");
        var admin = await dbContext.Usuarios.FirstOrDefaultAsync(x => x.Email == AdminEmail, cancellationToken);
        if (admin is null)
        {
            admin = new Usuario
            {
                Id = SeedIds.AdminUsuario,
                Nombre = "Carlos Cornejo Moscoso",
                Email = AdminEmail,
                PasswordHash = string.Empty,
                Rol = SistemaRoles.Admin,
                Activo = true,
                CreadoEnUtc = SeedClock.Now
            };

            admin.PasswordHash = passwordHasher.HashPassword(admin, AdminPassword);
            dbContext.Usuarios.Add(admin);
        }
        else
        {
            admin.Nombre = "Carlos Cornejo Moscoso";
            admin.Rol = SistemaRoles.Admin;
            admin.Activo = true;
            admin.PasswordHash = passwordHasher.HashPassword(admin, AdminPassword);
            admin.ActualizadoEnUtc = SeedClock.Now;
        }

        await SaveSeedChangesAsync(dbContext, logger, "Seed: usuarios demo", cancellationToken);

        await SeedClientesAsync(dbContext, logger, cancellationToken);
        await SeedEventosAsync(dbContext, logger, admin.Id, cancellationToken);
        await SeedFotosAsync(dbContext, logger, cancellationToken);
        await SeedPortadasEventosAsync(dbContext, logger, cancellationToken);
        await SeedPedidosAsync(dbContext, logger, cancellationToken);
        await SeedRequestedDemoDataAsync(dbContext, passwordHasher, logger, cancellationToken);
        await SeedPlantillasNotificacionAsync(dbContext, logger, cancellationToken);
    }

    private static async Task SeedClientesAsync(AppDbContext dbContext, ILogger? logger, CancellationToken cancellationToken)
    {
        LogSeedBlock(logger, "Seed: clientes demo");
        var clientes = new[]
        {
            new Cliente
            {
                Id = SeedIds.ClienteSofia,
                Nombre = "Sofia Martinez",
                Email = "sofia.martinez@example.com",
                Telefono = "+54 9 11 5555-1201",
                Documento = "33111222",
                CreadoEnUtc = SeedClock.Now.AddDays(-32)
            },
            new Cliente
            {
                Id = SeedIds.ClienteValentina,
                Nombre = "Valentina Rios",
                Email = "valentina.rios@example.com",
                Telefono = "+54 9 11 5555-1202",
                Documento = "34777888",
                CreadoEnUtc = SeedClock.Now.AddDays(-24)
            },
            new Cliente
            {
                Id = SeedIds.ClienteMateo,
                Nombre = "Mateo Alvarez",
                Email = "mateo.alvarez@example.com",
                Telefono = "+54 9 11 5555-1203",
                Documento = "30123456",
                CreadoEnUtc = SeedClock.Now.AddDays(-18)
            }
        };

        foreach (var cliente in clientes)
        {
            if (!await dbContext.Clientes.AnyAsync(x => x.Id == cliente.Id, cancellationToken))
            {
                dbContext.Clientes.Add(cliente);
            }
        }

        await SaveSeedChangesAsync(dbContext, logger, "Seed: clientes demo", cancellationToken);
    }

    private static async Task SeedEventosAsync(
        AppDbContext dbContext,
        ILogger? logger,
        Guid adminId,
        CancellationToken cancellationToken)
    {
        LogSeedBlock(logger, "Seed: eventos demo");
        var eventos = new[]
        {
            new Evento
            {
                Id = SeedIds.EventoBoda,
                Nombre = "Boda Sofia y Lucas",
                Descripcion = "Ceremonia civil y fiesta en salon Las Acacias.",
                Slug = "boda-sofia-y-lucas",
                FechaEventoUtc = new DateTime(2026, 4, 18, 21, 0, 0, DateTimeKind.Utc),
                Estado = EventoEstados.Publicado,
                Visibilidad = EventoVisibilidades.Publico,
                Activo = true,
                ClientePrincipalId = SeedIds.ClienteSofia,
                CreadoPorUsuarioId = adminId,
                CreadoEnUtc = SeedClock.Now.AddDays(-30)
            },
            new Evento
            {
                Id = SeedIds.EventoQuince,
                Nombre = "Quince Valentina",
                Descripcion = "Sesion previa, recepcion y pista.",
                Slug = "quince-valentina",
                FechaEventoUtc = new DateTime(2026, 5, 9, 22, 0, 0, DateTimeKind.Utc),
                Estado = EventoEstados.Publicado,
                Visibilidad = EventoVisibilidades.Publico,
                Activo = true,
                ClientePrincipalId = SeedIds.ClienteValentina,
                CreadoPorUsuarioId = adminId,
                CreadoEnUtc = SeedClock.Now.AddDays(-20)
            },
            new Evento
            {
                Id = SeedIds.EventoCorporativo,
                Nombre = "Workshop Marca Personal",
                Descripcion = "Retratos profesionales y cobertura del taller.",
                Slug = "workshop-marca-personal",
                FechaEventoUtc = new DateTime(2026, 5, 22, 18, 0, 0, DateTimeKind.Utc),
                Estado = EventoEstados.Publicado,
                Visibilidad = EventoVisibilidades.Publico,
                Activo = true,
                ClientePrincipalId = SeedIds.ClienteMateo,
                CreadoPorUsuarioId = adminId,
                CreadoEnUtc = SeedClock.Now.AddDays(-12)
            }
        };

        foreach (var evento in eventos)
        {
            var existing = await dbContext.Eventos.FirstOrDefaultAsync(x => x.Id == evento.Id, cancellationToken);
            if (existing is null)
            {
                dbContext.Eventos.Add(evento);
                continue;
            }

            existing.Nombre = evento.Nombre;
            existing.Descripcion = evento.Descripcion;
            existing.Slug = evento.Slug;
            existing.FechaEventoUtc = evento.FechaEventoUtc;
            existing.Estado = evento.Estado;
            existing.Visibilidad = evento.Visibilidad;
            existing.Activo = evento.Activo;
            existing.ClientePrincipalId = evento.ClientePrincipalId;
            existing.CreadoPorUsuarioId = evento.CreadoPorUsuarioId;
            existing.ActualizadoEnUtc = SeedClock.Now;
        }

        await SaveSeedChangesAsync(dbContext, logger, "Seed: eventos demo", cancellationToken);
    }

    private static async Task SeedFotosAsync(AppDbContext dbContext, ILogger? logger, CancellationToken cancellationToken)
    {
        LogSeedBlock(logger, "Seed: fotos demo");
        var fotos = new[]
        {
            CreateFoto(SeedIds.FotoBoda01, SeedIds.EventoBoda, "boda-sofia-lucas-001.jpg", "image/jpeg", 6500m, 4892300, 4000, 6000),
            CreateFoto(SeedIds.FotoBoda02, SeedIds.EventoBoda, "boda-sofia-lucas-002.jpg", "image/jpeg", 6500m, 5128900, 4000, 6000),
            CreateFoto(SeedIds.FotoBoda03, SeedIds.EventoBoda, "boda-sofia-lucas-003.webp", "image/webp", 7200m, 2189900, 3840, 5760),
            CreateFoto(SeedIds.FotoBoda04, SeedIds.EventoBoda, "boda-sofia-lucas-004.jpg", "image/jpeg", 6500m, 4981100, 4000, 6000),
            CreateFoto(SeedIds.FotoQuince01, SeedIds.EventoQuince, "quince-valentina-001.jpg", "image/jpeg", 5200m, 4202300, 4000, 6000),
            CreateFoto(SeedIds.FotoQuince02, SeedIds.EventoQuince, "quince-valentina-002.png", "image/png", 5600m, 8421200, 4000, 6000),
            CreateFoto(SeedIds.FotoQuince03, SeedIds.EventoQuince, "quince-valentina-003.webp", "image/webp", 5200m, 1983400, 3840, 5760),
            CreateFoto(SeedIds.FotoWorkshop01, SeedIds.EventoCorporativo, "workshop-marca-personal-001.jpg", "image/jpeg", 4300m, 3781200, 4000, 6000),
            CreateFoto(SeedIds.FotoWorkshop02, SeedIds.EventoCorporativo, "workshop-marca-personal-002.jpg", "image/jpeg", 4300m, 3911200, 4000, 6000),
            CreateFoto(SeedIds.FotoWorkshop03, SeedIds.EventoCorporativo, "workshop-marca-personal-003.webp", "image/webp", 4800m, 1765400, 3840, 5760)
        };

        foreach (var foto in fotos)
        {
            var existing = await dbContext.Fotos.FirstOrDefaultAsync(x => x.Id == foto.Id, cancellationToken);
            if (existing is null)
            {
                dbContext.Fotos.Add(foto);
                continue;
            }

            ApplySeedFoto(existing, foto);
        }

        await SaveSeedChangesAsync(dbContext, logger, "Seed: fotos demo", cancellationToken);
    }

    private static async Task SeedPortadasEventosAsync(AppDbContext dbContext, ILogger? logger, CancellationToken cancellationToken)
    {
        LogSeedBlock(logger, "Seed: portadas de evento");
        await SetPortadaAsync(dbContext, SeedIds.EventoBoda, SeedIds.FotoBoda01, cancellationToken);
        await SetPortadaAsync(dbContext, SeedIds.EventoQuince, SeedIds.FotoQuince01, cancellationToken);
        await SetPortadaAsync(dbContext, SeedIds.EventoCorporativo, SeedIds.FotoWorkshop01, cancellationToken);
        await SaveSeedChangesAsync(dbContext, logger, "Seed: portadas de evento", cancellationToken);
    }

    private static async Task SeedPedidosAsync(AppDbContext dbContext, ILogger? logger, CancellationToken cancellationToken)
    {
        LogSeedBlock(logger, "Seed: pedidos demo");
        if (!await dbContext.Pedidos.AnyAsync(x => x.Id == SeedIds.PedidoBodaPagado, cancellationToken))
        {
            var pedido = new Pedido
            {
                Id = SeedIds.PedidoBodaPagado,
                EventoId = SeedIds.EventoBoda,
                ClienteId = SeedIds.ClienteSofia,
                Estado = "Pagado",
                Subtotal = 20200m,
                DescuentoTotal = 0,
                Total = 20200m,
                Moneda = "ARS",
                MercadoPagoPreferenceId = "pref_test_boda_sofia_lucas",
                CreadoEnUtc = SeedClock.Now.AddDays(-8),
                PedidoFotos =
                [
                    new PedidoFoto { FotoId = SeedIds.FotoBoda01, Cantidad = 1, PrecioUnitario = 6500m },
                    new PedidoFoto { FotoId = SeedIds.FotoBoda02, Cantidad = 1, PrecioUnitario = 6500m },
                    new PedidoFoto { FotoId = SeedIds.FotoBoda03, Cantidad = 1, PrecioUnitario = 7200m }
                ],
                Pago = new Pago
                {
                    Id = SeedIds.PagoBoda,
                    MercadoPagoPaymentId = "payment_test_boda_sofia_lucas",
                    MercadoPagoPreferenceId = "pref_test_boda_sofia_lucas",
                    Estado = "approved",
                    Monto = 20200m,
                    Moneda = "ARS",
                    CreadoEnUtc = SeedClock.Now.AddDays(-8),
                    PagadoEnUtc = SeedClock.Now.AddDays(-8).AddMinutes(12),
                    ActualizadoEnUtc = SeedClock.Now.AddDays(-8).AddMinutes(12)
                }
            };

            dbContext.Pedidos.Add(pedido);
        }

        if (!await dbContext.Pedidos.AnyAsync(x => x.Id == SeedIds.PedidoQuincePendiente, cancellationToken))
        {
            dbContext.Pedidos.Add(new Pedido
            {
                Id = SeedIds.PedidoQuincePendiente,
                EventoId = SeedIds.EventoQuince,
                ClienteId = SeedIds.ClienteValentina,
                Estado = "Pendiente",
                Subtotal = 15600m,
                DescuentoTotal = 0,
                Total = 15600m,
                Moneda = "ARS",
                MercadoPagoPreferenceId = "pref_test_quince_valentina",
                CreadoEnUtc = SeedClock.Now.AddDays(-3),
                PedidoFotos =
                [
                    new PedidoFoto { FotoId = SeedIds.FotoQuince01, Cantidad = 1, PrecioUnitario = 5200m },
                    new PedidoFoto { FotoId = SeedIds.FotoQuince02, Cantidad = 1, PrecioUnitario = 5600m },
                    new PedidoFoto { FotoId = SeedIds.FotoQuince03, Cantidad = 1, PrecioUnitario = 4800m }
                ]
            });
        }

        if (!await dbContext.Pedidos.AnyAsync(x => x.Id == SeedIds.PedidoWorkshopPagado, cancellationToken))
        {
            dbContext.Pedidos.Add(new Pedido
            {
                Id = SeedIds.PedidoWorkshopPagado,
                EventoId = SeedIds.EventoCorporativo,
                ClienteId = SeedIds.ClienteMateo,
                Estado = "Pagado",
                Subtotal = 8600m,
                DescuentoTotal = 0,
                Total = 8600m,
                Moneda = "ARS",
                MercadoPagoPreferenceId = "pref_test_workshop_marca_personal",
                CreadoEnUtc = SeedClock.Now.AddDays(-2),
                PedidoFotos =
                [
                    new PedidoFoto { FotoId = SeedIds.FotoWorkshop01, Cantidad = 1, PrecioUnitario = 4300m },
                    new PedidoFoto { FotoId = SeedIds.FotoWorkshop02, Cantidad = 1, PrecioUnitario = 4300m }
                ],
                Pago = new Pago
                {
                    Id = SeedIds.PagoWorkshop,
                    MercadoPagoPaymentId = "payment_test_workshop_marca_personal",
                    MercadoPagoPreferenceId = "pref_test_workshop_marca_personal",
                    Estado = "approved",
                    Monto = 8600m,
                    Moneda = "ARS",
                    CreadoEnUtc = SeedClock.Now.AddDays(-2),
                    PagadoEnUtc = SeedClock.Now.AddDays(-2).AddMinutes(7),
                    ActualizadoEnUtc = SeedClock.Now.AddDays(-2).AddMinutes(7)
                }
            });
        }

        await EnsureSeedPedidoTotalsAsync(dbContext, cancellationToken);

        await SaveSeedChangesAsync(dbContext, logger, "Seed: pedidos demo", cancellationToken);

        await SeedDescargasAsync(dbContext, logger, cancellationToken);
    }

    private static async Task EnsureSeedPedidoTotalsAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var pedidoIds = new[]
        {
            SeedIds.PedidoBodaPagado,
            SeedIds.PedidoQuincePendiente,
            SeedIds.PedidoWorkshopPagado
        };

        var pedidos = await dbContext.Pedidos
            .Include(x => x.PedidoFotos)
            .Include(x => x.PedidoItems)
            .Where(x => pedidoIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        foreach (var pedido in pedidos)
        {
            var subtotal = pedido.PedidoItems.Count > 0
                ? pedido.PedidoItems.Sum(x => x.Subtotal)
                : pedido.PedidoFotos.Sum(x => x.PrecioUnitario * x.Cantidad);

            if (subtotal <= 0)
            {
                subtotal = pedido.Total;
            }

            pedido.Subtotal = subtotal;
            pedido.DescuentoTotal = Math.Max(0, pedido.DescuentoTotal);
            pedido.Total = Math.Max(0, subtotal - pedido.DescuentoTotal);
            pedido.ActualizadoEnUtc = SeedClock.Now;
        }
    }

    private static async Task SeedDescargasAsync(AppDbContext dbContext, ILogger? logger, CancellationToken cancellationToken)
    {
        LogSeedBlock(logger, "Seed: descargas demo");
        var descargas = new[]
        {
            CreateDescarga(SeedIds.DescargaBoda01, SeedIds.PedidoBodaPagado, SeedIds.EventoBoda, SeedIds.ClienteSofia, SeedIds.FotoBoda01, "boda-sofia-lucas-001.jpg"),
            CreateDescarga(SeedIds.DescargaBoda02, SeedIds.PedidoBodaPagado, SeedIds.EventoBoda, SeedIds.ClienteSofia, SeedIds.FotoBoda02, "boda-sofia-lucas-002.jpg"),
            CreateDescarga(SeedIds.DescargaWorkshop01, SeedIds.PedidoWorkshopPagado, SeedIds.EventoCorporativo, SeedIds.ClienteMateo, SeedIds.FotoWorkshop01, "workshop-marca-personal-001.jpg")
        };

        foreach (var descarga in descargas)
        {
            var existing = await dbContext.Descargas.FirstOrDefaultAsync(x => x.Id == descarga.Id, cancellationToken);
            if (existing is null)
            {
                dbContext.Descargas.Add(descarga);
                continue;
            }

            ApplySeedDescarga(existing, descarga);
        }

        await SaveSeedChangesAsync(dbContext, logger, "Seed: descargas demo", cancellationToken);
    }

    private static async Task SeedPlantillasNotificacionAsync(
        AppDbContext dbContext,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        LogSeedBlock(logger, "Seed: plantillas de notificacion");
        var templates = new[]
        {
            CreatePlantilla(NotificacionTipos.SolicitudPresupuestoCreadaAdmin, NotificacionCanales.Interna, "Nueva solicitud de presupuesto", "<p>{{NombreCliente}} solicito presupuesto para {{NombreEvento}}.</p><p>Email: {{EmailCliente}}</p><p>Fecha: {{Fecha}}</p>", "{{NombreCliente}} solicito presupuesto para {{NombreEvento}}."),
            CreatePlantilla(NotificacionTipos.SolicitudPresupuestoRecibidaCliente, NotificacionCanales.Email, "Recibimos tu solicitud", "<p>Hola {{NombreCliente}}, recibimos tu solicitud de presupuesto.</p><p>Te vamos a responder a la brevedad.</p>", "Hola {{NombreCliente}}, recibimos tu solicitud de presupuesto."),
            CreatePlantilla(NotificacionTipos.PedidoCreadoCliente, NotificacionCanales.Email, "Pedido creado", "<p>Hola {{NombreCliente}}, tu pedido {{PedidoId}} fue creado por un total de {{Total}}.</p>", "Tu pedido {{PedidoId}} fue creado por un total de {{Total}}."),
            CreatePlantilla(NotificacionTipos.PedidoCreadoAdmin, NotificacionCanales.Interna, "Nuevo pedido creado", "<p>{{NombreCliente}} creo el pedido {{PedidoId}} por {{Total}}.</p>", "{{NombreCliente}} creo el pedido {{PedidoId}} por {{Total}}."),
            CreatePlantilla(NotificacionTipos.PagoAprobadoCliente, NotificacionCanales.Email, "Pago aprobado", "<p>Hola {{NombreCliente}}, el pago del pedido {{PedidoId}} fue aprobado.</p>", "El pago del pedido {{PedidoId}} fue aprobado."),
            CreatePlantilla(NotificacionTipos.PagoAprobadoAdmin, NotificacionCanales.Interna, "Pago aprobado", "<p>Se aprobo el pago del pedido {{PedidoId}} por {{Total}}.</p>", "Se aprobo el pago del pedido {{PedidoId}} por {{Total}}."),
            CreatePlantilla(NotificacionTipos.PedidoListoDescargaCliente, NotificacionCanales.Email, "Tu pedido esta listo para descargar", "<p>Hola {{NombreCliente}}, tu pedido {{PedidoId}} esta listo para descargar. {{Link}}</p>", "Tu pedido {{PedidoId}} esta listo para descargar."),
            CreatePlantilla(NotificacionTipos.DescargaLinkGeneradoCliente, NotificacionCanales.Email, "Link de descarga generado", "<p>Hola {{NombreCliente}}, generamos una descarga para tu pedido {{PedidoId}}. {{Link}}</p>", "Generamos una descarga para tu pedido {{PedidoId}}."),
            CreatePlantilla(NotificacionTipos.EventoPublicadoCliente, NotificacionCanales.Email, "Galeria publicada", "<p>Hola {{NombreCliente}}, la galeria {{NombreEvento}} ya esta publicada. {{Link}}</p>", "La galeria {{NombreEvento}} ya esta publicada."),
            CreatePlantilla(NotificacionTipos.SesionPrivadaListaCliente, NotificacionCanales.Email, "Sesion privada lista", "<p>Hola {{NombreCliente}}, tu sesion privada {{NombreEvento}} esta en estado {{Estado}}. {{Link}}</p>", "Tu sesion privada {{NombreEvento}} esta en estado {{Estado}}."),
            CreatePlantilla(NotificacionTipos.NuevoComentarioAdmin, NotificacionCanales.Interna, "Nuevo comentario", "<p>{{NombreCliente}} agrego un comentario en {{NombreEvento}}.</p>", "{{NombreCliente}} agrego un comentario en {{NombreEvento}}."),
            CreatePlantilla(NotificacionTipos.CuponAplicadoCliente, NotificacionCanales.Email, "Cupon aplicado", "<p>Hola {{NombreCliente}}, aplicamos el cupon {{Codigo}} por {{Descuento}}. Total final: {{Total}}.</p>", "Aplicamos el cupon {{Codigo}} por {{Descuento}}. Total final: {{Total}}."),
            CreatePlantilla(NotificacionTipos.CuponUsadoAdmin, NotificacionCanales.Interna, "Cupon usado", "<p>Se uso el cupon {{Codigo}} en el pedido {{PedidoId}} con descuento {{Descuento}}.</p>", "Se uso el cupon {{Codigo}} en el pedido {{PedidoId}}."),
            CreatePlantilla(NotificacionTipos.TestimonioRecibidoAdmin, NotificacionCanales.Interna, "Nuevo testimonio recibido", "<p>{{NombreCliente}} dejo un testimonio con calificacion {{Calificacion}}.</p>", "{{NombreCliente}} dejo un testimonio con calificacion {{Calificacion}}."),
            CreatePlantilla(NotificacionTipos.CarritoAbandonadoCliente, NotificacionCanales.Email, "Tenes fotos pendientes en tu carrito", "<p>Hola {{NombreCliente}}, tu carrito tiene {{CantidadItems}} items por {{Total}}. {{Link}}</p>", "Tu carrito tiene {{CantidadItems}} items por {{Total}}."),
            CreatePlantilla(NotificacionTipos.PromocionActivaAdmin, NotificacionCanales.Interna, "Promocion activa", "<p>La promocion {{Titulo}} esta activa. Tipo: {{Tipo}}.</p>", "La promocion {{Titulo}} esta activa.")
        };

        foreach (var template in templates)
        {
            var existing = await dbContext.PlantillasNotificacion
                .FirstOrDefaultAsync(x => x.Codigo == template.Codigo, cancellationToken);

            if (existing is null)
            {
                dbContext.PlantillasNotificacion.Add(template);
                continue;
            }

            existing.Canal = template.Canal;
            existing.Asunto = template.Asunto;
            existing.CuerpoHtml = template.CuerpoHtml;
            existing.CuerpoTexto = template.CuerpoTexto;
            existing.Activa = true;
            existing.FechaActualizacionUtc = SeedClock.Now;
        }

        await SaveSeedChangesAsync(dbContext, logger, "Seed: plantillas de notificacion", cancellationToken);
    }

    private static async Task SeedRequestedDemoDataAsync(
        AppDbContext dbContext,
        PasswordHasher<Usuario> passwordHasher,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        LogSeedBlock(logger, "Seed: usuarios demo solicitados");
        var admin = await UpsertUserAsync(
            dbContext,
            passwordHasher,
            SeedIds.AdminDemo,
            "Administrador Fotografia",
            "admin@fotografia.com",
            "Admin123456",
            SistemaRoles.Admin,
            cancellationToken,
            descripcion: "Fotografa profesional especializada en eventos, retratos y galerias privadas.",
            whatsApp: "+5493510000000",
            instagram: "@fotografia.demo",
            correoPublico: "contacto@fotografia.com",
            direccion: "Cordoba, Argentina");

        var usuarioCliente = await UpsertUserAsync(
            dbContext,
            passwordHasher,
            SeedIds.UsuarioClienteDemo,
            "Cliente Demo",
            "cliente@fotografia.com",
            "Cliente123456",
            SistemaRoles.Usuario,
            cancellationToken);

        var usuarioInvitado = await UpsertUserAsync(
            dbContext,
            passwordHasher,
            SeedIds.UsuarioInvitadoDemo,
            "Cliente Invitado Demo",
            "usuario@fotografia.com",
            "Usuario123456",
            SistemaRoles.Usuario,
            cancellationToken);

        await SaveSeedChangesAsync(dbContext, logger, "Seed: usuarios demo solicitados", cancellationToken);

        LogSeedBlock(logger, "Seed: clientes demo solicitados");
        var clienteDemo = await UpsertClienteAsync(
            dbContext,
            SeedIds.ClienteDemo,
            usuarioCliente.Id,
            "Cliente Demo",
            "cliente@fotografia.com",
            "+5493511111111",
            "30111222",
            cancellationToken);

        var clienteInvitado = await UpsertClienteAsync(
            dbContext,
            SeedIds.ClienteInvitadoDemo,
            usuarioInvitado.Id,
            "Cliente Invitado Demo",
            "usuario@fotografia.com",
            "+5493512222222",
            "30222333",
            cancellationToken);

        await SaveSeedChangesAsync(dbContext, logger, "Seed: clientes demo solicitados", cancellationToken);

        LogSeedBlock(logger, "Seed: eventos demo solicitados");
        var casamiento = await UpsertEventoAsync(
            dbContext,
            SeedIds.EventoCasamientoDemo,
            "Casamiento Demo",
            "Evento de casamiento con galeria privada para clientes.",
            "casamiento-demo",
            now.AddDays(10),
            EventoEstados.Publicado,
            EventoVisibilidades.Publico,
            clienteDemo.Id,
            admin.Id,
            cancellationToken);

        var cumpleanos = await UpsertEventoAsync(
            dbContext,
            SeedIds.EventoCumpleanosDemo,
            "Cumpleanos Demo",
            "Evento social con fotos disponibles para seleccion y compra.",
            "cumpleanos-demo",
            now.AddDays(20),
            EventoEstados.Publicado,
            EventoVisibilidades.Publico,
            clienteInvitado.Id,
            admin.Id,
            cancellationToken);

        var book = await UpsertEventoAsync(
            dbContext,
            SeedIds.EventoBookDemo,
            "Book Fotografico Demo",
            "Sesion individual de prueba.",
            "book-fotografico-demo",
            now.AddDays(30),
            EventoEstados.Borrador,
            EventoVisibilidades.Oculto,
            null,
            admin.Id,
            cancellationToken);

        await SaveSeedChangesAsync(dbContext, logger, "Seed: eventos demo solicitados", cancellationToken);

        LogSeedBlock(logger, "Seed: fotos demo solicitadas");
        var casamiento001 = await UpsertDemoFotoAsync(dbContext, SeedIds.FotoCasamiento001, casamiento.Id, "casamiento-001.jpg", now, cancellationToken);
        var casamiento002 = await UpsertDemoFotoAsync(dbContext, SeedIds.FotoCasamiento002, casamiento.Id, "casamiento-002.jpg", now, cancellationToken);
        await UpsertDemoFotoAsync(dbContext, SeedIds.FotoCasamiento003, casamiento.Id, "casamiento-003.jpg", now, cancellationToken);

        var cumpleanos001 = await UpsertDemoFotoAsync(dbContext, SeedIds.FotoCumpleanos001, cumpleanos.Id, "cumpleanos-001.jpg", now, cancellationToken);
        await UpsertDemoFotoAsync(dbContext, SeedIds.FotoCumpleanos002, cumpleanos.Id, "cumpleanos-002.jpg", now, cancellationToken);
        await UpsertDemoFotoAsync(dbContext, SeedIds.FotoCumpleanos003, cumpleanos.Id, "cumpleanos-003.jpg", now, cancellationToken);

        var book001 = await UpsertDemoFotoAsync(dbContext, SeedIds.FotoBook001, book.Id, "book-001.jpg", now, cancellationToken);
        await UpsertDemoFotoAsync(dbContext, SeedIds.FotoBook002, book.Id, "book-002.jpg", now, cancellationToken);

        await SaveSeedChangesAsync(dbContext, logger, "Seed: fotos demo solicitadas", cancellationToken);

        LogSeedBlock(logger, "Seed: portadas de evento solicitadas");
        await SetPortadaAsync(dbContext, casamiento.Id, casamiento001.Id, cancellationToken);
        await SetPortadaAsync(dbContext, cumpleanos.Id, cumpleanos001.Id, cancellationToken);
        await SetPortadaAsync(dbContext, book.Id, book001.Id, cancellationToken);
        await SaveSeedChangesAsync(dbContext, logger, "Seed: portadas de evento solicitadas", cancellationToken);

        LogSeedBlock(logger, "Seed: pedidos demo solicitados");
        var pedidoPendiente = await UpsertPedidoAsync(
            dbContext,
            SeedIds.PedidoCasamientoPendiente,
            clienteDemo.Id,
            casamiento.Id,
            "Pendiente",
            "ARS",
            [casamiento001, casamiento002],
            now,
            cancellationToken);

        var pedidoPagado = await UpsertPedidoAsync(
            dbContext,
            SeedIds.PedidoCumpleanosPagado,
            clienteInvitado.Id,
            cumpleanos.Id,
            "Pagado",
            "ARS",
            [cumpleanos001],
            now,
            cancellationToken);

        await SaveSeedChangesAsync(dbContext, logger, "Seed: pedidos demo solicitados", cancellationToken);

        LogSeedBlock(logger, "Seed: pagos y descargas demo solicitados");
        await UpsertPagoAsync(dbContext, SeedIds.PagoCumpleanos, pedidoPagado, "Aprobado", "MP-DEMO-0001", "PREF-DEMO-0001", now, cancellationToken);
        await UpsertDescargaAsync(dbContext, SeedIds.DescargaCumpleanos001, pedidoPagado, clienteInvitado.Id, cumpleanos.Id, cumpleanos001, now, cancellationToken);
        await SaveSeedChangesAsync(dbContext, logger, "Seed: pagos y descargas demo solicitados", cancellationToken);

        LogSeedBlock(logger, "Seed: comentarios y favoritos demo");
        await UpsertComentarioEventoAsync(dbContext, SeedIds.ComentarioEventoAdmin, casamiento.Id, admin.Id, "Evento cargado correctamente. Galeria lista para revision.", now, cancellationToken);
        await UpsertComentarioEventoAsync(dbContext, SeedIds.ComentarioEventoCliente, casamiento.Id, usuarioCliente.Id, "Me gustaria marcar estas fotos como favoritas para revisarlas luego.", now, cancellationToken);
        await UpsertComentarioFotoAsync(dbContext, SeedIds.ComentarioFotoAdmin, casamiento001.Id, admin.Id, "Foto destacada para portada.", now, cancellationToken);
        await UpsertComentarioFotoAsync(dbContext, SeedIds.ComentarioFotoCliente, casamiento002.Id, usuarioCliente.Id, "Esta foto me interesa para compra.", now, cancellationToken);

        await UpsertEventoFavoritoAsync(dbContext, SeedIds.FavoritoEventoCliente, usuarioCliente.Id, casamiento.Id, now, cancellationToken);
        await UpsertFotoFavoritaAsync(dbContext, SeedIds.FavoritoFotoCliente001, usuarioCliente.Id, casamiento001.Id, now, cancellationToken);
        await UpsertFotoFavoritaAsync(dbContext, SeedIds.FavoritoFotoCliente002, usuarioCliente.Id, casamiento002.Id, now, cancellationToken);
        await UpsertEventoFavoritoAsync(dbContext, SeedIds.FavoritoEventoAdmin, admin.Id, book.Id, now, cancellationToken);
        await UpsertFotoFavoritaAsync(dbContext, SeedIds.FavoritoFotoAdmin, admin.Id, book001.Id, now, cancellationToken);
        await SaveSeedChangesAsync(dbContext, logger, "Seed: comentarios y favoritos demo", cancellationToken);

        LogSeedBlock(logger, "Seed: perfil fotografa");
        await UpsertPerfilFotografaAsync(dbContext, SeedIds.PerfilFotografaDemo, now, cancellationToken);
        await SaveSeedChangesAsync(dbContext, logger, "Seed: perfil fotografa", cancellationToken);

        LogSeedBlock(logger, "Seed: sitio publico comercial");
        await UpsertServicioFotografiaAsync(dbContext, SeedIds.ServicioCasamientos, "Casamientos", "Cobertura documental de ceremonia, recepcion y fiesta con entrega digital.", 180000m, "8 horas", 300, "https://placehold.co/900x600?text=Casamientos", 1, now, cancellationToken);
        await UpsertServicioFotografiaAsync(dbContext, SeedIds.ServicioCumpleanos, "Cumpleanos y quince", "Fotografia social para cumpleanos, quince y celebraciones familiares.", 95000m, "4 horas", 180, "https://placehold.co/900x600?text=Cumpleanos", 2, now, cancellationToken);
        await UpsertServicioFotografiaAsync(dbContext, SeedIds.ServicioBookPersonal, "Book personal", "Sesion personalizada en exterior o estudio con seleccion editada.", 55000m, "2 horas", 40, "https://placehold.co/900x600?text=Book+personal", 3, now, cancellationToken);
        await UpsertServicioFotografiaAsync(dbContext, SeedIds.ServicioCorporativo, "Eventos corporativos", "Cobertura para conferencias, workshops y lanzamientos de marca.", 120000m, "5 horas", 200, "https://placehold.co/900x600?text=Corporativo", 4, now, cancellationToken);
        await UpsertServicioFotografiaAsync(dbContext, SeedIds.ServicioProducto, "Fotografia de producto", "Imagenes limpias para catalogo, tienda online y redes sociales.", null, "A medida", null, "https://placehold.co/900x600?text=Producto", 5, now, cancellationToken);
        await UpsertServicioFotografiaAsync(dbContext, SeedIds.ServicioSesionesPrivadas, "Sesiones privadas", "Galerias privadas con compra y descarga segura de fotos.", 65000m, "2 horas", 50, "https://placehold.co/900x600?text=Sesion+privada", 6, now, cancellationToken);

        await UpsertPortfolioItemAsync(dbContext, SeedIds.PortfolioBodas, "Bodas documentales", "Momentos espontaneos de ceremonia y fiesta.", "https://placehold.co/900x600?text=Bodas", "Bodas", 1, true, now, cancellationToken);
        await UpsertPortfolioItemAsync(dbContext, SeedIds.PortfolioQuince, "Quince y celebraciones", "Retratos y cobertura social para celebraciones familiares.", "https://placehold.co/900x600?text=Quince", "Social", 2, true, now, cancellationToken);
        await UpsertPortfolioItemAsync(dbContext, SeedIds.PortfolioBooks, "Books personales", "Sesiones de retrato para marca personal y redes.", "https://placehold.co/900x600?text=Books", "Retrato", 3, true, now, cancellationToken);
        await UpsertPortfolioItemAsync(dbContext, SeedIds.PortfolioCorporativo, "Cobertura corporativa", "Imagenes profesionales para empresas y workshops.", "https://placehold.co/900x600?text=Corporativo", "Corporativo", 4, false, now, cancellationToken);
        await UpsertPortfolioItemAsync(dbContext, SeedIds.PortfolioProducto, "Producto y catalogo", "Fotos claras para ecommerce, menus y redes sociales.", "https://placehold.co/900x600?text=Producto", "Producto", 5, false, now, cancellationToken);
        await UpsertPortfolioItemAsync(dbContext, SeedIds.PortfolioFamilia, "Familias y sesiones privadas", "Galerias intimas con seleccion y entrega cuidada.", "https://placehold.co/900x600?text=Familia", "Familia", 6, false, now, cancellationToken);

        await UpsertPreguntaFrecuenteAsync(dbContext, SeedIds.FaqEntrega, "Como se entregan las fotos?", "Las fotos se entregan en galeria digital privada con previews y opciones de descarga seguras.", "Entrega", 1, now, cancellationToken);
        await UpsertPreguntaFrecuenteAsync(dbContext, SeedIds.FaqReservas, "Como reservo una fecha?", "Podemos coordinar por WhatsApp o email publico para revisar disponibilidad y confirmar la reserva.", "Reservas", 2, now, cancellationToken);
        await UpsertPreguntaFrecuenteAsync(dbContext, SeedIds.FaqPagos, "Que medios de pago aceptan?", "El sistema esta preparado para pagos online con Mercado Pago y seguimiento del pedido.", "Pagos", 3, now, cancellationToken);
        await UpsertPreguntaFrecuenteAsync(dbContext, SeedIds.FaqPrivadas, "Las galerias privadas son seguras?", "Si. Cada cliente accede a sus sesiones privadas y descargas segun permisos y compras realizadas.", "Galerias", 4, now, cancellationToken);
        await UpsertPreguntaFrecuenteAsync(dbContext, SeedIds.FaqEdicion, "Las fotos tienen edicion?", "Cada servicio define cantidad, seleccion y estilo de edicion antes de confirmar el trabajo.", "Servicios", 5, now, cancellationToken);
        await SaveSeedChangesAsync(dbContext, logger, "Seed: sitio publico comercial", cancellationToken);

        LogSeedBlock(logger, "Seed: paquetes demo");
        await UpsertPaqueteEventoAsync(dbContext, SeedIds.PaqueteCasamientoCompleto, casamiento.Id, "Pack completo del evento", "Incluye todas las fotos activas del casamiento.", 18500m, true, now, cancellationToken);
        await UpsertPaqueteEventoAsync(dbContext, SeedIds.PaqueteCasamientoPremium, casamiento.Id, "Pack seleccion premium", "Pack demo para una seleccion curada.", 9500m, false, now, cancellationToken);
        await UpsertPaqueteEventoAsync(dbContext, SeedIds.PaqueteCumpleCompleto, cumpleanos.Id, "Pack completo cumpleanos", "Incluye todas las fotos activas del evento.", 14500m, true, now, cancellationToken);
        await SaveSeedChangesAsync(dbContext, logger, "Seed: paquetes demo", cancellationToken);

        LogSeedBlock(logger, "Seed: sesiones privadas");
        var sesionPrivada = await UpsertSesionPrivadaAsync(
            dbContext,
            SeedIds.SesionPrivadaClienteDemo,
            clienteDemo.Id,
            "Sesion privada familiar",
            "Galeria privada demo asociada al cliente.",
            now.AddDays(-3),
            now,
            cancellationToken);
        await SaveSeedChangesAsync(dbContext, logger, "Seed: sesiones privadas", cancellationToken);

        LogSeedBlock(logger, "Seed: solicitudes y agenda demo");
        var solicitudCasamiento = await UpsertSolicitudPresupuestoAsync(
            dbContext,
            SeedIds.SolicitudPresupuestoCasamiento,
            "Lucia Fernandez",
            "lucia.fernandez@example.com",
            "+5493513333333",
            "Casamiento",
            SeedIds.ServicioCasamientos,
            now.AddMonths(3),
            "Villa Allende",
            120,
            "Quisiera consultar disponibilidad y presupuesto para cobertura de casamiento.",
            SolicitudPresupuestoEstados.Nuevo,
            now,
            cancellationToken);

        await UpsertSolicitudPresupuestoAsync(
            dbContext,
            SeedIds.SolicitudPresupuestoBook,
            "Martin Pereyra",
            "martin.pereyra@example.com",
            "+5493514444444",
            "Book personal",
            SeedIds.ServicioBookPersonal,
            now.AddDays(25),
            "Cordoba Capital",
            null,
            "Necesito fotos para marca personal y redes profesionales.",
            SolicitudPresupuestoEstados.Contactado,
            now,
            cancellationToken);

        await UpsertAgendaItemAsync(
            dbContext,
            SeedIds.AgendaReunionCliente,
            "Reunion con Lucia Fernandez",
            "Reunion inicial para revisar propuesta de casamiento.",
            AgendaItemTipos.Reunion,
            now.AddDays(2).Date.AddHours(14),
            now.AddDays(2).Date.AddHours(15),
            "Videollamada",
            AgendaItemEstados.Programado,
            null,
            null,
            clienteDemo.Id,
            solicitudCasamiento.Id,
            now,
            cancellationToken);

        await UpsertAgendaItemAsync(
            dbContext,
            SeedIds.AgendaSesionPrivadaDemo,
            "Sesion privada familiar",
            "Bloque reservado para sesion privada demo.",
            AgendaItemTipos.SesionPrivada,
            now.AddDays(5).Date.AddHours(10),
            now.AddDays(5).Date.AddHours(12),
            "Estudio",
            AgendaItemEstados.Confirmado,
            null,
            sesionPrivada.Id,
            clienteDemo.Id,
            null,
            now,
            cancellationToken);

        await UpsertAgendaItemAsync(
            dbContext,
            SeedIds.AgendaBloqueoFecha,
            "Bloqueo de fecha",
            "Fecha reservada para trabajo externo.",
            AgendaItemTipos.Bloqueo,
            now.AddDays(8).Date.AddHours(9),
            now.AddDays(8).Date.AddHours(18),
            null,
            AgendaItemEstados.Programado,
            null,
            null,
            null,
            null,
            now,
            cancellationToken);

        await SaveSeedChangesAsync(dbContext, logger, "Seed: solicitudes y agenda demo", cancellationToken);

        LogSeedBlock(logger, "Seed: gestion operativa demo");
        await UpsertPedidoEstadoHistorialAsync(
            dbContext,
            SeedIds.HistorialPedidoCasamientoPendiente,
            pedidoPendiente.Id,
            PedidoEstados.Pendiente,
            PedidoEstados.PendientePago,
            "Pedido demo pendiente de pago para seguimiento operativo.",
            admin.Id,
            now.AddMinutes(10),
            cancellationToken);

        await UpsertPedidoEstadoHistorialAsync(
            dbContext,
            SeedIds.HistorialPedidoCumpleanosPagado,
            pedidoPagado.Id,
            PedidoEstados.PendientePago,
            PedidoEstados.Pagado,
            "Pago demo aprobado y listo para generar descargas.",
            null,
            now.AddMinutes(20),
            cancellationToken);

        await UpsertNotaInternaAsync(
            dbContext,
            SeedIds.NotaInternaClienteDemo,
            NotaInternaTipos.Cliente,
            clienteDemo.Id,
            "Cliente demo para revisar historial completo, sesiones y preferencias.",
            admin.Id,
            now,
            cancellationToken);

        await UpsertNotaInternaAsync(
            dbContext,
            SeedIds.NotaInternaPedidoDemo,
            NotaInternaTipos.Pedido,
            pedidoPendiente.Id,
            "Pedido demo pendiente: contactar si no completa el pago.",
            admin.Id,
            now,
            cancellationToken);

        await UpsertNotaInternaAsync(
            dbContext,
            SeedIds.NotaInternaSolicitudDemo,
            NotaInternaTipos.SolicitudPresupuesto,
            solicitudCasamiento.Id,
            "Solicitud demo prioritaria para responder con disponibilidad.",
            admin.Id,
            now,
            cancellationToken);

        await SaveSeedChangesAsync(dbContext, logger, "Seed: gestion operativa demo", cancellationToken);

        LogSeedBlock(logger, "Seed: fotos privadas");
        await UpsertFotoPrivadaAsync(dbContext, SeedIds.FotoPrivadaClienteDemo001, sesionPrivada.Id, clienteDemo.Id, "privada-familiar-001.jpg", 2200m, now, cancellationToken);
        await UpsertFotoPrivadaAsync(dbContext, SeedIds.FotoPrivadaClienteDemo002, sesionPrivada.Id, clienteDemo.Id, "privada-familiar-002.jpg", 2200m, now, cancellationToken);

        await SaveSeedChangesAsync(dbContext, logger, "Seed: fotos privadas", cancellationToken);

        LogSeedBlock(logger, "Seed: ventas avanzadas fase 5");
        await UpsertCuponDescuentoAsync(dbContext, SeedIds.CuponBienvenida10, "BIENVENIDA10", "10% de bienvenida para primera compra.", CuponTipos.Porcentaje, 10m, 5000m, 8000m, now.AddMonths(-1), now.AddMonths(6), 300, 1, true, true, now, cancellationToken);
        await UpsertCuponDescuentoAsync(dbContext, SeedIds.CuponEvento15, "EVENTO15", "15% para compras de galerias de evento.", CuponTipos.Porcentaje, 15m, 10000m, 12000m, now.AddMonths(-1), now.AddMonths(4), 150, 2, false, true, now, cancellationToken);
        await UpsertCuponDescuentoAsync(dbContext, SeedIds.CuponSesion5000, "SESION5000", "Descuento fijo para sesiones privadas.", CuponTipos.MontoFijo, 5000m, 20000m, null, now.AddMonths(-1), now.AddMonths(5), 100, 1, false, true, now, cancellationToken);

        await UpsertPromocionAsync(dbContext, SeedIds.PromocionTemporada, "Temporada de eventos", "Promocion demo para eventos sociales publicados.", "https://placehold.co/1200x600?text=Eventos", PromocionTipos.Temporada, now.AddDays(-5), now.AddMonths(2), true, true, 1, null, SeedIds.ServicioCasamientos, null, now, cancellationToken);
        await UpsertPromocionAsync(dbContext, SeedIds.PromocionBienvenida, "Bienvenida 10", "Usa BIENVENIDA10 en tu primera compra.", "https://placehold.co/1200x600?text=Bienvenida10", PromocionTipos.Cupon, now.AddDays(-2), now.AddMonths(3), true, true, 2, SeedIds.CuponBienvenida10, null, null, now, cancellationToken);
        await UpsertPromocionAsync(dbContext, SeedIds.PromocionSesionPrivada, "Sesion privada destacada", "Beneficio para galerias y sesiones privadas.", "https://placehold.co/1200x600?text=Sesion+privada", PromocionTipos.Servicio, now.AddDays(-1), now.AddMonths(2), true, false, 3, SeedIds.CuponSesion5000, SeedIds.ServicioSesionesPrivadas, null, now, cancellationToken);

        await UpsertTestimonioAsync(dbContext, SeedIds.TestimonioSofia, "Sofia Martinez", "sofia.martinez@example.com", "La galeria fue facil de revisar y las descargas llegaron perfectas.", 5, true, true, SeedIds.ClienteSofia, SeedIds.PedidoBodaPagado, SeedIds.ServicioCasamientos, SeedIds.EventoBoda, now.AddDays(-8), cancellationToken);
        await UpsertTestimonioAsync(dbContext, SeedIds.TestimonioValentina, "Valentina Rios", "valentina.rios@example.com", "Nos encanto poder elegir fotos favoritas desde la galeria.", 5, true, true, SeedIds.ClienteValentina, null, SeedIds.ServicioCumpleanos, SeedIds.EventoQuince, now.AddDays(-6), cancellationToken);
        await UpsertTestimonioAsync(dbContext, SeedIds.TestimonioMateo, "Mateo Alvarez", "mateo.alvarez@example.com", "El material del workshop quedo profesional y listo para compartir.", 4, true, false, SeedIds.ClienteMateo, SeedIds.PedidoWorkshopPagado, SeedIds.ServicioCorporativo, SeedIds.EventoCorporativo, now.AddDays(-4), cancellationToken);
        await UpsertTestimonioAsync(dbContext, SeedIds.TestimonioClienteDemo, "Cliente Demo", "cliente@fotografia.com", "La sesion privada demo muestra muy bien el flujo de compra.", 5, false, false, clienteDemo.Id, null, SeedIds.ServicioSesionesPrivadas, null, now.AddDays(-1), cancellationToken);
        await SaveSeedChangesAsync(dbContext, logger, "Seed: ventas avanzadas fase 5", cancellationToken);
    }

    private static async Task<Usuario> UpsertUserAsync(
        AppDbContext dbContext,
        PasswordHasher<Usuario> passwordHasher,
        Guid id,
        string nombre,
        string email,
        string password,
        string rol,
        CancellationToken cancellationToken,
        string? descripcion = null,
        string? whatsApp = null,
        string? instagram = null,
        string? correoPublico = null,
        string? direccion = null)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var usuario = await dbContext.Usuarios.FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (usuario is null)
        {
            usuario = new Usuario
            {
                Id = id,
                Nombre = nombre,
                Email = normalizedEmail,
                PasswordHash = string.Empty,
                Rol = rol,
                Activo = true,
                CreadoEnUtc = DateTime.UtcNow
            };

            dbContext.Usuarios.Add(usuario);
        }

        usuario.Nombre = nombre;
        usuario.Email = normalizedEmail;
        usuario.Rol = rol;
        usuario.Activo = true;
        usuario.Descripcion = descripcion;
        usuario.WhatsApp = whatsApp;
        usuario.Instagram = instagram;
        usuario.CorreoPublico = correoPublico;
        usuario.Direccion = direccion;
        usuario.PasswordHash = passwordHasher.HashPassword(usuario, password);
        usuario.ActualizadoEnUtc = DateTime.UtcNow;

        return usuario;
    }

    private static async Task<Cliente> UpsertClienteAsync(
        AppDbContext dbContext,
        Guid id,
        Guid usuarioId,
        string nombre,
        string email,
        string telefono,
        string documento,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var cliente = await dbContext.Clientes
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail || x.UsuarioId == usuarioId, cancellationToken);

        if (cliente is null)
        {
            cliente = new Cliente
            {
                Id = id,
                Nombre = nombre,
                Email = normalizedEmail,
                CreadoEnUtc = DateTime.UtcNow
            };

            dbContext.Clientes.Add(cliente);
        }

        cliente.Nombre = nombre;
        cliente.Email = normalizedEmail;
        cliente.Telefono = telefono;
        cliente.Documento = documento;
        cliente.UsuarioId = usuarioId;
        cliente.ActualizadoEnUtc = DateTime.UtcNow;

        return cliente;
    }

    private static async Task<Evento> UpsertEventoAsync(
        AppDbContext dbContext,
        Guid id,
        string nombre,
        string descripcion,
        string slug,
        DateTime fechaEventoUtc,
        string estado,
        string visibilidad,
        Guid? clientePrincipalId,
        Guid adminId,
        CancellationToken cancellationToken)
    {
        var evento = await dbContext.Eventos.FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

        if (evento is null)
        {
            evento = new Evento
            {
                Id = id,
                Nombre = nombre,
                Slug = slug,
                FechaEventoUtc = fechaEventoUtc,
                CreadoEnUtc = DateTime.UtcNow
            };

            dbContext.Eventos.Add(evento);
        }

        evento.Nombre = nombre;
        evento.Descripcion = descripcion;
        evento.Slug = slug;
        evento.FechaEventoUtc = fechaEventoUtc;
        evento.Estado = estado;
        evento.Visibilidad = visibilidad;
        evento.Activo = estado != EventoEstados.Archivado;
        evento.ClientePrincipalId = clientePrincipalId;
        evento.CreadoPorUsuarioId = adminId;
        evento.ActualizadoEnUtc = DateTime.UtcNow;

        return evento;
    }

    private static async Task<Foto> UpsertDemoFotoAsync(
        AppDbContext dbContext,
        Guid id,
        Guid eventoId,
        string nombreArchivo,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var storageKey = $"eventos/{eventoId:N}/originales/{nombreArchivo}";
        var foto = await dbContext.Fotos
            .FirstOrDefaultAsync(x => x.EventoId == eventoId && x.StorageKey == storageKey, cancellationToken);

        if (foto is null)
        {
            foto = new Foto
            {
                Id = id,
                EventoId = eventoId,
                NombreArchivo = nombreArchivo,
                ContentType = "image/jpeg",
                StorageKey = storageKey
            };

            dbContext.Fotos.Add(foto);
        }

        foto.NombreArchivo = nombreArchivo;
        foto.ContentType = "image/jpeg";
        foto.StorageKey = storageKey;
        foto.PreviewUrl = $"https://placehold.co/600x400?text={nombreArchivo}";
        foto.MarcaAguaStorageKey = $"eventos/{eventoId:N}/watermark/{nombreArchivo}";
        foto.SizeInBytes = 5000000;
        foto.Width = 3000;
        foto.Height = 2000;
        foto.PrecioUnitario = 1500m;
        foto.TieneMarcaAgua = true;
        foto.Procesada = true;
        foto.Activa = true;
        foto.Destacado = nombreArchivo.Contains("001", StringComparison.OrdinalIgnoreCase);
        foto.OrdenDestacado = foto.Destacado ? 1 : null;
        foto.SubidaEnUtc = now;
        foto.FechaActualizacionUtc = now;

        return foto;
    }

    private static async Task<Pedido> UpsertPedidoAsync(
        AppDbContext dbContext,
        Guid id,
        Guid clienteId,
        Guid eventoId,
        string estado,
        string moneda,
        IReadOnlyCollection<Foto> fotos,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var pedido = await dbContext.Pedidos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (pedido is null)
        {
            pedido = new Pedido
            {
                Id = id,
                ClienteId = clienteId,
                EventoId = eventoId,
                CreadoEnUtc = now
            };

            dbContext.Pedidos.Add(pedido);
        }

        pedido.ClienteId = clienteId;
        pedido.EventoId = eventoId;
        pedido.Estado = estado;
        pedido.Moneda = moneda;
        pedido.Subtotal = fotos.Sum(x => x.PrecioUnitario);
        pedido.DescuentoTotal = 0;
        pedido.Total = pedido.Subtotal;
        pedido.ActualizadoEnUtc = now;

        var fotoIds = fotos.Select(x => x.Id).ToHashSet();
        var pedidoFotos = await dbContext.PedidoFotos
            .Where(x => x.PedidoId == id && fotoIds.Contains(x.FotoId))
            .ToListAsync(cancellationToken);

        foreach (var foto in fotos)
        {
            var item = pedidoFotos.FirstOrDefault(x => x.FotoId == foto.Id);
            if (item is null)
            {
                item = new PedidoFoto
                {
                    PedidoId = id,
                    FotoId = foto.Id,
                    Cantidad = 1,
                    PrecioUnitario = foto.PrecioUnitario
                };

                dbContext.PedidoFotos.Add(item);
                pedidoFotos.Add(item);
            }
            else
            {
                item.Cantidad = 1;
                item.PrecioUnitario = foto.PrecioUnitario;
            }
        }

        var pedidoItems = await dbContext.PedidoItems
            .Where(x => x.PedidoId == id && x.TipoItem == PedidoItemTipos.FotoEvento && x.FotoId != null && fotoIds.Contains(x.FotoId.Value))
            .ToListAsync(cancellationToken);

        foreach (var foto in fotos)
        {
            var item = pedidoItems.FirstOrDefault(x => x.FotoId == foto.Id);
            if (item is null)
            {
                item = new PedidoItem
                {
                    PedidoId = id,
                    TipoItem = PedidoItemTipos.FotoEvento,
                    FotoId = foto.Id,
                    Descripcion = foto.NombreArchivo,
                    Cantidad = 1,
                    PrecioUnitario = foto.PrecioUnitario,
                    Subtotal = foto.PrecioUnitario,
                    FechaCreacionUtc = now
                };

                dbContext.PedidoItems.Add(item);
                pedidoItems.Add(item);
            }
            else
            {
                item.Descripcion = foto.NombreArchivo;
                item.Cantidad = 1;
                item.PrecioUnitario = foto.PrecioUnitario;
                item.Subtotal = foto.PrecioUnitario;
            }
        }

        return pedido;
    }

    private static async Task UpsertPerfilFotografaAsync(
        AppDbContext dbContext,
        Guid id,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var perfil = await dbContext.PerfilesFotografa.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (perfil is null)
        {
            perfil = new PerfilFotografa
            {
                Id = id,
                Nombre = "Carlos Cornejo Moscoso",
                FechaCreacionUtc = now
            };

            dbContext.PerfilesFotografa.Add(perfil);
        }

        var otrosActivos = await dbContext.PerfilesFotografa
            .Where(x => x.Id != id && x.Activa)
            .ToListAsync(cancellationToken);

        foreach (var otro in otrosActivos)
        {
            otro.Activa = false;
            otro.FechaActualizacionUtc = now;
        }

        perfil.Nombre = "Carlos Cornejo Moscoso";
        perfil.Titulo = "Fotografia social y eventos";
        perfil.Descripcion = "Fotografa especializada en eventos sociales, books y sesiones privadas.";
        perfil.Biografia = "Acompano cada historia con una mirada documental, cuidando la seleccion y entrega digital de cada galeria.";
        perfil.WhatsApp = "+5493510000000";
        perfil.Instagram = "@fotografia.demo";
        perfil.Facebook = "fotografia.demo";
        perfil.TikTok = "@fotografia.demo";
        perfil.SitioWeb = "https://fotografia.example.com";
        perfil.CorreoPublico = "contacto@fotografia.example.com";
        perfil.Direccion = "Centro";
        perfil.Ciudad = "Cordoba";
        perfil.Provincia = "Cordoba";
        perfil.Pais = "Argentina";
        perfil.FotoPerfilUrl = "https://placehold.co/600x600?text=Fotografa";
        perfil.LogoUrl = "https://placehold.co/400x160?text=Logo";
        perfil.BannerUrl = "https://placehold.co/1400x500?text=Fotografia";
        perfil.TextoBienvenida = "Bienvenido a tu galeria de fotos.";
        perfil.Activa = true;
        perfil.FechaActualizacionUtc = now;
    }

    private static async Task UpsertServicioFotografiaAsync(
        AppDbContext dbContext,
        Guid id,
        string nombre,
        string descripcion,
        decimal? precioDesde,
        string? duracionEstimada,
        int? cantidadFotosIncluidas,
        string imagenUrl,
        int orden,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var servicio = await dbContext.ServiciosFotografia.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (servicio is null)
        {
            servicio = new ServicioFotografia
            {
                Id = id,
                Nombre = nombre,
                FechaCreacionUtc = now
            };

            dbContext.ServiciosFotografia.Add(servicio);
        }

        servicio.Nombre = nombre;
        servicio.Descripcion = descripcion;
        servicio.PrecioDesde = precioDesde;
        servicio.DuracionEstimada = duracionEstimada;
        servicio.CantidadFotosIncluidas = cantidadFotosIncluidas;
        servicio.ImagenUrl = imagenUrl;
        servicio.Activo = true;
        servicio.Destacado = orden <= 3;
        servicio.OrdenDestacado = orden <= 3 ? orden : null;
        servicio.Orden = orden;
        servicio.FechaActualizacionUtc = now;
    }

    private static async Task UpsertPortfolioItemAsync(
        AppDbContext dbContext,
        Guid id,
        string titulo,
        string descripcion,
        string imagenUrl,
        string categoria,
        int orden,
        bool destacado,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var item = await dbContext.PortfolioItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null)
        {
            item = new PortfolioItem
            {
                Id = id,
                Titulo = titulo,
                FechaCreacionUtc = now
            };

            dbContext.PortfolioItems.Add(item);
        }

        item.Titulo = titulo;
        item.Descripcion = descripcion;
        item.ImagenUrl = imagenUrl;
        item.Categoria = categoria;
        item.Orden = orden;
        item.Destacado = destacado;
        item.Activo = true;
        item.FechaActualizacionUtc = now;
    }

    private static async Task UpsertPreguntaFrecuenteAsync(
        AppDbContext dbContext,
        Guid id,
        string preguntaTexto,
        string respuesta,
        string categoria,
        int orden,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var pregunta = await dbContext.PreguntasFrecuentes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (pregunta is null)
        {
            pregunta = new PreguntaFrecuente
            {
                Id = id,
                Pregunta = preguntaTexto,
                Respuesta = respuesta,
                FechaCreacionUtc = now
            };

            dbContext.PreguntasFrecuentes.Add(pregunta);
        }

        pregunta.Pregunta = preguntaTexto;
        pregunta.Respuesta = respuesta;
        pregunta.Categoria = categoria;
        pregunta.Orden = orden;
        pregunta.Activa = true;
        pregunta.FechaActualizacionUtc = now;
    }

    private static async Task<SolicitudPresupuesto> UpsertSolicitudPresupuestoAsync(
        AppDbContext dbContext,
        Guid id,
        string nombre,
        string email,
        string whatsApp,
        string tipoEvento,
        Guid? servicioId,
        DateTime? fechaTentativaUtc,
        string lugar,
        int? cantidadInvitados,
        string mensaje,
        string estado,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var solicitud = await dbContext.SolicitudesPresupuesto.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (solicitud is null)
        {
            solicitud = new SolicitudPresupuesto
            {
                Id = id,
                Nombre = nombre,
                Email = normalizedEmail,
                Mensaje = mensaje,
                FechaCreacionUtc = now
            };

            dbContext.SolicitudesPresupuesto.Add(solicitud);
        }

        solicitud.Nombre = nombre;
        solicitud.Email = normalizedEmail;
        solicitud.WhatsApp = whatsApp;
        solicitud.TipoEvento = tipoEvento;
        solicitud.ServicioId = servicioId;
        solicitud.FechaTentativaUtc = fechaTentativaUtc;
        solicitud.Lugar = lugar;
        solicitud.CantidadInvitados = cantidadInvitados;
        solicitud.Mensaje = mensaje;
        solicitud.Estado = estado;
        solicitud.Activa = true;
        solicitud.FechaActualizacionUtc = now;

        return solicitud;
    }

    private static async Task UpsertAgendaItemAsync(
        AppDbContext dbContext,
        Guid id,
        string titulo,
        string descripcion,
        string tipo,
        DateTime fechaInicioUtc,
        DateTime fechaFinUtc,
        string? ubicacion,
        string estado,
        Guid? eventoId,
        Guid? sesionPrivadaId,
        Guid? clienteId,
        Guid? solicitudPresupuestoId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var item = await dbContext.AgendaItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null)
        {
            item = new AgendaItem
            {
                Id = id,
                Titulo = titulo,
                FechaCreacionUtc = now
            };

            dbContext.AgendaItems.Add(item);
        }

        item.Titulo = titulo;
        item.Descripcion = descripcion;
        item.Tipo = tipo;
        item.FechaInicioUtc = fechaInicioUtc;
        item.FechaFinUtc = fechaFinUtc;
        item.Ubicacion = ubicacion;
        item.Estado = estado;
        item.EventoId = eventoId;
        item.SesionPrivadaId = sesionPrivadaId;
        item.ClienteId = clienteId;
        item.SolicitudPresupuestoId = solicitudPresupuestoId;
        item.Activo = true;
        item.FechaActualizacionUtc = now;
    }

    private static async Task UpsertPedidoEstadoHistorialAsync(
        AppDbContext dbContext,
        Guid id,
        Guid pedidoId,
        string estadoAnterior,
        string estadoNuevo,
        string comentario,
        Guid? usuarioId,
        DateTime fechaCambioUtc,
        CancellationToken cancellationToken)
    {
        var historial = await dbContext.PedidoEstadoHistorial.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (historial is null)
        {
            historial = new PedidoEstadoHistorial
            {
                Id = id,
                PedidoId = pedidoId,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = estadoNuevo,
                FechaCambioUtc = fechaCambioUtc
            };

            dbContext.PedidoEstadoHistorial.Add(historial);
        }

        historial.PedidoId = pedidoId;
        historial.EstadoAnterior = estadoAnterior;
        historial.EstadoNuevo = estadoNuevo;
        historial.Comentario = comentario;
        historial.UsuarioId = usuarioId;
        historial.FechaCambioUtc = fechaCambioUtc;
    }

    private static async Task UpsertNotaInternaAsync(
        AppDbContext dbContext,
        Guid id,
        string entidadTipo,
        Guid entidadId,
        string texto,
        Guid usuarioId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var nota = await dbContext.NotasInternas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (nota is null)
        {
            nota = new NotaInterna
            {
                Id = id,
                EntidadTipo = entidadTipo,
                EntidadId = entidadId,
                Texto = texto,
                UsuarioId = usuarioId,
                FechaCreacionUtc = now
            };

            dbContext.NotasInternas.Add(nota);
        }

        nota.EntidadTipo = entidadTipo;
        nota.EntidadId = entidadId;
        nota.Texto = texto;
        nota.UsuarioId = usuarioId;
        nota.Activa = true;
        nota.FechaActualizacionUtc = now;
    }

    private static async Task UpsertPaqueteEventoAsync(
        AppDbContext dbContext,
        Guid id,
        Guid eventoId,
        string nombre,
        string descripcion,
        decimal precio,
        bool incluyeTodasLasFotos,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var paquete = await dbContext.PaquetesEvento.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (paquete is null)
        {
            paquete = new PaqueteEvento
            {
                Id = id,
                EventoId = eventoId,
                Nombre = nombre,
                FechaCreacionUtc = now
            };

            dbContext.PaquetesEvento.Add(paquete);
        }

        paquete.EventoId = eventoId;
        paquete.Nombre = nombre;
        paquete.Descripcion = descripcion;
        paquete.Precio = precio;
        paquete.IncluyeTodasLasFotos = incluyeTodasLasFotos;
        paquete.Activo = true;
        paquete.Destacado = incluyeTodasLasFotos;
        paquete.OrdenDestacado = incluyeTodasLasFotos ? 1 : null;
        paquete.FechaActualizacionUtc = now;
    }

    private static async Task<SesionPrivada> UpsertSesionPrivadaAsync(
        AppDbContext dbContext,
        Guid id,
        Guid clienteId,
        string titulo,
        string descripcion,
        DateTime fechaSesionUtc,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var sesion = await dbContext.SesionesPrivadas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (sesion is null)
        {
            sesion = new SesionPrivada
            {
                Id = id,
                ClienteId = clienteId,
                Titulo = titulo,
                FechaCreacionUtc = now
            };

            dbContext.SesionesPrivadas.Add(sesion);
        }

        sesion.ClienteId = clienteId;
        sesion.Titulo = titulo;
        sesion.Descripcion = descripcion;
        sesion.FechaSesionUtc = fechaSesionUtc;
        sesion.Estado = SesionPrivadaEstados.Programada;
        sesion.PrecioPaquete = 8500m;
        sesion.Activa = true;
        sesion.FechaActualizacionUtc = now;

        return sesion;
    }

    private static async Task UpsertFotoPrivadaAsync(
        AppDbContext dbContext,
        Guid id,
        Guid sesionPrivadaId,
        Guid clienteId,
        string nombreArchivo,
        decimal precioUnitario,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var storageKey = $"privadas/{sesionPrivadaId:N}/fotos/{nombreArchivo}";
        var foto = await dbContext.FotosPrivadas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (foto is null)
        {
            foto = new FotoPrivada
            {
                Id = id,
                SesionPrivadaId = sesionPrivadaId,
                ClienteId = clienteId,
                NombreArchivo = nombreArchivo,
                ContentType = "image/jpeg",
                StorageKey = storageKey,
                FechaCreacionUtc = now
            };

            dbContext.FotosPrivadas.Add(foto);
        }

        foto.SesionPrivadaId = sesionPrivadaId;
        foto.ClienteId = clienteId;
        foto.NombreArchivo = nombreArchivo;
        foto.ContentType = "image/jpeg";
        foto.StorageKey = storageKey;
        foto.PreviewUrl = $"https://placehold.co/600x400?text={nombreArchivo}";
        foto.MarcaAguaStorageKey = $"privadas/{sesionPrivadaId:N}/watermarks/{nombreArchivo}";
        foto.SizeInBytes = 4000000;
        foto.Width = 3000;
        foto.Height = 2000;
        foto.PrecioUnitario = precioUnitario;
        foto.Activa = true;
        foto.FechaActualizacionUtc = now;
    }

    private static async Task UpsertCuponDescuentoAsync(
        AppDbContext dbContext,
        Guid id,
        string codigo,
        string descripcion,
        string tipoDescuento,
        decimal valorDescuento,
        decimal? montoMinimoCompra,
        decimal? montoMaximoDescuento,
        DateTime? fechaInicioUtc,
        DateTime? fechaFinUtc,
        int? usosMaximos,
        int? usosMaximosPorUsuario,
        bool soloPrimerCompra,
        bool activo,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var normalizedCodigo = codigo.Trim().ToUpperInvariant();
        var cupon = await dbContext.CuponesDescuento.FirstOrDefaultAsync(x => x.Codigo == normalizedCodigo, cancellationToken);
        if (cupon is null)
        {
            cupon = new CuponDescuento
            {
                Id = id,
                Codigo = normalizedCodigo,
                TipoDescuento = tipoDescuento,
                FechaCreacionUtc = now
            };

            dbContext.CuponesDescuento.Add(cupon);
        }

        cupon.Codigo = normalizedCodigo;
        cupon.Descripcion = descripcion;
        cupon.TipoDescuento = tipoDescuento;
        cupon.ValorDescuento = valorDescuento;
        cupon.MontoMinimoCompra = montoMinimoCompra;
        cupon.MontoMaximoDescuento = montoMaximoDescuento;
        cupon.FechaInicioUtc = fechaInicioUtc;
        cupon.FechaFinUtc = fechaFinUtc;
        cupon.UsosMaximos = usosMaximos;
        cupon.UsosMaximosPorUsuario = usosMaximosPorUsuario;
        cupon.SoloPrimerCompra = soloPrimerCompra;
        cupon.Activo = activo;
        cupon.FechaActualizacionUtc = now;
    }

    private static async Task UpsertPromocionAsync(
        AppDbContext dbContext,
        Guid id,
        string titulo,
        string descripcion,
        string imagenUrl,
        string tipo,
        DateTime? fechaInicioUtc,
        DateTime? fechaFinUtc,
        bool activa,
        bool destacada,
        int orden,
        Guid? cuponDescuentoId,
        Guid? servicioFotografiaId,
        Guid? eventoId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var promocion = await dbContext.Promociones.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (promocion is null)
        {
            promocion = new Promocion
            {
                Id = id,
                Titulo = titulo,
                Tipo = tipo,
                FechaCreacionUtc = now
            };

            dbContext.Promociones.Add(promocion);
        }

        promocion.Titulo = titulo;
        promocion.Descripcion = descripcion;
        promocion.ImagenUrl = imagenUrl;
        promocion.Tipo = tipo;
        promocion.FechaInicioUtc = fechaInicioUtc;
        promocion.FechaFinUtc = fechaFinUtc;
        promocion.Activa = activa;
        promocion.Destacada = destacada;
        promocion.Orden = orden;
        promocion.CuponDescuentoId = cuponDescuentoId;
        promocion.ServicioFotografiaId = servicioFotografiaId;
        promocion.EventoId = eventoId;
        promocion.FechaActualizacionUtc = now;
    }

    private static async Task UpsertTestimonioAsync(
        AppDbContext dbContext,
        Guid id,
        string nombreCliente,
        string emailCliente,
        string texto,
        int calificacion,
        bool publicado,
        bool destacado,
        Guid? clienteId,
        Guid? pedidoId,
        Guid? servicioFotografiaId,
        Guid? eventoId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var testimonio = await dbContext.Testimonios.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (testimonio is null)
        {
            testimonio = new Testimonio
            {
                Id = id,
                NombreCliente = nombreCliente,
                Texto = texto,
                FechaCreacionUtc = now
            };

            dbContext.Testimonios.Add(testimonio);
        }

        testimonio.NombreCliente = nombreCliente;
        testimonio.EmailCliente = emailCliente.Trim().ToLowerInvariant();
        testimonio.Texto = texto;
        testimonio.Calificacion = calificacion;
        testimonio.ImagenUrl = $"https://placehold.co/400x400?text={Uri.EscapeDataString(nombreCliente)}";
        testimonio.Publicado = publicado;
        testimonio.Destacado = destacado;
        testimonio.Activo = true;
        testimonio.ClienteId = clienteId;
        testimonio.PedidoId = pedidoId;
        testimonio.ServicioFotografiaId = servicioFotografiaId;
        testimonio.EventoId = eventoId;
        testimonio.FechaActualizacionUtc = now;
    }

    private static async Task UpsertPagoAsync(
        AppDbContext dbContext,
        Guid id,
        Pedido pedido,
        string estado,
        string paymentId,
        string preferenceId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var trackedPedido = await dbContext.Pedidos.FirstOrDefaultAsync(x => x.Id == pedido.Id, cancellationToken);
        if (trackedPedido is null)
        {
            return;
        }

        var pago = await dbContext.Pagos.FirstOrDefaultAsync(x => x.PedidoId == pedido.Id || x.Id == id, cancellationToken);

        if (pago is null)
        {
            pago = new Pago
            {
                Id = id,
                PedidoId = pedido.Id,
                CreadoEnUtc = now
            };

            dbContext.Pagos.Add(pago);
        }

        pago.PedidoId = trackedPedido.Id;
        pago.Estado = estado;
        pago.Monto = trackedPedido.Total;
        pago.Moneda = trackedPedido.Moneda;
        pago.MercadoPagoPaymentId = paymentId;
        pago.MercadoPagoPreferenceId = preferenceId;
        pago.PagadoEnUtc = now;
        pago.ActualizadoEnUtc = now;
        trackedPedido.MercadoPagoPreferenceId = preferenceId;
        trackedPedido.ActualizadoEnUtc = now;
    }

    private static async Task UpsertDescargaAsync(
        AppDbContext dbContext,
        Guid id,
        Pedido pedido,
        Guid clienteId,
        Guid eventoId,
        Foto foto,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var descarga = await dbContext.Descargas
            .FirstOrDefaultAsync(x => x.Id == id || (x.PedidoId == pedido.Id && x.FotoId == foto.Id), cancellationToken);

        if (descarga is null)
        {
            descarga = new Descarga
            {
                Id = id,
                PedidoId = pedido.Id,
                EventoId = eventoId,
                ClienteId = clienteId,
                FotoId = foto.Id,
                StorageKey = foto.StorageKey,
                NombreArchivo = foto.NombreArchivo,
                CreadoEnUtc = now
            };

            dbContext.Descargas.Add(descarga);
        }

        descarga.PedidoId = pedido.Id;
        descarga.EventoId = eventoId;
        descarga.ClienteId = clienteId;
        descarga.FotoId = foto.Id;
        descarga.FotoPrivadaId = null;
        descarga.StorageKey = foto.StorageKey;
        descarga.NombreArchivo = foto.NombreArchivo;
        descarga.ExpiraEnUtc = now.AddDays(7);
        descarga.MaxDescargas = 5;
        descarga.DescargasRealizadas = 0;
        descarga.UltimaDescargaUtc = null;
        descarga.Activa = true;
        descarga.FechaActualizacionUtc = now;
    }

    private static async Task UpsertComentarioEventoAsync(
        AppDbContext dbContext,
        Guid id,
        Guid eventoId,
        Guid usuarioId,
        string texto,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var comentario = await dbContext.ComentariosEventos
            .FirstOrDefaultAsync(x => x.EventoId == eventoId && x.UsuarioId == usuarioId && x.Texto == texto, cancellationToken);

        if (comentario is null)
        {
            dbContext.ComentariosEventos.Add(new ComentarioEvento
            {
                Id = id,
                EventoId = eventoId,
                UsuarioId = usuarioId,
                Texto = texto,
                FechaCreacionUtc = now,
                Activo = true
            });
            return;
        }

        comentario.Activo = true;
    }

    private static async Task UpsertComentarioFotoAsync(
        AppDbContext dbContext,
        Guid id,
        Guid fotoId,
        Guid usuarioId,
        string texto,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var comentario = await dbContext.ComentariosFotos
            .FirstOrDefaultAsync(x => x.FotoId == fotoId && x.UsuarioId == usuarioId && x.Texto == texto, cancellationToken);

        if (comentario is null)
        {
            dbContext.ComentariosFotos.Add(new ComentarioFoto
            {
                Id = id,
                FotoId = fotoId,
                UsuarioId = usuarioId,
                Texto = texto,
                FechaCreacionUtc = now,
                Activo = true
            });
            return;
        }

        comentario.Activo = true;
    }

    private static async Task UpsertEventoFavoritoAsync(
        AppDbContext dbContext,
        Guid id,
        Guid usuarioId,
        Guid eventoId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.EventosFavoritos
            .AnyAsync(x => x.UsuarioId == usuarioId && x.EventoId == eventoId, cancellationToken);

        if (!exists)
        {
            dbContext.EventosFavoritos.Add(new EventoFavorito
            {
                Id = id,
                UsuarioId = usuarioId,
                EventoId = eventoId,
                FechaCreacionUtc = now
            });
        }
    }

    private static async Task UpsertFotoFavoritaAsync(
        AppDbContext dbContext,
        Guid id,
        Guid usuarioId,
        Guid fotoId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.FotosFavoritas
            .AnyAsync(x => x.UsuarioId == usuarioId && x.FotoId == fotoId, cancellationToken);

        if (!exists)
        {
            dbContext.FotosFavoritas.Add(new FotoFavorita
            {
                Id = id,
                UsuarioId = usuarioId,
                FotoId = fotoId,
                FechaCreacionUtc = now
            });
        }
    }

    private static Foto CreateFoto(
        Guid id,
        Guid eventoId,
        string nombreArchivo,
        string contentType,
        decimal precioUnitario,
        long sizeInBytes,
        int width,
        int height)
    {
        var storageKey = $"eventos/{eventoId:N}/fotos/{nombreArchivo}";

        return new Foto
        {
            Id = id,
            EventoId = eventoId,
            NombreArchivo = nombreArchivo,
            ContentType = contentType,
            StorageKey = storageKey,
            PreviewUrl = $"https://cdn.example.com/previews/{nombreArchivo}",
            MarcaAguaStorageKey = $"eventos/{eventoId:N}/watermarks/{nombreArchivo}",
            SizeInBytes = sizeInBytes,
            Width = width,
            Height = height,
            PrecioUnitario = precioUnitario,
            TieneMarcaAgua = true,
            Procesada = true,
            Activa = true,
            Destacado = nombreArchivo.Contains("001", StringComparison.OrdinalIgnoreCase),
            OrdenDestacado = nombreArchivo.Contains("001", StringComparison.OrdinalIgnoreCase) ? 1 : null,
            SubidaEnUtc = SeedClock.Now.AddDays(-10)
        };
    }

    private static void ApplySeedFoto(Foto target, Foto source)
    {
        target.EventoId = source.EventoId;
        target.NombreArchivo = source.NombreArchivo;
        target.ContentType = source.ContentType;
        target.StorageKey = source.StorageKey;
        target.PreviewUrl = source.PreviewUrl;
        target.MarcaAguaStorageKey = source.MarcaAguaStorageKey;
        target.SizeInBytes = source.SizeInBytes;
        target.Width = source.Width;
        target.Height = source.Height;
        target.PrecioUnitario = source.PrecioUnitario;
        target.TieneMarcaAgua = source.TieneMarcaAgua;
        target.Procesada = source.Procesada;
        target.Activa = source.Activa;
        target.Destacado = source.Destacado;
        target.OrdenDestacado = source.OrdenDestacado;
        target.SubidaEnUtc = source.SubidaEnUtc;
        target.FechaActualizacionUtc = SeedClock.Now;
    }

    private static async Task SetPortadaAsync(
        AppDbContext dbContext,
        Guid eventoId,
        Guid fotoId,
        CancellationToken cancellationToken)
    {
        var evento = await dbContext.Eventos.FirstOrDefaultAsync(x => x.Id == eventoId, cancellationToken);
        if (evento is null)
        {
            return;
        }

        var fotoBelongsToEvento = await dbContext.Fotos.AnyAsync(
            x => x.Id == fotoId && x.EventoId == eventoId && x.Activa,
            cancellationToken);

        if (!fotoBelongsToEvento)
        {
            return;
        }

        evento.PortadaFotoId = fotoId;
        evento.ActualizadoEnUtc = SeedClock.Now;
    }

    private static void LogSeedBlock(ILogger? logger, string block)
    {
        logger?.LogInformation("{SeedBlock}", block);
    }

    private static async Task SaveSeedChangesAsync(
        AppDbContext dbContext,
        ILogger? logger,
        string block,
        CancellationToken cancellationToken)
    {
        var entries = dbContext.ChangeTracker.Entries()
            .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        var added = entries.Count(x => x.State == EntityState.Added);
        var modified = entries.Count(x => x.State == EntityState.Modified);
        var deleted = entries.Count(x => x.State == EntityState.Deleted);

        logger?.LogInformation(
            "{SeedBlock}: SaveChanges Added={Added} Modified={Modified} Deleted={Deleted}",
            block,
            added,
            modified,
            deleted);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            LogConcurrencyException(logger, block, ex);
            throw;
        }
    }

    private static void LogConcurrencyException(
        ILogger? logger,
        string block,
        DbUpdateConcurrencyException exception)
    {
        if (logger is null)
        {
            return;
        }

        logger.LogError(
            exception,
            "{SeedBlock}: DbUpdateConcurrencyException durante seed. Entries={EntryCount}",
            block,
            exception.Entries.Count);

        foreach (var entry in exception.Entries)
        {
            logger.LogError(
                "{SeedBlock}: entidad={EntityType} estado={State} pk={PrimaryKey} valores={CurrentValues}",
                block,
                entry.Metadata.ClrType.Name,
                entry.State,
                FormatPrimaryKey(entry),
                FormatCurrentValues(entry));
        }
    }

    private static string FormatPrimaryKey(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();
        if (key is null)
        {
            return "<sin-pk>";
        }

        return string.Join(", ", key.Properties.Select(property =>
        {
            var value = entry.Property(property.Name).CurrentValue;
            return $"{property.Name}={value}";
        }));
    }

    private static string FormatCurrentValues(EntityEntry entry)
    {
        var values = entry.Properties
            .Where(property => !IsSensitiveProperty(property.Metadata.Name))
            .Select(property => $"{property.Metadata.Name}={property.CurrentValue}");

        return string.Join(", ", values);
    }

    private static bool IsSensitiveProperty(string propertyName)
    {
        return propertyName.Contains("Password", StringComparison.OrdinalIgnoreCase)
            || propertyName.Contains("Token", StringComparison.OrdinalIgnoreCase)
            || propertyName.Contains("Secret", StringComparison.OrdinalIgnoreCase);
    }

    private static Descarga CreateDescarga(
        Guid id,
        Guid pedidoId,
        Guid eventoId,
        Guid clienteId,
        Guid fotoId,
        string nombreArchivo)
    {
        var now = DateTime.UtcNow;

        return new Descarga
        {
            Id = id,
            PedidoId = pedidoId,
            EventoId = eventoId,
            ClienteId = clienteId,
            FotoId = fotoId,
            StorageKey = $"eventos/{eventoId:N}/fotos/{nombreArchivo}",
            NombreArchivo = nombreArchivo,
            ExpiraEnUtc = now.AddDays(7),
            CreadoEnUtc = now,
            FechaActualizacionUtc = now,
            MaxDescargas = 5,
            DescargasRealizadas = 0,
            UltimaDescargaUtc = null,
            Activa = true
        };
    }

    private static PlantillaNotificacion CreatePlantilla(
        string codigo,
        string canal,
        string asunto,
        string cuerpoHtml,
        string cuerpoTexto)
    {
        return new PlantillaNotificacion
        {
            Codigo = codigo,
            Canal = canal,
            Asunto = asunto,
            CuerpoHtml = cuerpoHtml,
            CuerpoTexto = cuerpoTexto,
            Activa = true,
            FechaCreacionUtc = SeedClock.Now
        };
    }

    private static void ApplySeedDescarga(Descarga target, Descarga source)
    {
        target.PedidoId = source.PedidoId;
        target.EventoId = source.EventoId;
        target.ClienteId = source.ClienteId;
        target.FotoId = source.FotoId;
        target.FotoPrivadaId = source.FotoPrivadaId;
        target.StorageKey = source.StorageKey;
        target.NombreArchivo = source.NombreArchivo;
        target.ExpiraEnUtc = source.ExpiraEnUtc;
        target.MaxDescargas = 5;
        target.DescargasRealizadas = 0;
        target.UltimaDescargaUtc = null;
        target.Activa = true;
        target.FechaActualizacionUtc = DateTime.UtcNow;
    }

    private static class SeedClock
    {
        public static readonly DateTime Now = new(2026, 5, 30, 18, 45, 0, DateTimeKind.Utc);
    }

    private static class SeedIds
    {
        public static readonly Guid AdminUsuario = Guid.Parse("10000000-0000-0000-0000-000000000001");
        public static readonly Guid ClienteSofia = Guid.Parse("20000000-0000-0000-0000-000000000001");
        public static readonly Guid ClienteValentina = Guid.Parse("20000000-0000-0000-0000-000000000002");
        public static readonly Guid ClienteMateo = Guid.Parse("20000000-0000-0000-0000-000000000003");
        public static readonly Guid EventoBoda = Guid.Parse("30000000-0000-0000-0000-000000000001");
        public static readonly Guid EventoQuince = Guid.Parse("30000000-0000-0000-0000-000000000002");
        public static readonly Guid EventoCorporativo = Guid.Parse("30000000-0000-0000-0000-000000000003");
        public static readonly Guid FotoBoda01 = Guid.Parse("40000000-0000-0000-0000-000000000001");
        public static readonly Guid FotoBoda02 = Guid.Parse("40000000-0000-0000-0000-000000000002");
        public static readonly Guid FotoBoda03 = Guid.Parse("40000000-0000-0000-0000-000000000003");
        public static readonly Guid FotoBoda04 = Guid.Parse("40000000-0000-0000-0000-000000000004");
        public static readonly Guid FotoQuince01 = Guid.Parse("40000000-0000-0000-0000-000000000005");
        public static readonly Guid FotoQuince02 = Guid.Parse("40000000-0000-0000-0000-000000000006");
        public static readonly Guid FotoQuince03 = Guid.Parse("40000000-0000-0000-0000-000000000007");
        public static readonly Guid FotoWorkshop01 = Guid.Parse("40000000-0000-0000-0000-000000000008");
        public static readonly Guid FotoWorkshop02 = Guid.Parse("40000000-0000-0000-0000-000000000009");
        public static readonly Guid FotoWorkshop03 = Guid.Parse("40000000-0000-0000-0000-000000000010");
        public static readonly Guid PedidoBodaPagado = Guid.Parse("50000000-0000-0000-0000-000000000001");
        public static readonly Guid PedidoQuincePendiente = Guid.Parse("50000000-0000-0000-0000-000000000002");
        public static readonly Guid PedidoWorkshopPagado = Guid.Parse("50000000-0000-0000-0000-000000000003");
        public static readonly Guid PagoBoda = Guid.Parse("60000000-0000-0000-0000-000000000001");
        public static readonly Guid PagoWorkshop = Guid.Parse("60000000-0000-0000-0000-000000000002");
        public static readonly Guid DescargaBoda01 = Guid.Parse("70000000-0000-0000-0000-000000000001");
        public static readonly Guid DescargaBoda02 = Guid.Parse("70000000-0000-0000-0000-000000000002");
        public static readonly Guid DescargaWorkshop01 = Guid.Parse("70000000-0000-0000-0000-000000000003");
        public static readonly Guid AdminDemo = Guid.Parse("10000000-0000-0000-0000-000000000002");
        public static readonly Guid UsuarioClienteDemo = Guid.Parse("10000000-0000-0000-0000-000000000003");
        public static readonly Guid UsuarioInvitadoDemo = Guid.Parse("10000000-0000-0000-0000-000000000004");
        public static readonly Guid ClienteDemo = Guid.Parse("20000000-0000-0000-0000-000000000101");
        public static readonly Guid ClienteInvitadoDemo = Guid.Parse("20000000-0000-0000-0000-000000000102");
        public static readonly Guid EventoCasamientoDemo = Guid.Parse("30000000-0000-0000-0000-000000000101");
        public static readonly Guid EventoCumpleanosDemo = Guid.Parse("30000000-0000-0000-0000-000000000102");
        public static readonly Guid EventoBookDemo = Guid.Parse("30000000-0000-0000-0000-000000000103");
        public static readonly Guid FotoCasamiento001 = Guid.Parse("40000000-0000-0000-0000-000000000101");
        public static readonly Guid FotoCasamiento002 = Guid.Parse("40000000-0000-0000-0000-000000000102");
        public static readonly Guid FotoCasamiento003 = Guid.Parse("40000000-0000-0000-0000-000000000103");
        public static readonly Guid FotoCumpleanos001 = Guid.Parse("40000000-0000-0000-0000-000000000104");
        public static readonly Guid FotoCumpleanos002 = Guid.Parse("40000000-0000-0000-0000-000000000105");
        public static readonly Guid FotoCumpleanos003 = Guid.Parse("40000000-0000-0000-0000-000000000106");
        public static readonly Guid FotoBook001 = Guid.Parse("40000000-0000-0000-0000-000000000107");
        public static readonly Guid FotoBook002 = Guid.Parse("40000000-0000-0000-0000-000000000108");
        public static readonly Guid PedidoCasamientoPendiente = Guid.Parse("50000000-0000-0000-0000-000000000101");
        public static readonly Guid PedidoCumpleanosPagado = Guid.Parse("50000000-0000-0000-0000-000000000102");
        public static readonly Guid PagoCumpleanos = Guid.Parse("60000000-0000-0000-0000-000000000101");
        public static readonly Guid DescargaCumpleanos001 = Guid.Parse("70000000-0000-0000-0000-000000000101");
        public static readonly Guid ComentarioEventoAdmin = Guid.Parse("80000000-0000-0000-0000-000000000101");
        public static readonly Guid ComentarioEventoCliente = Guid.Parse("80000000-0000-0000-0000-000000000102");
        public static readonly Guid ComentarioFotoAdmin = Guid.Parse("80000000-0000-0000-0000-000000000103");
        public static readonly Guid ComentarioFotoCliente = Guid.Parse("80000000-0000-0000-0000-000000000104");
        public static readonly Guid FavoritoEventoCliente = Guid.Parse("90000000-0000-0000-0000-000000000101");
        public static readonly Guid FavoritoFotoCliente001 = Guid.Parse("90000000-0000-0000-0000-000000000102");
        public static readonly Guid FavoritoFotoCliente002 = Guid.Parse("90000000-0000-0000-0000-000000000103");
        public static readonly Guid FavoritoEventoAdmin = Guid.Parse("90000000-0000-0000-0000-000000000104");
        public static readonly Guid FavoritoFotoAdmin = Guid.Parse("90000000-0000-0000-0000-000000000105");
        public static readonly Guid PerfilFotografaDemo = Guid.Parse("a0000000-0000-0000-0000-000000000101");
        public static readonly Guid PortfolioBodas = Guid.Parse("e1000000-0000-0000-0000-000000000101");
        public static readonly Guid PortfolioQuince = Guid.Parse("e1000000-0000-0000-0000-000000000102");
        public static readonly Guid PortfolioBooks = Guid.Parse("e1000000-0000-0000-0000-000000000103");
        public static readonly Guid PortfolioCorporativo = Guid.Parse("e1000000-0000-0000-0000-000000000104");
        public static readonly Guid PortfolioProducto = Guid.Parse("e1000000-0000-0000-0000-000000000105");
        public static readonly Guid PortfolioFamilia = Guid.Parse("e1000000-0000-0000-0000-000000000106");
        public static readonly Guid ServicioCasamientos = Guid.Parse("e2000000-0000-0000-0000-000000000101");
        public static readonly Guid ServicioCumpleanos = Guid.Parse("e2000000-0000-0000-0000-000000000102");
        public static readonly Guid ServicioBookPersonal = Guid.Parse("e2000000-0000-0000-0000-000000000103");
        public static readonly Guid ServicioCorporativo = Guid.Parse("e2000000-0000-0000-0000-000000000104");
        public static readonly Guid ServicioProducto = Guid.Parse("e2000000-0000-0000-0000-000000000105");
        public static readonly Guid ServicioSesionesPrivadas = Guid.Parse("e2000000-0000-0000-0000-000000000106");
        public static readonly Guid FaqEntrega = Guid.Parse("e3000000-0000-0000-0000-000000000101");
        public static readonly Guid FaqReservas = Guid.Parse("e3000000-0000-0000-0000-000000000102");
        public static readonly Guid FaqPagos = Guid.Parse("e3000000-0000-0000-0000-000000000103");
        public static readonly Guid FaqPrivadas = Guid.Parse("e3000000-0000-0000-0000-000000000104");
        public static readonly Guid FaqEdicion = Guid.Parse("e3000000-0000-0000-0000-000000000105");
        public static readonly Guid SolicitudPresupuestoCasamiento = Guid.Parse("e4000000-0000-0000-0000-000000000101");
        public static readonly Guid SolicitudPresupuestoBook = Guid.Parse("e4000000-0000-0000-0000-000000000102");
        public static readonly Guid AgendaReunionCliente = Guid.Parse("e5000000-0000-0000-0000-000000000101");
        public static readonly Guid AgendaSesionPrivadaDemo = Guid.Parse("e5000000-0000-0000-0000-000000000102");
        public static readonly Guid AgendaBloqueoFecha = Guid.Parse("e5000000-0000-0000-0000-000000000103");
        public static readonly Guid PaqueteCasamientoCompleto = Guid.Parse("b0000000-0000-0000-0000-000000000101");
        public static readonly Guid PaqueteCasamientoPremium = Guid.Parse("b0000000-0000-0000-0000-000000000102");
        public static readonly Guid PaqueteCumpleCompleto = Guid.Parse("b0000000-0000-0000-0000-000000000103");
        public static readonly Guid SesionPrivadaClienteDemo = Guid.Parse("c0000000-0000-0000-0000-000000000101");
        public static readonly Guid FotoPrivadaClienteDemo001 = Guid.Parse("d0000000-0000-0000-0000-000000000101");
        public static readonly Guid FotoPrivadaClienteDemo002 = Guid.Parse("d0000000-0000-0000-0000-000000000102");
        public static readonly Guid HistorialPedidoCasamientoPendiente = Guid.Parse("f0000000-0000-0000-0000-000000000101");
        public static readonly Guid HistorialPedidoCumpleanosPagado = Guid.Parse("f0000000-0000-0000-0000-000000000102");
        public static readonly Guid NotaInternaClienteDemo = Guid.Parse("f1000000-0000-0000-0000-000000000101");
        public static readonly Guid NotaInternaPedidoDemo = Guid.Parse("f1000000-0000-0000-0000-000000000102");
        public static readonly Guid NotaInternaSolicitudDemo = Guid.Parse("f1000000-0000-0000-0000-000000000103");
        public static readonly Guid CuponBienvenida10 = Guid.Parse("f2000000-0000-0000-0000-000000000101");
        public static readonly Guid CuponEvento15 = Guid.Parse("f2000000-0000-0000-0000-000000000102");
        public static readonly Guid CuponSesion5000 = Guid.Parse("f2000000-0000-0000-0000-000000000103");
        public static readonly Guid PromocionTemporada = Guid.Parse("f3000000-0000-0000-0000-000000000101");
        public static readonly Guid PromocionBienvenida = Guid.Parse("f3000000-0000-0000-0000-000000000102");
        public static readonly Guid PromocionSesionPrivada = Guid.Parse("f3000000-0000-0000-0000-000000000103");
        public static readonly Guid TestimonioSofia = Guid.Parse("f4000000-0000-0000-0000-000000000101");
        public static readonly Guid TestimonioValentina = Guid.Parse("f4000000-0000-0000-0000-000000000102");
        public static readonly Guid TestimonioMateo = Guid.Parse("f4000000-0000-0000-0000-000000000103");
        public static readonly Guid TestimonioClienteDemo = Guid.Parse("f4000000-0000-0000-0000-000000000104");
    }
}
