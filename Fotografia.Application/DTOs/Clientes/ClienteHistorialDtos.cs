namespace Fotografia.Application.DTOs.Clientes;

public sealed class ClienteHistorialResponseDto
{
    public ClienteResponseDto Cliente { get; set; } = new();
    public ClienteHistorialUsuarioDto? Usuario { get; set; }
    public List<ClienteHistorialEventoDto> Eventos { get; set; } = [];
    public List<ClienteHistorialSesionPrivadaDto> SesionesPrivadas { get; set; } = [];
    public List<ClienteHistorialFotoPrivadaDto> FotosPrivadas { get; set; } = [];
    public List<ClienteHistorialPedidoDto> Pedidos { get; set; } = [];
    public List<ClienteHistorialPagoDto> Pagos { get; set; } = [];
    public List<ClienteHistorialDescargaDto> Descargas { get; set; } = [];
    public List<ClienteHistorialFavoritoEventoDto> FavoritosEventos { get; set; } = [];
    public List<ClienteHistorialFavoritoFotoDto> FavoritosFotos { get; set; } = [];
    public List<ClienteHistorialComentarioDto> Comentarios { get; set; } = [];
    public List<ClienteHistorialSolicitudPresupuestoDto> SolicitudesPresupuesto { get; set; } = [];
    public List<ClienteHistorialAgendaItemDto> AgendaItems { get; set; } = [];
    public ClienteHistorialTotalesDto Totales { get; set; } = new();
}

public sealed class ClienteHistorialUsuarioDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public sealed class ClienteHistorialEventoDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaEventoUtc { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Visibilidad { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public sealed class ClienteHistorialSesionPrivadaDto
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public DateTime FechaSesionUtc { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool Activa { get; set; }
    public int CantidadFotos { get; set; }
}

public sealed class ClienteHistorialFotoPrivadaDto
{
    public Guid Id { get; set; }
    public Guid SesionPrivadaId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string? PreviewUrl { get; set; }
    public decimal PrecioUnitario { get; set; }
    public bool Activa { get; set; }
}

public sealed class ClienteHistorialPedidoDto
{
    public Guid Id { get; set; }
    public Guid? EventoId { get; set; }
    public string? EventoNombre { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public DateTime CreadoEnUtc { get; set; }
    public DateTime? ActualizadoEnUtc { get; set; }
    public List<ClienteHistorialPedidoItemDto> Items { get; set; } = [];
}

public sealed class ClienteHistorialPedidoItemDto
{
    public Guid Id { get; set; }
    public string TipoItem { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
}

public sealed class ClienteHistorialPagoDto
{
    public Guid Id { get; set; }
    public Guid PedidoId { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public DateTime CreadoEnUtc { get; set; }
    public DateTime? PagadoEnUtc { get; set; }
}

public sealed class ClienteHistorialDescargaDto
{
    public Guid Id { get; set; }
    public Guid PedidoId { get; set; }
    public Guid? FotoId { get; set; }
    public Guid? FotoPrivadaId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public DateTime ExpiraEnUtc { get; set; }
    public int? MaxDescargas { get; set; }
    public int DescargasRealizadas { get; set; }
    public DateTime? UltimaDescargaUtc { get; set; }
    public bool Activa { get; set; }
}

public sealed class ClienteHistorialFavoritoEventoDto
{
    public Guid Id { get; set; }
    public Guid EventoId { get; set; }
    public string NombreEvento { get; set; } = string.Empty;
    public DateTime FechaCreacionUtc { get; set; }
}

public sealed class ClienteHistorialFavoritoFotoDto
{
    public Guid Id { get; set; }
    public Guid FotoId { get; set; }
    public Guid EventoId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string? PreviewUrl { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
}

public sealed class ClienteHistorialComentarioDto
{
    public Guid Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public Guid EntidadId { get; set; }
    public string? EntidadNombre { get; set; }
    public string Texto { get; set; } = string.Empty;
    public DateTime FechaCreacionUtc { get; set; }
    public bool Activo { get; set; }
}

public sealed class ClienteHistorialSolicitudPresupuestoDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? WhatsApp { get; set; }
    public string? TipoEvento { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool Activa { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
}

public sealed class ClienteHistorialAgendaItemDto
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaInicioUtc { get; set; }
    public DateTime FechaFinUtc { get; set; }
    public bool Activo { get; set; }
}

public sealed class ClienteHistorialTotalesDto
{
    public int TotalPedidos { get; set; }
    public decimal TotalGastado { get; set; }
    public int TotalDescargas { get; set; }
    public int TotalSesionesPrivadas { get; set; }
    public int TotalEventos { get; set; }
    public int TotalSolicitudes { get; set; }
    public DateTime? UltimaCompraUtc { get; set; }
    public DateTime? UltimaActividadUtc { get; set; }
}
