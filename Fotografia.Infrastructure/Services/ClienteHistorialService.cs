using Fotografia.Application.DTOs.Clientes;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Services;

public sealed class ClienteHistorialService(
    AppDbContext dbContext,
    ICurrentUserService currentUser) : IClienteHistorialService
{
    private const int MaxItemsPerSection = 50;

    public async Task<ApiResponse<ClienteHistorialResponseDto>> GetHistorialAsync(
        Guid clienteId,
        CancellationToken cancellationToken = default)
    {
        var cliente = await dbContext.Clientes
            .AsNoTracking()
            .Include(x => x.Usuario)
            .FirstOrDefaultAsync(x => x.Id == clienteId, cancellationToken);

        if (cliente is null)
        {
            return ApiResponse<ClienteHistorialResponseDto>.NotFound("Cliente no encontrado.");
        }

        if (!CanAccess(cliente))
        {
            return ApiResponse<ClienteHistorialResponseDto>.Forbidden("No puede consultar el historial de otro cliente.");
        }

        return ApiResponse<ClienteHistorialResponseDto>.Ok(
            await BuildHistorialAsync(cliente, cancellationToken));
    }

    public async Task<ApiResponse<ClienteHistorialResponseDto>> GetMiHistorialAsync(
        CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is not Guid userId)
        {
            return ApiResponse<ClienteHistorialResponseDto>.Forbidden("Debe iniciar sesion.");
        }

        var cliente = await dbContext.Clientes
            .AsNoTracking()
            .Include(x => x.Usuario)
            .FirstOrDefaultAsync(x => x.UsuarioId == userId, cancellationToken);

        if (cliente is null)
        {
            return ApiResponse<ClienteHistorialResponseDto>.NotFound("No hay un cliente asociado al usuario actual.");
        }

        return ApiResponse<ClienteHistorialResponseDto>.Ok(
            await BuildHistorialAsync(cliente, cancellationToken));
    }

    private async Task<ClienteHistorialResponseDto> BuildHistorialAsync(
        Cliente cliente,
        CancellationToken cancellationToken)
    {
        var usuarioId = cliente.UsuarioId;
        var normalizedEmail = cliente.Email.Trim().ToLowerInvariant();
        var telefono = Normalize(cliente.Telefono);

        var eventos = await dbContext.Eventos
            .AsNoTracking()
            .Where(x => x.ClientePrincipalId == cliente.Id)
            .OrderByDescending(x => x.FechaEventoUtc)
            .Take(MaxItemsPerSection)
            .Select(x => new ClienteHistorialEventoDto
            {
                Id = x.Id,
                Nombre = x.Nombre,
                FechaEventoUtc = x.FechaEventoUtc,
                Estado = x.Estado,
                Visibilidad = x.Visibilidad,
                Activo = x.Activo
            })
            .ToListAsync(cancellationToken);

        var sesiones = await dbContext.SesionesPrivadas
            .AsNoTracking()
            .Where(x => x.ClienteId == cliente.Id)
            .OrderByDescending(x => x.FechaSesionUtc)
            .Take(MaxItemsPerSection)
            .Select(x => new ClienteHistorialSesionPrivadaDto
            {
                Id = x.Id,
                Titulo = x.Titulo,
                FechaSesionUtc = x.FechaSesionUtc,
                Estado = x.Estado,
                Activa = x.Activa,
                CantidadFotos = x.Fotos.Count(foto => foto.Activa)
            })
            .ToListAsync(cancellationToken);

        var fotosPrivadas = await dbContext.FotosPrivadas
            .AsNoTracking()
            .Where(x => x.ClienteId == cliente.Id)
            .OrderByDescending(x => x.FechaCreacionUtc)
            .Take(MaxItemsPerSection)
            .Select(x => new ClienteHistorialFotoPrivadaDto
            {
                Id = x.Id,
                SesionPrivadaId = x.SesionPrivadaId,
                NombreArchivo = x.NombreArchivo,
                PreviewUrl = x.PreviewUrl,
                PrecioUnitario = x.PrecioUnitario,
                Activa = x.Activa
            })
            .ToListAsync(cancellationToken);

        var pedidos = await dbContext.Pedidos
            .AsNoTracking()
            .Include(x => x.Evento)
            .Include(x => x.PedidoItems)
            .Where(x => x.ClienteId == cliente.Id)
            .OrderByDescending(x => x.CreadoEnUtc)
            .Take(MaxItemsPerSection)
            .ToListAsync(cancellationToken);

        var pagos = await dbContext.Pagos
            .AsNoTracking()
            .Include(x => x.Pedido)
            .Where(x => x.Pedido != null && x.Pedido.ClienteId == cliente.Id)
            .OrderByDescending(x => x.CreadoEnUtc)
            .Take(MaxItemsPerSection)
            .Select(x => new ClienteHistorialPagoDto
            {
                Id = x.Id,
                PedidoId = x.PedidoId,
                Estado = x.Estado,
                Monto = x.Monto,
                Moneda = x.Moneda,
                CreadoEnUtc = x.CreadoEnUtc,
                PagadoEnUtc = x.PagadoEnUtc
            })
            .ToListAsync(cancellationToken);

        var descargas = await dbContext.Descargas
            .AsNoTracking()
            .Where(x => x.ClienteId == cliente.Id)
            .OrderByDescending(x => x.CreadoEnUtc)
            .Take(MaxItemsPerSection)
            .Select(x => new ClienteHistorialDescargaDto
            {
                Id = x.Id,
                PedidoId = x.PedidoId,
                FotoId = x.FotoId,
                FotoPrivadaId = x.FotoPrivadaId,
                NombreArchivo = x.NombreArchivo,
                ExpiraEnUtc = x.ExpiraEnUtc,
                MaxDescargas = x.MaxDescargas,
                DescargasRealizadas = x.DescargasRealizadas,
                UltimaDescargaUtc = x.UltimaDescargaUtc,
                Activa = x.Activa
            })
            .ToListAsync(cancellationToken);

        List<ClienteHistorialFavoritoEventoDto> favoritosEventos = usuarioId is Guid usuario
            ? await GetFavoritosEventosAsync(usuario, cancellationToken)
            : [];

        List<ClienteHistorialFavoritoFotoDto> favoritosFotos = usuarioId is Guid usuarioFotos
            ? await GetFavoritosFotosAsync(usuarioFotos, cancellationToken)
            : [];

        List<ClienteHistorialComentarioDto> comentarios = usuarioId is Guid usuarioComentarios
            ? await GetComentariosAsync(usuarioComentarios, cancellationToken)
            : [];

        var solicitudesQuery = dbContext.SolicitudesPresupuesto.AsNoTracking();
        solicitudesQuery = string.IsNullOrWhiteSpace(telefono)
            ? solicitudesQuery.Where(x => x.Email == normalizedEmail)
            : solicitudesQuery.Where(x => x.Email == normalizedEmail || x.WhatsApp == telefono);

        var solicitudes = await solicitudesQuery
            .OrderByDescending(x => x.FechaCreacionUtc)
            .Take(MaxItemsPerSection)
            .Select(x => new ClienteHistorialSolicitudPresupuestoDto
            {
                Id = x.Id,
                Nombre = x.Nombre,
                Email = x.Email,
                WhatsApp = x.WhatsApp,
                TipoEvento = x.TipoEvento,
                Estado = x.Estado,
                Activa = x.Activa,
                FechaCreacionUtc = x.FechaCreacionUtc
            })
            .ToListAsync(cancellationToken);

        var solicitudIds = solicitudes.Select(x => x.Id).ToList();
        var agenda = await dbContext.AgendaItems
            .AsNoTracking()
            .Where(x => x.ClienteId == cliente.Id || (x.SolicitudPresupuestoId != null && solicitudIds.Contains(x.SolicitudPresupuestoId.Value)))
            .OrderByDescending(x => x.FechaInicioUtc)
            .Take(MaxItemsPerSection)
            .Select(x => new ClienteHistorialAgendaItemDto
            {
                Id = x.Id,
                Titulo = x.Titulo,
                Tipo = x.Tipo,
                Estado = x.Estado,
                FechaInicioUtc = x.FechaInicioUtc,
                FechaFinUtc = x.FechaFinUtc,
                Activo = x.Activo
            })
            .ToListAsync(cancellationToken);

        var pedidoDtos = pedidos.Select(MapPedido).ToList();

        return new ClienteHistorialResponseDto
        {
            Cliente = new ClienteResponseDto
            {
                Id = cliente.Id,
                UsuarioId = cliente.UsuarioId,
                Nombre = cliente.Nombre,
                Email = cliente.Email,
                Telefono = cliente.Telefono,
                Documento = cliente.Documento,
                CreadoEnUtc = cliente.CreadoEnUtc
            },
            Usuario = cliente.Usuario is null
                ? null
                : new ClienteHistorialUsuarioDto
                {
                    Id = cliente.Usuario.Id,
                    Nombre = cliente.Usuario.Nombre,
                    Email = cliente.Usuario.Email,
                    Rol = cliente.Usuario.Rol,
                    Activo = cliente.Usuario.Activo
                },
            Eventos = eventos,
            SesionesPrivadas = sesiones,
            FotosPrivadas = fotosPrivadas,
            Pedidos = pedidoDtos,
            Pagos = pagos,
            Descargas = descargas,
            FavoritosEventos = favoritosEventos,
            FavoritosFotos = favoritosFotos,
            Comentarios = comentarios,
            SolicitudesPresupuesto = solicitudes,
            AgendaItems = agenda,
            Totales = BuildTotales(eventos, sesiones, pedidos, pagos, descargas, solicitudes, agenda)
        };
    }

    private async Task<List<ClienteHistorialFavoritoEventoDto>> GetFavoritosEventosAsync(
        Guid usuarioId,
        CancellationToken cancellationToken)
    {
        return await dbContext.EventosFavoritos
            .AsNoTracking()
            .Include(x => x.Evento)
            .Where(x => x.UsuarioId == usuarioId)
            .OrderByDescending(x => x.FechaCreacionUtc)
            .Take(MaxItemsPerSection)
            .Select(x => new ClienteHistorialFavoritoEventoDto
            {
                Id = x.Id,
                EventoId = x.EventoId,
                NombreEvento = x.Evento == null ? string.Empty : x.Evento.Nombre,
                FechaCreacionUtc = x.FechaCreacionUtc
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<List<ClienteHistorialFavoritoFotoDto>> GetFavoritosFotosAsync(
        Guid usuarioId,
        CancellationToken cancellationToken)
    {
        return await dbContext.FotosFavoritas
            .AsNoTracking()
            .Include(x => x.Foto)
            .Where(x => x.UsuarioId == usuarioId && x.Foto != null)
            .OrderByDescending(x => x.FechaCreacionUtc)
            .Take(MaxItemsPerSection)
            .Select(x => new ClienteHistorialFavoritoFotoDto
            {
                Id = x.Id,
                FotoId = x.FotoId,
                EventoId = x.Foto == null ? default : x.Foto.EventoId,
                NombreArchivo = x.Foto == null ? string.Empty : x.Foto.NombreArchivo,
                PreviewUrl = x.Foto == null ? null : x.Foto.PreviewUrl,
                FechaCreacionUtc = x.FechaCreacionUtc
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<List<ClienteHistorialComentarioDto>> GetComentariosAsync(
        Guid usuarioId,
        CancellationToken cancellationToken)
    {
        var eventos = await dbContext.ComentariosEventos
            .AsNoTracking()
            .Include(x => x.Evento)
            .Where(x => x.UsuarioId == usuarioId)
            .OrderByDescending(x => x.FechaCreacionUtc)
            .Take(MaxItemsPerSection)
            .Select(x => new ClienteHistorialComentarioDto
            {
                Id = x.Id,
                Tipo = "Evento",
                EntidadId = x.EventoId,
                EntidadNombre = x.Evento == null ? null : x.Evento.Nombre,
                Texto = x.Texto,
                FechaCreacionUtc = x.FechaCreacionUtc,
                Activo = x.Activo
            })
            .ToListAsync(cancellationToken);

        var fotos = await dbContext.ComentariosFotos
            .AsNoTracking()
            .Include(x => x.Foto)
            .Where(x => x.UsuarioId == usuarioId)
            .OrderByDescending(x => x.FechaCreacionUtc)
            .Take(MaxItemsPerSection)
            .Select(x => new ClienteHistorialComentarioDto
            {
                Id = x.Id,
                Tipo = "Foto",
                EntidadId = x.FotoId,
                EntidadNombre = x.Foto == null ? null : x.Foto.NombreArchivo,
                Texto = x.Texto,
                FechaCreacionUtc = x.FechaCreacionUtc,
                Activo = x.Activo
            })
            .ToListAsync(cancellationToken);

        return eventos.Concat(fotos)
            .OrderByDescending(x => x.FechaCreacionUtc)
            .Take(MaxItemsPerSection)
            .ToList();
    }

    private static ClienteHistorialPedidoDto MapPedido(Pedido pedido)
    {
        return new ClienteHistorialPedidoDto
        {
            Id = pedido.Id,
            EventoId = pedido.EventoId,
            EventoNombre = pedido.Evento?.Nombre,
            Estado = pedido.Estado,
            Total = pedido.Total,
            Moneda = pedido.Moneda,
            CreadoEnUtc = pedido.CreadoEnUtc,
            ActualizadoEnUtc = pedido.ActualizadoEnUtc,
            Items = pedido.PedidoItems.Select(item => new ClienteHistorialPedidoItemDto
            {
                Id = item.Id,
                TipoItem = item.TipoItem,
                Descripcion = item.Descripcion,
                PrecioUnitario = item.PrecioUnitario,
                Cantidad = item.Cantidad,
                Subtotal = item.Subtotal
            }).ToList()
        };
    }

    private static ClienteHistorialTotalesDto BuildTotales(
        IReadOnlyCollection<ClienteHistorialEventoDto> eventos,
        IReadOnlyCollection<ClienteHistorialSesionPrivadaDto> sesiones,
        IReadOnlyCollection<Pedido> pedidos,
        IReadOnlyCollection<ClienteHistorialPagoDto> pagos,
        IReadOnlyCollection<ClienteHistorialDescargaDto> descargas,
        IReadOnlyCollection<ClienteHistorialSolicitudPresupuestoDto> solicitudes,
        IReadOnlyCollection<ClienteHistorialAgendaItemDto> agenda)
    {
        var ultimaCompra = pagos
            .Where(x => PedidoEstados.EsEstadoPagado(x.Estado))
            .Select(x => x.PagadoEnUtc ?? x.CreadoEnUtc)
            .DefaultIfEmpty()
            .Max();

        var actividad = new List<DateTime>();
        actividad.AddRange(eventos.Select(x => x.FechaEventoUtc));
        actividad.AddRange(sesiones.Select(x => x.FechaSesionUtc));
        actividad.AddRange(pedidos.Select(x => x.ActualizadoEnUtc ?? x.CreadoEnUtc));
        actividad.AddRange(pagos.Select(x => x.PagadoEnUtc ?? x.CreadoEnUtc));
        actividad.AddRange(descargas.Select(x => x.UltimaDescargaUtc ?? x.ExpiraEnUtc));
        actividad.AddRange(solicitudes.Select(x => x.FechaCreacionUtc));
        actividad.AddRange(agenda.Select(x => x.FechaInicioUtc));

        return new ClienteHistorialTotalesDto
        {
            TotalPedidos = pedidos.Count,
            TotalGastado = pedidos.Where(x => PedidoEstados.EsPagado(x.Estado)).Sum(x => x.Total),
            TotalDescargas = descargas.Count,
            TotalSesionesPrivadas = sesiones.Count,
            TotalEventos = eventos.Count,
            TotalSolicitudes = solicitudes.Count,
            UltimaCompraUtc = ultimaCompra == default ? null : ultimaCompra,
            UltimaActividadUtc = actividad.Count == 0 ? null : actividad.Max()
        };
    }

    private bool CanAccess(Cliente cliente)
    {
        return currentUser.IsAdmin || cliente.UsuarioId == currentUser.UserId;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
