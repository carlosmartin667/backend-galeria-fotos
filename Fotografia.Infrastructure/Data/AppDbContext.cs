using Fotografia.Domain.Entities;
using Fotografia.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Evento> Eventos => Set<Evento>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Foto> Fotos => Set<Foto>();
    public DbSet<PaqueteEvento> PaquetesEvento => Set<PaqueteEvento>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoFoto> PedidoFotos => Set<PedidoFoto>();
    public DbSet<PedidoItem> PedidoItems => Set<PedidoItem>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<Descarga> Descargas => Set<Descarga>();
    public DbSet<CarritoCompra> CarritosCompra => Set<CarritoCompra>();
    public DbSet<CarritoItem> CarritoItems => Set<CarritoItem>();
    public DbSet<PerfilFotografa> PerfilesFotografa => Set<PerfilFotografa>();
    public DbSet<SesionPrivada> SesionesPrivadas => Set<SesionPrivada>();
    public DbSet<FotoPrivada> FotosPrivadas => Set<FotoPrivada>();
    public DbSet<ComentarioEvento> ComentariosEventos => Set<ComentarioEvento>();
    public DbSet<ComentarioFoto> ComentariosFotos => Set<ComentarioFoto>();
    public DbSet<EventoFavorito> EventosFavoritos => Set<EventoFavorito>();
    public DbSet<FotoFavorita> FotosFavoritas => Set<FotoFavorita>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Nombre).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(x => x.Rol).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Descripcion).HasMaxLength(1000);
            entity.Property(x => x.WhatsApp).HasMaxLength(64);
            entity.Property(x => x.Instagram).HasMaxLength(120);
            entity.Property(x => x.CorreoPublico).HasMaxLength(160);
            entity.Property(x => x.Direccion).HasMaxLength(300);
        });

        modelBuilder.Entity<PerfilFotografa>(entity =>
        {
            entity.ToTable("PerfilesFotografa");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Activa).IsUnique().HasFilter("[Activa] = 1");
            entity.Property(x => x.Nombre).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Titulo).HasMaxLength(180);
            entity.Property(x => x.Descripcion).HasMaxLength(1000);
            entity.Property(x => x.Biografia).HasMaxLength(3000);
            entity.Property(x => x.WhatsApp).HasMaxLength(64);
            entity.Property(x => x.Instagram).HasMaxLength(120);
            entity.Property(x => x.Facebook).HasMaxLength(120);
            entity.Property(x => x.TikTok).HasMaxLength(120);
            entity.Property(x => x.SitioWeb).HasMaxLength(300);
            entity.Property(x => x.CorreoPublico).HasMaxLength(160);
            entity.Property(x => x.Direccion).HasMaxLength(300);
            entity.Property(x => x.Ciudad).HasMaxLength(120);
            entity.Property(x => x.Provincia).HasMaxLength(120);
            entity.Property(x => x.Pais).HasMaxLength(120);
            entity.Property(x => x.FotoPerfilUrl).HasMaxLength(1000);
            entity.Property(x => x.LogoUrl).HasMaxLength(1000);
            entity.Property(x => x.BannerUrl).HasMaxLength(1000);
            entity.Property(x => x.TextoBienvenida).HasMaxLength(1000);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Clientes");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Email);
            entity.HasIndex(x => x.UsuarioId).IsUnique().HasFilter("[UsuarioId] IS NOT NULL");
            entity.Property(x => x.Nombre).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Telefono).HasMaxLength(64);
            entity.Property(x => x.Documento).HasMaxLength(64);
            entity.HasOne(x => x.Usuario)
                .WithOne(x => x.Cliente)
                .HasForeignKey<Cliente>(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Evento>(entity =>
        {
            entity.ToTable("Eventos");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.HasIndex(x => new { x.Activo, x.Estado, x.Visibilidad });
            entity.HasIndex(x => x.PortadaFotoId);
            entity.Property(x => x.Nombre).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Descripcion).HasMaxLength(1000);
            entity.Property(x => x.Slug).HasMaxLength(220).IsRequired();
            entity.Property(x => x.Estado).HasMaxLength(64).HasDefaultValue(EventoEstados.Publicado).IsRequired();
            entity.Property(x => x.Visibilidad).HasMaxLength(64).HasDefaultValue(EventoVisibilidades.Publico).IsRequired();
            entity.Property(x => x.Activo).HasDefaultValue(true);
            entity.HasOne(x => x.ClientePrincipal)
                .WithMany(x => x.EventosPrincipales)
                .HasForeignKey(x => x.ClientePrincipalId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.PortadaFoto)
                .WithMany()
                .HasForeignKey(x => x.PortadaFotoId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CreadoPorUsuario)
                .WithMany(x => x.EventosCreados)
                .HasForeignKey(x => x.CreadoPorUsuarioId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Foto>(entity =>
        {
            entity.ToTable("Fotos");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.EventoId, x.StorageKey }).IsUnique();
            entity.Property(x => x.NombreArchivo).HasMaxLength(260).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(120).IsRequired();
            entity.Property(x => x.StorageKey).HasMaxLength(700).IsRequired();
            entity.Property(x => x.PreviewUrl).HasMaxLength(1000);
            entity.Property(x => x.MarcaAguaStorageKey).HasMaxLength(700);
            entity.Property(x => x.PrecioUnitario).HasPrecision(18, 2);
            entity.Property(x => x.TieneMarcaAgua).HasDefaultValue(false);
            entity.Property(x => x.Procesada).HasDefaultValue(false);
            entity.HasOne(x => x.Evento)
                .WithMany(x => x.Fotos)
                .HasForeignKey(x => x.EventoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PaqueteEvento>(entity =>
        {
            entity.ToTable("PaquetesEvento");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.EventoId);
            entity.Property(x => x.Nombre).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Descripcion).HasMaxLength(1000);
            entity.Property(x => x.Precio).HasPrecision(18, 2);
            entity.HasOne(x => x.Evento)
                .WithMany(x => x.Paquetes)
                .HasForeignKey(x => x.EventoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SesionPrivada>(entity =>
        {
            entity.ToTable("SesionesPrivadas");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.ClienteId);
            entity.Property(x => x.Titulo).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Descripcion).HasMaxLength(1000);
            entity.Property(x => x.Estado).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PrecioPaquete).HasPrecision(18, 2);
            entity.HasOne(x => x.Cliente)
                .WithMany(x => x.SesionesPrivadas)
                .HasForeignKey(x => x.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FotoPrivada>(entity =>
        {
            entity.ToTable("FotosPrivadas");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.SesionPrivadaId);
            entity.HasIndex(x => x.ClienteId);
            entity.HasIndex(x => new { x.SesionPrivadaId, x.StorageKey }).IsUnique();
            entity.Property(x => x.NombreArchivo).HasMaxLength(260).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(120).IsRequired();
            entity.Property(x => x.StorageKey).HasMaxLength(700).IsRequired();
            entity.Property(x => x.PreviewUrl).HasMaxLength(1000);
            entity.Property(x => x.MarcaAguaStorageKey).HasMaxLength(700);
            entity.Property(x => x.PrecioUnitario).HasPrecision(18, 2);
            entity.HasOne(x => x.SesionPrivada)
                .WithMany(x => x.Fotos)
                .HasForeignKey(x => x.SesionPrivadaId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Cliente)
                .WithMany(x => x.FotosPrivadas)
                .HasForeignKey(x => x.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CarritoCompra>(entity =>
        {
            entity.ToTable("CarritosCompra");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UsuarioId, x.Estado })
                .IsUnique()
                .HasFilter("[Estado] = 'Activo'");
            entity.Property(x => x.Estado).HasMaxLength(64).IsRequired();
            entity.HasOne(x => x.Usuario)
                .WithMany(x => x.CarritosCompra)
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CarritoItem>(entity =>
        {
            entity.ToTable("CarritoItems");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.CarritoCompraId);
            entity.HasIndex(x => new { x.CarritoCompraId, x.TipoItem, x.FotoId })
                .IsUnique()
                .HasFilter("[FotoId] IS NOT NULL");
            entity.HasIndex(x => new { x.CarritoCompraId, x.TipoItem, x.PaqueteEventoId })
                .IsUnique()
                .HasFilter("[PaqueteEventoId] IS NOT NULL");
            entity.HasIndex(x => new { x.CarritoCompraId, x.TipoItem, x.FotoPrivadaId })
                .IsUnique()
                .HasFilter("[FotoPrivadaId] IS NOT NULL");
            entity.Property(x => x.TipoItem).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Descripcion).HasMaxLength(300).IsRequired();
            entity.Property(x => x.PrecioUnitario).HasPrecision(18, 2);
            entity.HasOne(x => x.CarritoCompra)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.CarritoCompraId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Foto)
                .WithMany(x => x.CarritoItems)
                .HasForeignKey(x => x.FotoId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.PaqueteEvento)
                .WithMany(x => x.CarritoItems)
                .HasForeignKey(x => x.PaqueteEventoId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.FotoPrivada)
                .WithMany(x => x.CarritoItems)
                .HasForeignKey(x => x.FotoPrivadaId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.ToTable("Pedidos");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Estado).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Total).HasPrecision(18, 2);
            entity.Property(x => x.Moneda).HasMaxLength(8).IsRequired();
            entity.Property(x => x.MercadoPagoPreferenceId).HasMaxLength(160);
            entity.HasOne(x => x.Evento)
                .WithMany(x => x.Pedidos)
                .HasForeignKey(x => x.EventoId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Cliente)
                .WithMany(x => x.Pedidos)
                .HasForeignKey(x => x.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PedidoFoto>(entity =>
        {
            entity.ToTable("PedidoFotos");
            entity.HasKey(x => new { x.PedidoId, x.FotoId });
            entity.Property(x => x.PrecioUnitario).HasPrecision(18, 2);
            entity.HasOne(x => x.Pedido)
                .WithMany(x => x.PedidoFotos)
                .HasForeignKey(x => x.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Foto)
                .WithMany(x => x.PedidoFotos)
                .HasForeignKey(x => x.FotoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PedidoItem>(entity =>
        {
            entity.ToTable("PedidoItems");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.PedidoId);
            entity.Property(x => x.TipoItem).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Descripcion).HasMaxLength(300).IsRequired();
            entity.Property(x => x.PrecioUnitario).HasPrecision(18, 2);
            entity.Property(x => x.Subtotal).HasPrecision(18, 2);
            entity.HasOne(x => x.Pedido)
                .WithMany(x => x.PedidoItems)
                .HasForeignKey(x => x.PedidoId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Foto)
                .WithMany(x => x.PedidoItems)
                .HasForeignKey(x => x.FotoId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.PaqueteEvento)
                .WithMany(x => x.PedidoItems)
                .HasForeignKey(x => x.PaqueteEventoId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.FotoPrivada)
                .WithMany(x => x.PedidoItems)
                .HasForeignKey(x => x.FotoPrivadaId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.ToTable("Pagos");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.PedidoId).IsUnique();
            entity.Property(x => x.MercadoPagoPaymentId).HasMaxLength(160);
            entity.Property(x => x.MercadoPagoPreferenceId).HasMaxLength(160);
            entity.Property(x => x.Estado).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Monto).HasPrecision(18, 2);
            entity.Property(x => x.Moneda).HasMaxLength(8).IsRequired();
            entity.HasOne(x => x.Pedido)
                .WithOne(x => x.Pago)
                .HasForeignKey<Pago>(x => x.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Descarga>(entity =>
        {
            entity.ToTable("Descargas");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.StorageKey).HasMaxLength(700).IsRequired();
            entity.Property(x => x.NombreArchivo).HasMaxLength(260).IsRequired();
            entity.Property(x => x.MaxDescargas).HasDefaultValue(5);
            entity.Property(x => x.Activa).HasDefaultValue(true);
            entity.HasOne(x => x.Pedido)
                .WithMany(x => x.Descargas)
                .HasForeignKey(x => x.PedidoId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Evento)
                .WithMany(x => x.Descargas)
                .HasForeignKey(x => x.EventoId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Cliente)
                .WithMany(x => x.Descargas)
                .HasForeignKey(x => x.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Foto)
                .WithMany()
                .HasForeignKey(x => x.FotoId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.FotoPrivada)
                .WithMany()
                .HasForeignKey(x => x.FotoPrivadaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ComentarioEvento>(entity =>
        {
            entity.ToTable("ComentariosEventos");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Texto).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.FechaCreacionUtc).IsRequired();
            entity.HasOne(x => x.Evento)
                .WithMany(x => x.Comentarios)
                .HasForeignKey(x => x.EventoId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Usuario)
                .WithMany(x => x.ComentariosEventos)
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ComentarioFoto>(entity =>
        {
            entity.ToTable("ComentariosFotos");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Texto).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.FechaCreacionUtc).IsRequired();
            entity.HasOne(x => x.Foto)
                .WithMany(x => x.Comentarios)
                .HasForeignKey(x => x.FotoId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Usuario)
                .WithMany(x => x.ComentariosFotos)
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EventoFavorito>(entity =>
        {
            entity.ToTable("EventosFavoritos");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UsuarioId, x.EventoId }).IsUnique();
            entity.Property(x => x.FechaCreacionUtc).IsRequired();
            entity.HasOne(x => x.Evento)
                .WithMany(x => x.Favoritos)
                .HasForeignKey(x => x.EventoId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Usuario)
                .WithMany(x => x.EventosFavoritos)
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FotoFavorita>(entity =>
        {
            entity.ToTable("FotosFavoritas");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UsuarioId, x.FotoId }).IsUnique();
            entity.Property(x => x.FechaCreacionUtc).IsRequired();
            entity.HasOne(x => x.Foto)
                .WithMany(x => x.Favoritos)
                .HasForeignKey(x => x.FotoId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Usuario)
                .WithMany(x => x.FotosFavoritas)
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
