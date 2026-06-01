namespace Fotografia.Application.DTOs.Carrito;

public sealed class CarritoItemResponseDto
{
    public Guid Id { get; set; }
    public string TipoItem { get; set; } = string.Empty;
    public Guid? FotoId { get; set; }
    public Guid? PaqueteEventoId { get; set; }
    public Guid? FotoPrivadaId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaCreacionUtc { get; set; }
}

public sealed class CarritoResponseDto
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
    public DateTime? FechaActualizacionUtc { get; set; }
    public List<CarritoItemResponseDto> Items { get; set; } = [];
}
