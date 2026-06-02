namespace Fotografia.Domain.Entities;

public sealed class Testimonio
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string NombreCliente { get; set; }
    public string? EmailCliente { get; set; }
    public required string Texto { get; set; }
    public int Calificacion { get; set; }
    public string? ImagenUrl { get; set; }
    public bool Publicado { get; set; }
    public bool Destacado { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacionUtc { get; set; }

    public Guid? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public Guid? PedidoId { get; set; }
    public Pedido? Pedido { get; set; }

    public Guid? ServicioFotografiaId { get; set; }
    public ServicioFotografia? ServicioFotografia { get; set; }

    public Guid? EventoId { get; set; }
    public Evento? Evento { get; set; }
}
