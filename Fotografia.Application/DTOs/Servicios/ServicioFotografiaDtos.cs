using System.ComponentModel.DataAnnotations;

namespace Fotografia.Application.DTOs.Servicios;

public sealed class ServicioFotografiaResponseDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal? PrecioDesde { get; set; }
    public string? DuracionEstimada { get; set; }
    public int? CantidadFotosIncluidas { get; set; }
    public string? ImagenUrl { get; set; }
    public bool Activo { get; set; }
    public bool Destacado { get; set; }
    public int? OrdenDestacado { get; set; }
    public int Orden { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
    public DateTime? FechaActualizacionUtc { get; set; }
}

public sealed class CrearServicioFotografiaRequestDto
{
    [Required]
    [MaxLength(180)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(1200)]
    public string? Descripcion { get; set; }

    [Range(0, 999999999)]
    public decimal? PrecioDesde { get; set; }

    [MaxLength(120)]
    public string? DuracionEstimada { get; set; }

    [Range(0, 100000)]
    public int? CantidadFotosIncluidas { get; set; }

    [MaxLength(1000)]
    public string? ImagenUrl { get; set; }

    public bool Activo { get; set; } = true;
    public bool Destacado { get; set; }
    public int? OrdenDestacado { get; set; }
    public int Orden { get; set; }
}

public sealed class ActualizarServicioFotografiaRequestDto
{
    [Required]
    [MaxLength(180)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(1200)]
    public string? Descripcion { get; set; }

    [Range(0, 999999999)]
    public decimal? PrecioDesde { get; set; }

    [MaxLength(120)]
    public string? DuracionEstimada { get; set; }

    [Range(0, 100000)]
    public int? CantidadFotosIncluidas { get; set; }

    [MaxLength(1000)]
    public string? ImagenUrl { get; set; }

    public bool Activo { get; set; } = true;
    public bool Destacado { get; set; }
    public int? OrdenDestacado { get; set; }
    public int Orden { get; set; }
}
