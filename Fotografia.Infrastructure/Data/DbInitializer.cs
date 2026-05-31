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
                Rol = "Admin",
                Activo = true,
                CreadoEnUtc = SeedClock.Now
            };

            admin.PasswordHash = passwordHasher.HashPassword(admin, AdminPassword);
            dbContext.Usuarios.Add(admin);
        }
        else
        {
            admin.Nombre = "Carlos Cornejo Moscoso";
            admin.Rol = "Admin";
            admin.Activo = true;
            admin.PasswordHash = passwordHasher.HashPassword(admin, AdminPassword);
            admin.ActualizadoEnUtc = SeedClock.Now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await SeedClientesAsync(dbContext, cancellationToken);
        await SeedEventosAsync(dbContext, admin.Id, cancellationToken);
        await SeedFotosAsync(dbContext, cancellationToken);
        await SeedPedidosAsync(dbContext, cancellationToken);
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
    }
}
