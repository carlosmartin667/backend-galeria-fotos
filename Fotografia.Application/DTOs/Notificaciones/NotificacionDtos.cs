using System.ComponentModel.DataAnnotations;

namespace Fotografia.Application.DTOs.Notificaciones;

public sealed class NotificacionResponseDto
{
    public Guid Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Canal { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string? DestinatarioEmail { get; set; }
    public Guid? UsuarioId { get; set; }
    public string? UsuarioNombre { get; set; }
    public string? EntidadTipo { get; set; }
    public Guid? EntidadId { get; set; }
    public string? CorrelationKey { get; set; }
    public int Intentos { get; set; }
    public int MaxIntentos { get; set; }
    public string? Error { get; set; }
    public bool Leida { get; set; }
    public DateTime? FechaLecturaUtc { get; set; }
    public DateTime? ProgramadaParaUtc { get; set; }
    public DateTime? EnviadaEnUtc { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
    public DateTime? FechaActualizacionUtc { get; set; }
    public bool Activa { get; set; }
}

public sealed class EnqueueNotificacionRequestDto
{
    [Required]
    [MaxLength(120)]
    public string Tipo { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    public string Canal { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    public string Mensaje { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(256)]
    public string? DestinatarioEmail { get; set; }

    public Guid? UsuarioId { get; set; }

    [MaxLength(80)]
    public string? EntidadTipo { get; set; }

    public Guid? EntidadId { get; set; }

    [MaxLength(240)]
    public string? CorrelationKey { get; set; }

    public DateTime? ProgramadaParaUtc { get; set; }
    public int? MaxIntentos { get; set; }
}

public sealed class EnqueueTemplateNotificacionRequestDto
{
    [Required]
    [MaxLength(120)]
    public string Codigo { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(256)]
    public string? DestinatarioEmail { get; set; }

    public Guid? UsuarioId { get; set; }

    [MaxLength(80)]
    public string? EntidadTipo { get; set; }

    public Guid? EntidadId { get; set; }

    [MaxLength(240)]
    public string? CorrelationKey { get; set; }

    public DateTime? ProgramadaParaUtc { get; set; }
    public Dictionary<string, string?> Reemplazos { get; set; } = [];
}

public sealed class NotificacionesAdminQueryDto
{
    public string? Estado { get; set; }
    public string? Canal { get; set; }
    public string? Tipo { get; set; }
    public bool? Activa { get; set; }
    public int Take { get; set; } = 100;
}

public sealed class PlantillaNotificacionResponseDto
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Canal { get; set; } = string.Empty;
    public string Asunto { get; set; } = string.Empty;
    public string CuerpoHtml { get; set; } = string.Empty;
    public string? CuerpoTexto { get; set; }
    public bool Activa { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
    public DateTime? FechaActualizacionUtc { get; set; }
}

public sealed class CrearPlantillaNotificacionRequestDto
{
    [Required]
    [MaxLength(120)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    public string Canal { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string Asunto { get; set; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    public string CuerpoHtml { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? CuerpoTexto { get; set; }

    public bool Activa { get; set; } = true;
}

public sealed class ActualizarPlantillaNotificacionRequestDto
{
    [Required]
    [MaxLength(32)]
    public string Canal { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string Asunto { get; set; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    public string CuerpoHtml { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? CuerpoTexto { get; set; }

    public bool Activa { get; set; } = true;
}
