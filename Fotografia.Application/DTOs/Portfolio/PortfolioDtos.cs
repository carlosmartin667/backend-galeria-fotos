using System.ComponentModel.DataAnnotations;

namespace Fotografia.Application.DTOs.Portfolio;

public sealed class PortfolioItemResponseDto
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? ImagenUrl { get; set; }
    public string? Categoria { get; set; }
    public int Orden { get; set; }
    public bool Destacado { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
    public DateTime? FechaActualizacionUtc { get; set; }
}

public sealed class CrearPortfolioItemRequestDto
{
    [Required]
    [MaxLength(180)]
    public string Titulo { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Descripcion { get; set; }

    [MaxLength(1000)]
    public string? ImagenUrl { get; set; }

    [MaxLength(120)]
    public string? Categoria { get; set; }

    public int Orden { get; set; }
    public bool Destacado { get; set; }
    public bool Activo { get; set; } = true;
}

public sealed class ActualizarPortfolioItemRequestDto
{
    [Required]
    [MaxLength(180)]
    public string Titulo { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Descripcion { get; set; }

    [MaxLength(1000)]
    public string? ImagenUrl { get; set; }

    [MaxLength(120)]
    public string? Categoria { get; set; }

    public int Orden { get; set; }
    public bool Destacado { get; set; }
    public bool Activo { get; set; } = true;
}
