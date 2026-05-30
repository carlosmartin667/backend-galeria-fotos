using System.ComponentModel.DataAnnotations;
using Fotografia.Api.DTOs.Fotos;

namespace Fotografia.Api.DTOs.Pedidos;

public sealed class CrearPedidoRequestDto
{
    [Required]
    public Guid EventoId { get; set; }

    [Required]
    public Guid ClienteId { get; set; }

    [MinLength(1)]
    public List<Guid> FotoIds { get; set; } = [];
}

public sealed class PedidoFotoResponseDto
{
    public Guid FotoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public FotoResponseDto? Foto { get; set; }
}

public sealed class PedidoResponseDto
{
    public Guid Id { get; set; }
    public Guid EventoId { get; set; }
    public Guid ClienteId { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public string? MercadoPagoPreferenceId { get; set; }
    public DateTime CreadoEnUtc { get; set; }
    public List<PedidoFotoResponseDto> Fotos { get; set; } = [];
}
