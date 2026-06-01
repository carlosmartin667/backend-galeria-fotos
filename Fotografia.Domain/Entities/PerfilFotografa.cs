namespace Fotografia.Domain.Entities;

public sealed class PerfilFotografa
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Nombre { get; set; }
    public string? Titulo { get; set; }
    public string? Descripcion { get; set; }
    public string? Biografia { get; set; }
    public string? WhatsApp { get; set; }
    public string? Instagram { get; set; }
    public string? Facebook { get; set; }
    public string? TikTok { get; set; }
    public string? SitioWeb { get; set; }
    public string? CorreoPublico { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? Provincia { get; set; }
    public string? Pais { get; set; }
    public string? FotoPerfilUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? TextoBienvenida { get; set; }
    public bool Activa { get; set; } = true;
    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacionUtc { get; set; }
}
