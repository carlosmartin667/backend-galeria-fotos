using System.ComponentModel.DataAnnotations;

namespace Fotografia.Application.DTOs.Sitio;

public sealed class PerfilFotografaResponseDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
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
    public bool Activa { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
    public DateTime? FechaActualizacionUtc { get; set; }
}

public sealed class ActualizarPerfilFotografaRequestDto
{
    [Required]
    [MaxLength(160)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(180)]
    public string? Titulo { get; set; }

    [MaxLength(1000)]
    public string? Descripcion { get; set; }

    [MaxLength(3000)]
    public string? Biografia { get; set; }

    [MaxLength(64)]
    public string? WhatsApp { get; set; }

    [MaxLength(120)]
    public string? Instagram { get; set; }

    [MaxLength(120)]
    public string? Facebook { get; set; }

    [MaxLength(120)]
    public string? TikTok { get; set; }

    [MaxLength(300)]
    public string? SitioWeb { get; set; }

    [EmailAddress]
    [MaxLength(160)]
    public string? CorreoPublico { get; set; }

    [MaxLength(300)]
    public string? Direccion { get; set; }

    [MaxLength(120)]
    public string? Ciudad { get; set; }

    [MaxLength(120)]
    public string? Provincia { get; set; }

    [MaxLength(120)]
    public string? Pais { get; set; }

    [MaxLength(1000)]
    public string? FotoPerfilUrl { get; set; }

    [MaxLength(1000)]
    public string? LogoUrl { get; set; }

    [MaxLength(1000)]
    public string? BannerUrl { get; set; }

    [MaxLength(1000)]
    public string? TextoBienvenida { get; set; }

    public bool Activa { get; set; } = true;
}
