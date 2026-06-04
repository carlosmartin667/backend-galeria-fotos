using System.ComponentModel.DataAnnotations;

namespace Fotografia.Application.DTOs.NotasInternas;

public sealed class NotaInternaResponseDto
{
    public Guid Id { get; set; }
    public string EntidadTipo { get; set; } = string.Empty;
    public Guid EntidadId { get; set; }
    public string Texto { get; set; } = string.Empty;
    public Guid UsuarioId { get; set; }
    public string? UsuarioNombre { get; set; }
    public bool Activa { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
    public DateTime? FechaActualizacionUtc { get; set; }
}

public sealed class CrearNotaInternaRequestDto
{
    [Required]
    [MaxLength(2000)]
    public string Texto { get; set; } = string.Empty;
}

public sealed class ActualizarNotaInternaRequestDto
{
    [Required]
    [MaxLength(2000)]
    public string Texto { get; set; } = string.Empty;

    public bool Activa { get; set; } = true;
}
