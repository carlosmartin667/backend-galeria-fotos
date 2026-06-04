using System.ComponentModel.DataAnnotations;

namespace Fotografia.Application.DTOs.Cupones;

public sealed class CuponDescuentoResponseDto
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string TipoDescuento { get; set; } = string.Empty;
    public decimal ValorDescuento { get; set; }
    public decimal? MontoMinimoCompra { get; set; }
    public decimal? MontoMaximoDescuento { get; set; }
    public DateTime? FechaInicioUtc { get; set; }
    public DateTime? FechaFinUtc { get; set; }
    public int? UsosMaximos { get; set; }
    public int UsosActuales { get; set; }
    public int? UsosMaximosPorUsuario { get; set; }
    public bool SoloPrimerCompra { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
    public DateTime? FechaActualizacionUtc { get; set; }
}

public sealed class CrearCuponDescuentoRequestDto
{
    [Required]
    [MaxLength(64)]
    public string Codigo { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    [Required]
    [MaxLength(32)]
    public string TipoDescuento { get; set; } = string.Empty;

    public decimal ValorDescuento { get; set; }
    public decimal? MontoMinimoCompra { get; set; }
    public decimal? MontoMaximoDescuento { get; set; }
    public DateTime? FechaInicioUtc { get; set; }
    public DateTime? FechaFinUtc { get; set; }
    public int? UsosMaximos { get; set; }
    public int? UsosMaximosPorUsuario { get; set; }
    public bool SoloPrimerCompra { get; set; }
    public bool Activo { get; set; } = true;
}

public sealed class ActualizarCuponDescuentoRequestDto
{
    [MaxLength(500)]
    public string? Descripcion { get; set; }

    [Required]
    [MaxLength(32)]
    public string TipoDescuento { get; set; } = string.Empty;

    public decimal ValorDescuento { get; set; }
    public decimal? MontoMinimoCompra { get; set; }
    public decimal? MontoMaximoDescuento { get; set; }
    public DateTime? FechaInicioUtc { get; set; }
    public DateTime? FechaFinUtc { get; set; }
    public int? UsosMaximos { get; set; }
    public int? UsosMaximosPorUsuario { get; set; }
    public bool SoloPrimerCompra { get; set; }
    public bool Activo { get; set; } = true;
}

public sealed class ValidarCuponRequestDto
{
    [Required]
    [MaxLength(64)]
    public string Codigo { get; set; } = string.Empty;

    public decimal? Subtotal { get; set; }
}

public sealed class CuponValidacionResponseDto
{
    public bool Valido { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string? Mensaje { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal TotalFinal { get; set; }
    public Guid? CuponDescuentoId { get; set; }
}

public sealed class CuponUsoResponseDto
{
    public Guid Id { get; set; }
    public Guid CuponDescuentoId { get; set; }
    public Guid PedidoId { get; set; }
    public Guid? ClienteId { get; set; }
    public Guid? UsuarioId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public decimal MontoDescuento { get; set; }
    public DateTime FechaUsoUtc { get; set; }
    public bool Confirmado { get; set; }
}
