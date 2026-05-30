using System.ComponentModel.DataAnnotations;

namespace Fotografia.Application.DTOs.Clientes;

public sealed class CrearClienteRequestDto
{
    [Required]
    [MaxLength(160)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(64)]
    public string? Telefono { get; set; }

    [MaxLength(64)]
    public string? Documento { get; set; }
}

public sealed class ActualizarClienteRequestDto
{
    [Required]
    [MaxLength(160)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(64)]
    public string? Telefono { get; set; }

    [MaxLength(64)]
    public string? Documento { get; set; }
}

public sealed class ClienteResponseDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Documento { get; set; }
    public DateTime CreadoEnUtc { get; set; }
}
