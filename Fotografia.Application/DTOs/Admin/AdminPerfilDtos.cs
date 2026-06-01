using System.ComponentModel.DataAnnotations;

namespace Fotografia.Application.DTOs.Admin;

public sealed class AdminPerfilPublicoResponseDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? WhatsApp { get; set; }
    public string? Instagram { get; set; }
    public string? CorreoPublico { get; set; }
    public string? Direccion { get; set; }
}

public sealed class AdminPerfilResponseDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? WhatsApp { get; set; }
    public string? Instagram { get; set; }
    public string? CorreoPublico { get; set; }
    public string? Direccion { get; set; }
}

public sealed class ActualizarAdminPerfilRequestDto
{
    [Required]
    [MaxLength(160)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Descripcion { get; set; }

    [MaxLength(64)]
    public string? WhatsApp { get; set; }

    [MaxLength(120)]
    public string? Instagram { get; set; }

    [EmailAddress]
    [MaxLength(160)]
    public string? CorreoPublico { get; set; }

    [MaxLength(300)]
    public string? Direccion { get; set; }
}
