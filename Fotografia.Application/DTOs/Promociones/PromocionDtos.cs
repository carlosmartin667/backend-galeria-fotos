using System.ComponentModel.DataAnnotations;

namespace Fotografia.Application.DTOs.Promociones;

public sealed class PromocionResponseDto
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? ImagenUrl { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public DateTime? FechaInicioUtc { get; set; }
    public DateTime? FechaFinUtc { get; set; }
    public bool Activa { get; set; }
    public bool Destacada { get; set; }
    public int Orden { get; set; }
    public Guid? CuponDescuentoId { get; set; }
    public string? CuponCodigo { get; set; }
    public Guid? ServicioFotografiaId { get; set; }
    public string? ServicioNombre { get; set; }
    public Guid? EventoId { get; set; }
    public string? EventoNombre { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
    public DateTime? FechaActualizacionUtc { get; set; }
}

public class CrearPromocionRequestDto
{
    [Required]
    [MaxLength(180)]
    public string Titulo { get; set; } = string.Empty;

    [MaxLength(1200)]
    public string? Descripcion { get; set; }

    [MaxLength(1000)]
    public string? ImagenUrl { get; set; }

    [Required]
    [MaxLength(64)]
    public string Tipo { get; set; } = string.Empty;

    public DateTime? FechaInicioUtc { get; set; }
    public DateTime? FechaFinUtc { get; set; }
    public bool Activa { get; set; } = true;
    public bool Destacada { get; set; }
    public int Orden { get; set; }
    public Guid? CuponDescuentoId { get; set; }
    public Guid? ServicioFotografiaId { get; set; }
    public Guid? EventoId { get; set; }
}

public sealed class ActualizarPromocionRequestDto : CrearPromocionRequestDto;
