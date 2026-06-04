namespace Fotografia.Domain.Entities;

public sealed class Notificacion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Tipo { get; set; }
    public required string Canal { get; set; }
    public required string Estado { get; set; }
    public required string Titulo { get; set; }
    public required string Mensaje { get; set; }
    public string? DestinatarioEmail { get; set; }

    public Guid? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public string? EntidadTipo { get; set; }
    public Guid? EntidadId { get; set; }
    public string? CorrelationKey { get; set; }

    public int Intentos { get; set; }
    public int MaxIntentos { get; set; } = 3;
    public string? Error { get; set; }
    public bool Leida { get; set; }
    public DateTime? FechaLecturaUtc { get; set; }
    public DateTime? ProgramadaParaUtc { get; set; }
    public DateTime? EnviadaEnUtc { get; set; }
    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacionUtc { get; set; }
    public bool Activa { get; set; } = true;
}
