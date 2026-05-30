using Fotografia.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Evento> Eventos => Set<Evento>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Foto> Fotos => Set<Foto>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoFoto> PedidoFotos => Set<PedidoFoto>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<Descarga> Descargas => Set<Descarga>();

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
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Clientes");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Email);
            entity.Property(x => x.Nombre).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Telefono).HasMaxLength(64);
            entity.Property(x => x.Documento).HasMaxLength(64);
        });

        modelBuilder.Entity<Evento>(entity =>
        {
            entity.ToTable("Eventos");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.Property(x => x.Nombre).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Descripcion).HasMaxLength(1000);
            entity.Property(x => x.Slug).HasMaxLength(220).IsRequired();
            entity.Property(x => x.Estado).HasMaxLength(64).IsRequired();
            entity.HasOne(x => x.ClientePrincipal)
                .WithMany(x => x.EventosPrincipales)
                .HasForeignKey(x => x.ClientePrincipalId)
                .OnDelete(DeleteBehavior.SetNull);
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
            entity.HasOne(x => x.Evento)
                .WithMany(x => x.Fotos)
                .HasForeignKey(x => x.EventoId)
                .OnDelete(DeleteBehavior.Cascade);
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
                .OnDelete(DeleteBehavior.Restrict);
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
        });
    }
}
