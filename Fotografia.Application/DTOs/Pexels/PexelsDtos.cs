using System.ComponentModel.DataAnnotations;

namespace Fotografia.Application.DTOs.Pexels;

public sealed class ImportarFotosPexelsRequestDto
{
    [Required]
    public Guid EventoId { get; set; }

    [Required]
    [MaxLength(120)]
    public string Query { get; set; } = string.Empty;

    [Range(1, 200)]
    public int Cantidad { get; set; }

    [Range(0, 999999999)]
    public decimal PrecioUnitario { get; set; }
}

public sealed class ImportarFotosPexelsResponseDto
{
    public Guid EventoId { get; set; }
    public string Query { get; set; } = string.Empty;
    public int CantidadSolicitada { get; set; }
    public int CantidadImportada { get; set; }
    public int CantidadDuplicada { get; set; }
    public List<FotoImportadaPexelsDto> Fotos { get; set; } = [];
}

public sealed class FotoImportadaPexelsDto
{
    public Guid FotoId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string? PreviewUrl { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
}

public sealed class PexelsPhotoDto
{
    public long Id { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string? OriginalUrl { get; set; }
    public string? PreviewUrl { get; set; }
    public string? Photographer { get; set; }
    public string? PhotographerUrl { get; set; }
}
