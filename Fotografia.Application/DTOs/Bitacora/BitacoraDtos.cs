using System.ComponentModel.DataAnnotations;
using Fotografia.Application.DTOs.Common;

namespace Fotografia.Application.DTOs.Bitacora;

public sealed class BitacoraResponseDto
{
    public Guid Id { get; set; }
    public Guid? UsuarioId { get; set; }
    public string? UsuarioEmail { get; set; }
    public string? Rol { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string EntidadTipo { get; set; } = string.Empty;
    public Guid? EntidadId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? Ip { get; set; }
    public string? UserAgent { get; set; }
    public string? CorrelationId { get; set; }
    public string? RequestPath { get; set; }
    public string? HttpMethod { get; set; }
    public string? MetadataJson { get; set; }
    public string Severidad { get; set; } = string.Empty;
    public DateTime FechaUtc { get; set; }
}

public sealed class BitacoraQueryDto
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public Guid? UsuarioId { get; set; }
    [MaxLength(256)]
    public string? UsuarioEmail { get; set; }
    [MaxLength(120)]
    public string? Accion { get; set; }
    [MaxLength(80)]
    public string? EntidadTipo { get; set; }
    public Guid? EntidadId { get; set; }
    [MaxLength(32)]
    public string? Severidad { get; set; }
    [MaxLength(120)]
    public string? CorrelationId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;

    public string? Validate()
    {
        if (Page < 1)
        {
            return "page debe ser mayor o igual a 1.";
        }

        if (PageSize < 1 || PageSize > 100)
        {
            return "pageSize debe estar entre 1 y 100.";
        }

        if (Desde is not null && Hasta is not null && Hasta < Desde)
        {
            return "hasta debe ser mayor o igual a desde.";
        }

        return null;
    }
}

public sealed class CrearBitacoraRequestDto
{
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
    public object? Metadata { get; set; }
    public string? MetadataJson { get; set; }
    public string Severidad { get; set; } = "Info";
}

public sealed class BitacoraResumenDto
{
    public int Total { get; set; }
    public IReadOnlyDictionary<string, int> PorSeveridad { get; set; } = new Dictionary<string, int>();
    public IReadOnlyDictionary<string, int> PorAccion { get; set; } = new Dictionary<string, int>();
}
