using System.ComponentModel.DataAnnotations;

namespace Fotografia.Application.DTOs.SesionesPrivadas;

public sealed class SesionPrivadaResponseDto
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTime FechaSesionUtc { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal? PrecioPaquete { get; set; }
    public bool Activa { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
    public DateTime? FechaActualizacionUtc { get; set; }
    public int CantidadFotos { get; set; }
}

public sealed class CrearSesionPrivadaRequestDto
{
    [Required]
    public Guid ClienteId { get; set; }

    [Required]
    [MaxLength(180)]
    public string Titulo { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Descripcion { get; set; }

    [Required]
    public DateTime FechaSesionUtc { get; set; }

    [MaxLength(64)]
    public string Estado { get; set; } = "Activa";

    [Range(0, 999999999)]
    public decimal? PrecioPaquete { get; set; }
}

public sealed class ActualizarSesionPrivadaRequestDto
{
    [Required]
    [MaxLength(180)]
    public string Titulo { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Descripcion { get; set; }

    [Required]
    public DateTime FechaSesionUtc { get; set; }

    [MaxLength(64)]
    public string Estado { get; set; } = "Activa";

    [Range(0, 999999999)]
    public decimal? PrecioPaquete { get; set; }

    public bool Activa { get; set; } = true;
}

public sealed class FotoPrivadaResponseDto
{
    public Guid Id { get; set; }
    public Guid SesionPrivadaId { get; set; }
    public Guid ClienteId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public string? PreviewUrl { get; set; }
    public string? MarcaAguaStorageKey { get; set; }
    public long SizeInBytes { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public decimal PrecioUnitario { get; set; }
    public bool Activa { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
    public DateTime? FechaActualizacionUtc { get; set; }
}

public sealed class CrearFotoPrivadaMetadataRequestDto
{
    [Required]
    [MaxLength(260)]
    public string NombreArchivo { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string ContentType { get; set; } = string.Empty;

    [Required]
    [MaxLength(700)]
    public string StorageKey { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? PreviewUrl { get; set; }

    [MaxLength(700)]
    public string? MarcaAguaStorageKey { get; set; }

    public long SizeInBytes { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public decimal PrecioUnitario { get; set; }
}

public sealed class ActualizarFotoPrivadaRequestDto
{
    [Required]
    [MaxLength(260)]
    public string NombreArchivo { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? PreviewUrl { get; set; }

    [MaxLength(700)]
    public string? MarcaAguaStorageKey { get; set; }

    public decimal PrecioUnitario { get; set; }
    public bool Activa { get; set; } = true;
}

public sealed class GenerarStorageKeyFotoPrivadaRequestDto
{
    [Required]
    [MaxLength(260)]
    public string NombreArchivo { get; set; } = string.Empty;
}
