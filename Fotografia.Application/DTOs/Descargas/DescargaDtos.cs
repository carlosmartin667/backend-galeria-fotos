using System.ComponentModel.DataAnnotations;

namespace Fotografia.Application.DTOs.Descargas;

public sealed class CrearLinkDescargaRequestDto
{
    [Required]
    public Guid PedidoId { get; set; }

    public Guid? FotoId { get; set; }

    public Guid? FotoPrivadaId { get; set; }
}

public sealed class LinkDescargaResponseDto
{
    public Guid DescargaId { get; set; }
    public Guid PedidoId { get; set; }
    public Guid? FotoId { get; set; }
    public Guid? FotoPrivadaId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime ExpiraEnUtc { get; set; }
    public int? MaxDescargas { get; set; }
    public int DescargasRealizadas { get; set; }
    public DateTime? UltimaDescargaUtc { get; set; }
}

public sealed class DescargaResponseDto
{
    public Guid Id { get; set; }
    public Guid PedidoId { get; set; }
    public Guid? EventoId { get; set; }
    public Guid ClienteId { get; set; }
    public Guid? FotoId { get; set; }
    public Guid? FotoPrivadaId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public DateTime ExpiraEnUtc { get; set; }
    public DateTime CreadoEnUtc { get; set; }
    public DateTime? FechaActualizacionUtc { get; set; }
    public int? MaxDescargas { get; set; }
    public int DescargasRealizadas { get; set; }
    public DateTime? UltimaDescargaUtc { get; set; }
    public bool Activa { get; set; }
}

public sealed class RegenerarDescargaResponseDto
{
    public Guid DescargaId { get; set; }
    public Guid DescargaAnteriorId { get; set; }
    public string Url { get; set; } = string.Empty;
    public DateTime ExpiraEnUtc { get; set; }
    public int? MaxDescargas { get; set; }
    public int DescargasRealizadas { get; set; }
}
