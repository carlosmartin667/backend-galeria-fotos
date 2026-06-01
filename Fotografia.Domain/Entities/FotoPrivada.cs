namespace Fotografia.Domain.Entities;

public sealed class FotoPrivada
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SesionPrivadaId { get; set; }
    public SesionPrivada? SesionPrivada { get; set; }

    public Guid ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public required string NombreArchivo { get; set; }
    public required string ContentType { get; set; }
    public required string StorageKey { get; set; }
    public string? PreviewUrl { get; set; }
    public string? MarcaAguaStorageKey { get; set; }
    public long SizeInBytes { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public decimal PrecioUnitario { get; set; }
    public bool Activa { get; set; } = true;
    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacionUtc { get; set; }

    public ICollection<CarritoItem> CarritoItems { get; set; } = [];
    public ICollection<PedidoItem> PedidoItems { get; set; } = [];
}
