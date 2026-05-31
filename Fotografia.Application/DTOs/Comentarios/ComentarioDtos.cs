using System.ComponentModel.DataAnnotations;

namespace Fotografia.Application.DTOs.Comentarios;

public sealed class ComentarioRequestDto
{
    [Required]
    [MaxLength(1000)]
    public string Texto { get; set; } = string.Empty;
}

public sealed class ComentarioResponseDto
{
    public Guid Id { get; set; }
    public string Texto { get; set; } = string.Empty;
    public Guid UsuarioId { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public DateTime FechaCreacionUtc { get; set; }
    public DateTime? FechaActualizacionUtc { get; set; }
    public bool Activo { get; set; }
}
