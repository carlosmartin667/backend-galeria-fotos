using System.ComponentModel.DataAnnotations;

namespace Fotografia.Application.DTOs.Faq;

public sealed class PreguntaFrecuenteResponseDto
{
    public Guid Id { get; set; }
    public string Pregunta { get; set; } = string.Empty;
    public string Respuesta { get; set; } = string.Empty;
    public string? Categoria { get; set; }
    public int Orden { get; set; }
    public bool Activa { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
    public DateTime? FechaActualizacionUtc { get; set; }
}

public sealed class CrearPreguntaFrecuenteRequestDto
{
    [Required]
    [MaxLength(220)]
    public string Pregunta { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Respuesta { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? Categoria { get; set; }

    public int Orden { get; set; }
    public bool Activa { get; set; } = true;
}

public sealed class ActualizarPreguntaFrecuenteRequestDto
{
    [Required]
    [MaxLength(220)]
    public string Pregunta { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Respuesta { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? Categoria { get; set; }

    public int Orden { get; set; }
    public bool Activa { get; set; } = true;
}
