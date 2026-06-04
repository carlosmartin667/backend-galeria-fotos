namespace Fotografia.Domain.Entities;

public sealed class Promocion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Titulo { get; set; }
    public string? Descripcion { get; set; }
    public string? ImagenUrl { get; set; }
    public required string Tipo { get; set; }
    public DateTime? FechaInicioUtc { get; set; }
    public DateTime? FechaFinUtc { get; set; }
    public bool Activa { get; set; } = true;
    public bool Destacada { get; set; }
    public int Orden { get; set; }

    public Guid? CuponDescuentoId { get; set; }
    public CuponDescuento? CuponDescuento { get; set; }

    public Guid? ServicioFotografiaId { get; set; }
    public ServicioFotografia? ServicioFotografia { get; set; }

    public Guid? EventoId { get; set; }
    public Evento? Evento { get; set; }

    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacionUtc { get; set; }
}
