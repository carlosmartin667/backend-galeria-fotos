using Fotografia.Application.Security;
using Fotografia.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
        await SeedTestDataAsync(dbContext, cancellationToken);
    }

    public static async Task SeedTestDataAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var passwordHasher = new PasswordHasher<Usuario>();

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

        await dbContext.SaveChangesAsync(cancellationToken);

        await SeedClientesAsync(dbContext, cancellationToken);
        await SeedEventosAsync(dbContext, admin.Id, cancellationToken);
        await SeedFotosAsync(dbContext, cancellationToken);
        await SeedPedidosAsync(dbContext, cancellationToken);
        await SeedRequestedDemoDataAsync(dbContext, passwordHasher, cancellationToken);
    }

    private static async Task SeedClientesAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
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

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedEventosAsync(AppDbContext dbContext, Guid adminId, CancellationToken cancellationToken)
    {
        var eventos = new[]
        {
            new Evento
            {
                Id = SeedIds.EventoBoda,
                Nombre = "Boda Sofia y Lucas",
                Descripcion = "Ceremonia civil y fiesta en salon Las Acacias.",
                Slug = "boda-sofia-y-lucas",
                FechaEventoUtc = new DateTime(2026, 4, 18, 21, 0, 0, DateTimeKind.Utc),
                Estado = "Activo",
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
                Estado = "Activo",
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
                Estado = "Activo",
                ClientePrincipalId = SeedIds.ClienteMateo,
                CreadoPorUsuarioId = adminId,
                CreadoEnUtc = SeedClock.Now.AddDays(-12)
            }
        };

        foreach (var evento in eventos)
        {
            if (!await dbContext.Eventos.AnyAsync(x => x.Id == evento.Id, cancellationToken))
            {
                dbContext.Eventos.Add(evento);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedFotosAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
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
            if (!await dbContext.Fotos.AnyAsync(x => x.Id == foto.Id, cancellationToken))
            {
                dbContext.Fotos.Add(foto);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedPedidosAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        if (!await dbContext.Pedidos.AnyAsync(x => x.Id == SeedIds.PedidoBodaPagado, cancellationToken))
        {
            var pedido = new Pedido
            {
                Id = SeedIds.PedidoBodaPagado,
                EventoId = SeedIds.EventoBoda,
                ClienteId = SeedIds.ClienteSofia,
                Estado = "Pagado",
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

        await dbContext.SaveChangesAsync(cancellationToken);

        await SeedDescargasAsync(dbContext, cancellationToken);
    }

    private static async Task SeedDescargasAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var descargas = new[]
        {
            CreateDescarga(SeedIds.DescargaBoda01, SeedIds.PedidoBodaPagado, SeedIds.EventoBoda, SeedIds.ClienteSofia, SeedIds.FotoBoda01, "boda-sofia-lucas-001.jpg"),
            CreateDescarga(SeedIds.DescargaBoda02, SeedIds.PedidoBodaPagado, SeedIds.EventoBoda, SeedIds.ClienteSofia, SeedIds.FotoBoda02, "boda-sofia-lucas-002.jpg"),
            CreateDescarga(SeedIds.DescargaWorkshop01, SeedIds.PedidoWorkshopPagado, SeedIds.EventoCorporativo, SeedIds.ClienteMateo, SeedIds.FotoWorkshop01, "workshop-marca-personal-001.jpg")
        };

        foreach (var descarga in descargas)
        {
            if (!await dbContext.Descargas.AnyAsync(x => x.Id == descarga.Id, cancellationToken))
            {
                dbContext.Descargas.Add(descarga);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedRequestedDemoDataAsync(
        AppDbContext dbContext,
        PasswordHasher<Usuario> passwordHasher,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

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

        await dbContext.SaveChangesAsync(cancellationToken);

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

        await dbContext.SaveChangesAsync(cancellationToken);

        var casamiento = await UpsertEventoAsync(
            dbContext,
            SeedIds.EventoCasamientoDemo,
            "Casamiento Demo",
            "Evento de casamiento con galeria privada para clientes.",
            "casamiento-demo",
            now.AddDays(10),
            "Publicado",
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
            "Publicado",
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
            "Borrador",
            null,
            admin.Id,
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        var casamiento001 = await UpsertDemoFotoAsync(dbContext, SeedIds.FotoCasamiento001, casamiento.Id, "casamiento-001.jpg", now, cancellationToken);
        var casamiento002 = await UpsertDemoFotoAsync(dbContext, SeedIds.FotoCasamiento002, casamiento.Id, "casamiento-002.jpg", now, cancellationToken);
        await UpsertDemoFotoAsync(dbContext, SeedIds.FotoCasamiento003, casamiento.Id, "casamiento-003.jpg", now, cancellationToken);

        var cumpleanos001 = await UpsertDemoFotoAsync(dbContext, SeedIds.FotoCumpleanos001, cumpleanos.Id, "cumpleanos-001.jpg", now, cancellationToken);
        await UpsertDemoFotoAsync(dbContext, SeedIds.FotoCumpleanos002, cumpleanos.Id, "cumpleanos-002.jpg", now, cancellationToken);
        await UpsertDemoFotoAsync(dbContext, SeedIds.FotoCumpleanos003, cumpleanos.Id, "cumpleanos-003.jpg", now, cancellationToken);

        var book001 = await UpsertDemoFotoAsync(dbContext, SeedIds.FotoBook001, book.Id, "book-001.jpg", now, cancellationToken);
        await UpsertDemoFotoAsync(dbContext, SeedIds.FotoBook002, book.Id, "book-002.jpg", now, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        await UpsertPedidoAsync(
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

        await dbContext.SaveChangesAsync(cancellationToken);

        await UpsertPagoAsync(dbContext, SeedIds.PagoCumpleanos, pedidoPagado, "Aprobado", "MP-DEMO-0001", "PREF-DEMO-0001", now, cancellationToken);
        await UpsertDescargaAsync(dbContext, SeedIds.DescargaCumpleanos001, pedidoPagado, clienteInvitado.Id, cumpleanos.Id, cumpleanos001, now, cancellationToken);

        await UpsertComentarioEventoAsync(dbContext, SeedIds.ComentarioEventoAdmin, casamiento.Id, admin.Id, "Evento cargado correctamente. Galeria lista para revision.", now, cancellationToken);
        await UpsertComentarioEventoAsync(dbContext, SeedIds.ComentarioEventoCliente, casamiento.Id, usuarioCliente.Id, "Me gustaria marcar estas fotos como favoritas para revisarlas luego.", now, cancellationToken);
        await UpsertComentarioFotoAsync(dbContext, SeedIds.ComentarioFotoAdmin, casamiento001.Id, admin.Id, "Foto destacada para portada.", now, cancellationToken);
        await UpsertComentarioFotoAsync(dbContext, SeedIds.ComentarioFotoCliente, casamiento002.Id, usuarioCliente.Id, "Esta foto me interesa para compra.", now, cancellationToken);

        await UpsertEventoFavoritoAsync(dbContext, SeedIds.FavoritoEventoCliente, usuarioCliente.Id, casamiento.Id, now, cancellationToken);
        await UpsertFotoFavoritaAsync(dbContext, SeedIds.FavoritoFotoCliente001, usuarioCliente.Id, casamiento001.Id, now, cancellationToken);
        await UpsertFotoFavoritaAsync(dbContext, SeedIds.FavoritoFotoCliente002, usuarioCliente.Id, casamiento002.Id, now, cancellationToken);
        await UpsertEventoFavoritoAsync(dbContext, SeedIds.FavoritoEventoAdmin, admin.Id, book.Id, now, cancellationToken);
        await UpsertFotoFavoritaAsync(dbContext, SeedIds.FavoritoFotoAdmin, admin.Id, book001.Id, now, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
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
        foto.Activa = true;
        foto.SubidaEnUtc = now;

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
        var pedido = await dbContext.Pedidos
            .Include(x => x.PedidoFotos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

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
        pedido.Total = fotos.Sum(x => x.PrecioUnitario);
        pedido.ActualizadoEnUtc = now;

        foreach (var foto in fotos)
        {
            var item = pedido.PedidoFotos.FirstOrDefault(x => x.FotoId == foto.Id);
            if (item is null)
            {
                pedido.PedidoFotos.Add(new PedidoFoto
                {
                    FotoId = foto.Id,
                    Cantidad = 1,
                    PrecioUnitario = foto.PrecioUnitario
                });
            }
            else
            {
                item.Cantidad = 1;
                item.PrecioUnitario = foto.PrecioUnitario;
            }
        }

        return pedido;
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

        pago.PedidoId = pedido.Id;
        pago.Estado = estado;
        pago.Monto = pedido.Total;
        pago.Moneda = pedido.Moneda;
        pago.MercadoPagoPaymentId = paymentId;
        pago.MercadoPagoPreferenceId = preferenceId;
        pago.PagadoEnUtc = now;
        pago.ActualizadoEnUtc = now;
        pedido.MercadoPagoPreferenceId = preferenceId;
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
            .FirstOrDefaultAsync(x => x.PedidoId == pedido.Id && x.FotoId == foto.Id, cancellationToken);

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
                NombreArchivo = foto.NombreArchivo
            };

            dbContext.Descargas.Add(descarga);
        }

        descarga.StorageKey = foto.StorageKey;
        descarga.NombreArchivo = foto.NombreArchivo;
        descarga.ExpiraEnUtc = now.AddHours(24);
        descarga.CreadoEnUtc = now;
        descarga.DescargasRealizadas = 0;
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
            Activa = true,
            SubidaEnUtc = SeedClock.Now.AddDays(-10)
        };
    }

    private static Descarga CreateDescarga(
        Guid id,
        Guid pedidoId,
        Guid eventoId,
        Guid clienteId,
        Guid fotoId,
        string nombreArchivo)
    {
        return new Descarga
        {
            Id = id,
            PedidoId = pedidoId,
            EventoId = eventoId,
            ClienteId = clienteId,
            FotoId = fotoId,
            StorageKey = $"eventos/{eventoId:N}/fotos/{nombreArchivo}",
            NombreArchivo = nombreArchivo,
            ExpiraEnUtc = SeedClock.Now.AddHours(6),
            CreadoEnUtc = SeedClock.Now.AddHours(-2),
            DescargasRealizadas = 1
        };
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
    }
}
