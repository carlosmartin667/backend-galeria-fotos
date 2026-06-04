using System.ComponentModel.DataAnnotations;

namespace Fotografia.Application.DTOs.Testimonios;

public sealed class TestimonioPublicoResponseDto
{
    public Guid Id { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public string Texto { get; set; } = string.Empty;
    public int Calificacion { get; set; }
    public string? ImagenUrl { get; set; }
    public bool Destacado { get; set; }
    public Guid? ServicioFotografiaId { get; set; }
    public Guid? EventoId { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
}

public sealed class TestimonioAdminResponseDto
{
    public Guid Id { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public string? EmailCliente { get; set; }
    public string Texto { get; set; } = string.Empty;
    public int Calificacion { get; set; }
    public string? ImagenUrl { get; set; }
    public bool Publicado { get; set; }
    public bool Destacado { get; set; }
    public bool Activo { get; set; }
    public Guid? ClienteId { get; set; }
    public Guid? PedidoId { get; set; }
    public Guid? ServicioFotografiaId { get; set; }
    public Guid? EventoId { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
    public DateTime? FechaActualizacionUtc { get; set; }
}

public sealed class CrearTestimonioRequestDto
{
    [Required]
    [MaxLength(160)]
    public string NombreCliente { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(256)]
    public string? EmailCliente { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Texto { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Calificacion { get; set; }

    [MaxLength(1000)]
    public string? ImagenUrl { get; set; }

    public Guid? ClienteId { get; set; }
    public Guid? PedidoId { get; set; }
    public Guid? ServicioFotografiaId { get; set; }
    public Guid? EventoId { get; set; }
}

public sealed class ActualizarTestimonioAdminRequestDto
{
    [Required]
    [MaxLength(160)]
    public string NombreCliente { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(256)]
    public string? EmailCliente { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Texto { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Calificacion { get; set; }

    [MaxLength(1000)]
    public string? ImagenUrl { get; set; }

    public bool Publicado { get; set; }
    public bool Destacado { get; set; }
    public bool Activo { get; set; } = true;
    public Guid? ClienteId { get; set; }
    public Guid? PedidoId { get; set; }
    public Guid? ServicioFotografiaId { get; set; }
    public Guid? EventoId { get; set; }
}
