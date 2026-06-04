using System.ComponentModel.DataAnnotations;

namespace Fotografia.Application.DTOs.Agenda;

public sealed class AgendaQueryDto
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public string? Tipo { get; set; }
    public string? Estado { get; set; }
    public bool? Activo { get; set; }
}

public sealed class AgendaItemResponseDto
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public DateTime FechaInicioUtc { get; set; }
    public DateTime FechaFinUtc { get; set; }
    public string? Ubicacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public Guid? EventoId { get; set; }
    public string? EventoNombre { get; set; }
    public Guid? SesionPrivadaId { get; set; }
    public string? SesionPrivadaTitulo { get; set; }
    public Guid? ClienteId { get; set; }
    public string? ClienteNombre { get; set; }
    public Guid? SolicitudPresupuestoId { get; set; }
    public string? SolicitudPresupuestoNombre { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
    public DateTime? FechaActualizacionUtc { get; set; }
}

public sealed class CrearAgendaItemRequestDto
{
    [Required]
    [MaxLength(180)]
    public string Titulo { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Descripcion { get; set; }

    [Required]
    [MaxLength(64)]
    public string Tipo { get; set; } = "Otro";

    [Required]
    public DateTime FechaInicioUtc { get; set; }

    [Required]
    public DateTime FechaFinUtc { get; set; }

    [MaxLength(240)]
    public string? Ubicacion { get; set; }

    [MaxLength(64)]
    public string Estado { get; set; } = "Programado";

    public Guid? EventoId { get; set; }
    public Guid? SesionPrivadaId { get; set; }
    public Guid? ClienteId { get; set; }
    public Guid? SolicitudPresupuestoId { get; set; }
    public bool Activo { get; set; } = true;
}

public sealed class ActualizarAgendaItemRequestDto
{
    [Required]
    [MaxLength(180)]
    public string Titulo { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Descripcion { get; set; }

    [Required]
    [MaxLength(64)]
    public string Tipo { get; set; } = "Otro";

    [Required]
    public DateTime FechaInicioUtc { get; set; }

    [Required]
    public DateTime FechaFinUtc { get; set; }

    [MaxLength(240)]
    public string? Ubicacion { get; set; }

    [MaxLength(64)]
    public string Estado { get; set; } = "Programado";

    public Guid? EventoId { get; set; }
    public Guid? SesionPrivadaId { get; set; }
    public Guid? ClienteId { get; set; }
    public Guid? SolicitudPresupuestoId { get; set; }
    public bool Activo { get; set; } = true;
}

public sealed class DisponibilidadAgendaResponseDto
{
    public DateTime DesdeUtc { get; set; }
    public DateTime HastaUtc { get; set; }
    public List<DisponibilidadAgendaItemDto> Items { get; set; } = [];
}

public sealed class DisponibilidadAgendaItemDto
{
    public DateTime FechaInicioUtc { get; set; }
    public DateTime FechaFinUtc { get; set; }
    public bool Ocupado { get; set; } = true;
    public string Tipo { get; set; } = "Ocupado";
}
