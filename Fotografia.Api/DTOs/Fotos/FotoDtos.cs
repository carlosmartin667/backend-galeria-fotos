using System.ComponentModel.DataAnnotations;

namespace Fotografia.Api.DTOs.Fotos;

public sealed class CrearFotoMetadataRequestDto
{
    [Required]
    public Guid EventoId { get; set; }

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

public sealed class ActualizarFotoRequestDto
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

public sealed class FotoResponseDto
{
    public Guid Id { get; set; }
    public Guid EventoId { get; set; }
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
    public DateTime SubidaEnUtc { get; set; }
}

public sealed class GenerarStorageKeyRequestDto
{
    [Required]
    public Guid EventoId { get; set; }

    [Required]
    [MaxLength(260)]
    public string NombreArchivo { get; set; } = string.Empty;
}

public sealed class StorageKeyResponseDto
{
    public string StorageKey { get; set; } = string.Empty;
}
