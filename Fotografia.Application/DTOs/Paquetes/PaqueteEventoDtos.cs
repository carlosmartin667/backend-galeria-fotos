using System.ComponentModel.DataAnnotations;

namespace Fotografia.Application.DTOs.Paquetes;

public sealed class PaqueteEventoResponseDto
{
    public Guid Id { get; set; }
    public Guid EventoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public bool IncluyeTodasLasFotos { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
    public DateTime? FechaActualizacionUtc { get; set; }
}

public sealed class CrearPaqueteEventoRequestDto
{
    [Required]
    [MaxLength(180)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Descripcion { get; set; }

    [Range(0, 999999999)]
    public decimal Precio { get; set; }

    public bool IncluyeTodasLasFotos { get; set; } = true;
    public bool Activo { get; set; } = true;
}

public sealed class ActualizarPaqueteEventoRequestDto
{
    [Required]
    [MaxLength(180)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Descripcion { get; set; }

    [Range(0, 999999999)]
    public decimal Precio { get; set; }

    public bool IncluyeTodasLasFotos { get; set; } = true;
    public bool Activo { get; set; } = true;
}
