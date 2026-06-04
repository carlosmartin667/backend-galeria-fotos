namespace Fotografia.Domain.Entities;

public sealed class PedidoEstadoHistorial
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PedidoId { get; set; }
    public Pedido? Pedido { get; set; }

    public required string EstadoAnterior { get; set; }
    public required string EstadoNuevo { get; set; }
    public string? Comentario { get; set; }

    public Guid? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public DateTime FechaCambioUtc { get; set; } = DateTime.UtcNow;
}
