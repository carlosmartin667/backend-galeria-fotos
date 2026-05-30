namespace Fotografia.Api.Entities;

public sealed class Descarga
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PedidoId { get; set; }
    public Pedido? Pedido { get; set; }

    public Guid EventoId { get; set; }
    public Evento? Evento { get; set; }

    public Guid ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public Guid FotoId { get; set; }
    public Foto? Foto { get; set; }

    public required string StorageKey { get; set; }
    public required string NombreArchivo { get; set; }
    public DateTime ExpiraEnUtc { get; set; }
    public DateTime CreadoEnUtc { get; set; } = DateTime.UtcNow;
    public int DescargasRealizadas { get; set; }
}
