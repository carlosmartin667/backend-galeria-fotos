using System.ComponentModel.DataAnnotations;

namespace Fotografia.Application.DTOs.Eventos;

public sealed class CrearEventoRequestDto
{
    [Required]
    [MaxLength(180)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Descripcion { get; set; }

    [Required]
    public DateTime FechaEventoUtc { get; set; }

    [MaxLength(64)]
    public string? Estado { get; set; }

    [MaxLength(64)]
    public string? Visibilidad { get; set; }

    public DateTime? FechaLimiteCompraUtc { get; set; }

    public Guid? ClientePrincipalId { get; set; }
}

public sealed class ActualizarEventoRequestDto
{
    [Required]
    [MaxLength(180)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Descripcion { get; set; }

    [Required]
    public DateTime FechaEventoUtc { get; set; }

    [Required]
    [MaxLength(64)]
    public string Estado { get; set; } = "Activo";

    [MaxLength(64)]
    public string Visibilidad { get; set; } = "Publico";

    public DateTime? FechaLimiteCompraUtc { get; set; }
    public bool Activo { get; set; } = true;
    public Guid? ClientePrincipalId { get; set; }
}

public sealed class EventoResponseDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Slug { get; set; } = string.Empty;
    public DateTime FechaEventoUtc { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Visibilidad { get; set; } = string.Empty;
    public DateTime? FechaLimiteCompraUtc { get; set; }
    public bool Activo { get; set; }
    public Guid? ClientePrincipalId { get; set; }
    public Guid? PortadaFotoId { get; set; }
    public string? PortadaPreviewUrl { get; set; }
    public string? PortadaNombreArchivo { get; set; }
    public int CantidadFotos { get; set; }
    public DateTime CreadoEnUtc { get; set; }
    public DateTime? ActualizadoEnUtc { get; set; }
}
