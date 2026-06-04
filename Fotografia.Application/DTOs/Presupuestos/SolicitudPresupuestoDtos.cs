using System.ComponentModel.DataAnnotations;

namespace Fotografia.Application.DTOs.Presupuestos;

public sealed class CrearSolicitudPresupuestoRequestDto
{
    [Required]
    [MaxLength(160)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(64)]
    public string? WhatsApp { get; set; }

    [MaxLength(120)]
    public string? TipoEvento { get; set; }

    public Guid? ServicioId { get; set; }
    public DateTime? FechaTentativaUtc { get; set; }

    [MaxLength(240)]
    public string? Lugar { get; set; }

    [Range(1, 100000)]
    public int? CantidadInvitados { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Mensaje { get; set; } = string.Empty;
}

public sealed class ActualizarSolicitudPresupuestoRequestDto
{
    [Required]
    [MaxLength(160)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(64)]
    public string? WhatsApp { get; set; }

    [MaxLength(120)]
    public string? TipoEvento { get; set; }

    public Guid? ServicioId { get; set; }
    public DateTime? FechaTentativaUtc { get; set; }

    [MaxLength(240)]
    public string? Lugar { get; set; }

    [Range(1, 100000)]
    public int? CantidadInvitados { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Mensaje { get; set; } = string.Empty;

    [MaxLength(64)]
    public string Estado { get; set; } = "Nuevo";

    public bool Activa { get; set; } = true;
}

public sealed class CambiarEstadoSolicitudPresupuestoRequestDto
{
    [Required]
    [MaxLength(64)]
    public string Estado { get; set; } = string.Empty;
}

public sealed class SolicitudPresupuestoResponseDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? WhatsApp { get; set; }
    public string? TipoEvento { get; set; }
    public Guid? ServicioId { get; set; }
    public string? ServicioNombre { get; set; }
    public DateTime? FechaTentativaUtc { get; set; }
    public string? Lugar { get; set; }
    public int? CantidadInvitados { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public bool Activa { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
    public DateTime? FechaActualizacionUtc { get; set; }
}
