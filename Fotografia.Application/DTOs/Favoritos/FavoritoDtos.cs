namespace Fotografia.Application.DTOs.Favoritos;

public sealed class FavoritoEventoResponseDto
{
    public Guid Id { get; set; }
    public Guid EventoId { get; set; }
    public string NombreEvento { get; set; } = string.Empty;
    public DateTime FechaEventoUtc { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
}

public sealed class FavoritoFotoResponseDto
{
    public Guid Id { get; set; }
    public Guid FotoId { get; set; }
    public Guid EventoId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string? PreviewUrl { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
}
