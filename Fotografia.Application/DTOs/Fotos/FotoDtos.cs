using System.ComponentModel.DataAnnotations;

namespace Fotografia.Application.DTOs.Fotos;

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
    public bool? TieneMarcaAgua { get; set; }
    public bool? Procesada { get; set; }
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
    public bool? TieneMarcaAgua { get; set; }
    public bool? Procesada { get; set; }
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
    public bool TieneMarcaAgua { get; set; }
    public bool Procesada { get; set; }
    public bool Activa { get; set; }
    public DateTime SubidaEnUtc { get; set; }
    public DateTime? FechaActualizacionUtc { get; set; }
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

public sealed class CrearFotoMetadataBulkRequestDto
{
    [Required]
    [MinLength(1)]
    public List<CrearFotoMetadataRequestDto> Fotos { get; set; } = [];
}

public sealed class FotoMetadataBulkResponseDto
{
    public int Solicitadas { get; set; }
    public int Creadas { get; set; }
    public int Omitidas { get; set; }
    public int Errores { get; set; }
    public List<FotoResponseDto> FotosCreadas { get; set; } = [];
    public List<FotoBulkOmitidaDto> OmitidasDetalle { get; set; } = [];
    public List<FotoBulkErrorDto> ErroresDetalle { get; set; } = [];
}

public sealed class FotoBulkOmitidaDto
{
    public int Index { get; set; }
    public Guid EventoId { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
}

public sealed class FotoBulkErrorDto
{
    public int Index { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}

public sealed class GenerarStorageKeysBulkRequestDto
{
    [Required]
    public Guid EventoId { get; set; }

    [Required]
    [MinLength(1)]
    public List<string> NombresArchivo { get; set; } = [];
}

public sealed class StorageKeysBulkResponseDto
{
    public Guid EventoId { get; set; }
    public int Solicitadas { get; set; }
    public int Generadas { get; set; }
    public int Errores { get; set; }
    public List<StorageKeyBulkItemDto> Items { get; set; } = [];
    public List<FotoBulkErrorDto> ErroresDetalle { get; set; } = [];
}

public sealed class StorageKeyBulkItemDto
{
    public int Index { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
}
