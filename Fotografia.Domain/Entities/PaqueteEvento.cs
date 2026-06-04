namespace Fotografia.Domain.Entities;

public sealed class PaqueteEvento
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventoId { get; set; }
    public Evento? Evento { get; set; }

    public required string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public bool IncluyeTodasLasFotos { get; set; } = true;
    public bool Destacado { get; set; }
    public int? OrdenDestacado { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacionUtc { get; set; }

    public ICollection<CarritoItem> CarritoItems { get; set; } = [];
    public ICollection<PedidoItem> PedidoItems { get; set; } = [];
}
