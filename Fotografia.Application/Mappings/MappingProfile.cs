using AutoMapper;
using Fotografia.Application.DTOs.Admin;
using Fotografia.Application.DTOs.Auth;
using Fotografia.Application.DTOs.Carrito;
using Fotografia.Application.DTOs.Clientes;
using Fotografia.Application.DTOs.Comentarios;
using Fotografia.Application.DTOs.Eventos;
using Fotografia.Application.DTOs.Favoritos;
using Fotografia.Application.DTOs.Faq;
using Fotografia.Application.DTOs.Fotos;
using Fotografia.Application.DTOs.Paquetes;
using Fotografia.Application.DTOs.Pagos;
using Fotografia.Application.DTOs.Pedidos;
using Fotografia.Application.DTOs.Portfolio;
using Fotografia.Application.DTOs.Servicios;
using Fotografia.Application.DTOs.SesionesPrivadas;
using Fotografia.Application.DTOs.Sitio;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;

namespace Fotografia.Application.Mappings;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Usuario, UsuarioResponseDto>();
        CreateMap<Usuario, AdminPerfilPublicoResponseDto>();
        CreateMap<Usuario, AdminPerfilResponseDto>();

        CreateMap<CrearEventoRequestDto, Evento>()
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.Estado) ? EventoEstados.Publicado : src.Estado.Trim()))
            .ForMember(dest => dest.Visibilidad, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.Visibilidad) ? EventoVisibilidades.Publico : src.Visibilidad.Trim()));
        CreateMap<Evento, EventoResponseDto>()
            .ForMember(dest => dest.CantidadFotos, opt => opt.MapFrom(src => src.Fotos.Count(foto => foto.Activa)))
            .ForMember(dest => dest.PortadaFotoId, opt => opt.MapFrom(src =>
                src.PortadaFotoId ?? src.Fotos
                    .Where(foto => foto.Activa)
                    .OrderBy(foto => foto.SubidaEnUtc)
                    .Select(foto => (Guid?)foto.Id)
                    .FirstOrDefault()))
            .ForMember(dest => dest.PortadaPreviewUrl, opt => opt.MapFrom(src =>
                src.PortadaFoto != null && src.PortadaFoto.Activa
                    ? src.PortadaFoto.PreviewUrl
                    : src.Fotos
                        .Where(foto => foto.Activa)
                        .OrderBy(foto => foto.SubidaEnUtc)
                        .Select(foto => foto.PreviewUrl)
                        .FirstOrDefault()))
            .ForMember(dest => dest.PortadaNombreArchivo, opt => opt.MapFrom(src =>
                src.PortadaFoto != null && src.PortadaFoto.Activa
                    ? src.PortadaFoto.NombreArchivo
                    : src.Fotos
                        .Where(foto => foto.Activa)
                        .OrderBy(foto => foto.SubidaEnUtc)
                        .Select(foto => foto.NombreArchivo)
                        .FirstOrDefault()));
        CreateMap<PaqueteEvento, PaqueteEventoResponseDto>();
        CreateMap<CrearPaqueteEventoRequestDto, PaqueteEvento>();

        CreateMap<CrearClienteRequestDto, Cliente>();
        CreateMap<Cliente, ClienteResponseDto>();

        CreateMap<CrearFotoMetadataRequestDto, Foto>();
        CreateMap<Foto, FotoResponseDto>();
        CreateMap<FotoPrivada, FotoPrivadaResponseDto>();
        CreateMap<SesionPrivada, SesionPrivadaResponseDto>()
            .ForMember(dest => dest.CantidadFotos, opt => opt.MapFrom(src => src.Fotos.Count));

        CreateMap<PedidoFoto, PedidoFotoResponseDto>()
            .ForMember(dest => dest.Foto, opt => opt.MapFrom(src => src.Foto));
        CreateMap<PedidoItem, PedidoItemResponseDto>();
        CreateMap<Pedido, PedidoResponseDto>()
            .ForMember(dest => dest.Fotos, opt => opt.MapFrom(src => src.PedidoFotos))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.PedidoItems));
        CreateMap<CarritoItem, CarritoItemResponseDto>()
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.PrecioUnitario * src.Cantidad));
        CreateMap<CarritoCompra, CarritoResponseDto>()
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Items.Sum(item => item.PrecioUnitario * item.Cantidad)));

        CreateMap<Pago, PagoResponseDto>();
        CreateMap<PerfilFotografa, PerfilFotografaResponseDto>();
        CreateMap<PortfolioItem, PortfolioItemResponseDto>();
        CreateMap<CrearPortfolioItemRequestDto, PortfolioItem>();
        CreateMap<ServicioFotografia, ServicioFotografiaResponseDto>();
        CreateMap<CrearServicioFotografiaRequestDto, ServicioFotografia>();
        CreateMap<PreguntaFrecuente, PreguntaFrecuenteResponseDto>();
        CreateMap<CrearPreguntaFrecuenteRequestDto, PreguntaFrecuente>();

        CreateMap<ComentarioEvento, ComentarioResponseDto>()
            .ForMember(dest => dest.NombreUsuario, opt => opt.MapFrom(src => src.Usuario == null ? string.Empty : src.Usuario.Nombre));
        CreateMap<ComentarioFoto, ComentarioResponseDto>()
            .ForMember(dest => dest.NombreUsuario, opt => opt.MapFrom(src => src.Usuario == null ? string.Empty : src.Usuario.Nombre));

        CreateMap<EventoFavorito, FavoritoEventoResponseDto>()
            .ForMember(dest => dest.NombreEvento, opt => opt.MapFrom(src => src.Evento == null ? string.Empty : src.Evento.Nombre))
            .ForMember(dest => dest.FechaEventoUtc, opt => opt.MapFrom(src => src.Evento == null ? default : src.Evento.FechaEventoUtc));
        CreateMap<FotoFavorita, FavoritoFotoResponseDto>()
            .ForMember(dest => dest.EventoId, opt => opt.MapFrom(src => src.Foto == null ? default : src.Foto.EventoId))
            .ForMember(dest => dest.NombreArchivo, opt => opt.MapFrom(src => src.Foto == null ? string.Empty : src.Foto.NombreArchivo))
            .ForMember(dest => dest.PreviewUrl, opt => opt.MapFrom(src => src.Foto == null ? null : src.Foto.PreviewUrl));
    }
}
