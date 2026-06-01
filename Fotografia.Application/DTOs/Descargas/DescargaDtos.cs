using System.ComponentModel.DataAnnotations;

namespace Fotografia.Application.DTOs.Descargas;

public sealed class CrearLinkDescargaRequestDto
{
    [Required]
    public Guid PedidoId { get; set; }

    [Required]
    public Guid FotoId { get; set; }
}

public sealed class LinkDescargaResponseDto
{
    public Guid DescargaId { get; set; }
    public Guid PedidoId { get; set; }
    public Guid FotoId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime ExpiraEnUtc { get; set; }
}
