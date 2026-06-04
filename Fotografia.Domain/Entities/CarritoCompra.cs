using Fotografia.Domain.Constants;

namespace Fotografia.Domain.Entities;

public sealed class CarritoCompra
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public string Estado { get; set; } = CarritoEstados.Activo;
    public string? CuponCodigo { get; set; }
    public Guid? CuponDescuentoId { get; set; }
    public CuponDescuento? CuponDescuento { get; set; }
    public DateTime? FechaUltimoRecordatorioUtc { get; set; }
    public int RecordatoriosEnviados { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacionUtc { get; set; }

    public ICollection<CarritoItem> Items { get; set; } = [];
    public ICollection<CarritoAbandonadoRegistro> RegistrosAbandono { get; set; } = [];
}
