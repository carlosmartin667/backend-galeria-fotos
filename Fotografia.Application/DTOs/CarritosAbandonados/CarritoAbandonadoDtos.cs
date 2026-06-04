namespace Fotografia.Application.DTOs.CarritosAbandonados;

public sealed class CarritoAbandonadoResponseDto
{
    public Guid Id { get; set; }
    public Guid CarritoCompraId { get; set; }
    public Guid? UsuarioId { get; set; }
    public string? UsuarioEmail { get; set; }
    public Guid? ClienteId { get; set; }
    public string? ClienteNombre { get; set; }
    public DateTime FechaDetectadoUtc { get; set; }
    public DateTime? FechaUltimaNotificacionUtc { get; set; }
    public int NotificacionesEnviadas { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public int CantidadItems { get; set; }
    public decimal TotalEstimado { get; set; }
}

public sealed class CarritosAbandonadosResumenDto
{
    public int Detectados { get; set; }
    public int Notificados { get; set; }
    public int Recuperados { get; set; }
    public int Cerrados { get; set; }
    public int TotalActivos { get; set; }
}
