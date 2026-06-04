namespace Fotografia.Domain.Entities;

public sealed class Bitacora
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? UsuarioId { get; set; }
    public string? UsuarioEmail { get; set; }
    public string? Rol { get; set; }
    public required string Accion { get; set; }
    public required string EntidadTipo { get; set; }
    public Guid? EntidadId { get; set; }
    public required string Descripcion { get; set; }
    public string? Ip { get; set; }
    public string? UserAgent { get; set; }
    public string? CorrelationId { get; set; }
    public string? RequestPath { get; set; }
    public string? HttpMethod { get; set; }
    public string? MetadataJson { get; set; }
    public required string Severidad { get; set; }
    public DateTime FechaUtc { get; set; } = DateTime.UtcNow;

    public Usuario? Usuario { get; set; }
}
