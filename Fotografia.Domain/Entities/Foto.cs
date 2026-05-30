namespace Fotografia.Domain.Entities;

public sealed class Foto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventoId { get; set; }
    public Evento? Evento { get; set; }

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
    public DateTime SubidaEnUtc { get; set; } = DateTime.UtcNow;

    public ICollection<PedidoFoto> PedidoFotos { get; set; } = [];
}
