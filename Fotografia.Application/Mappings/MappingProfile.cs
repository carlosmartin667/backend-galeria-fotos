using AutoMapper;
using Fotografia.Application.DTOs.Admin;
using Fotografia.Application.DTOs.Auth;
using Fotografia.Application.DTOs.Clientes;
using Fotografia.Application.DTOs.Comentarios;
using Fotografia.Application.DTOs.Eventos;
using Fotografia.Application.DTOs.Favoritos;
using Fotografia.Application.DTOs.Fotos;
using Fotografia.Application.DTOs.Pagos;
using Fotografia.Application.DTOs.Pedidos;
using Fotografia.Domain.Entities;

namespace Fotografia.Application.Mappings;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Usuario, UsuarioResponseDto>();
        CreateMap<Usuario, AdminPerfilPublicoResponseDto>();
        CreateMap<Usuario, AdminPerfilResponseDto>();

        CreateMap<CrearEventoRequestDto, Evento>();
        CreateMap<Evento, EventoResponseDto>()
            .ForMember(dest => dest.CantidadFotos, opt => opt.MapFrom(src => src.Fotos.Count));

        CreateMap<CrearClienteRequestDto, Cliente>();
        CreateMap<Cliente, ClienteResponseDto>();

        CreateMap<CrearFotoMetadataRequestDto, Foto>();
        CreateMap<Foto, FotoResponseDto>();

        CreateMap<PedidoFoto, PedidoFotoResponseDto>()
            .ForMember(dest => dest.Foto, opt => opt.MapFrom(src => src.Foto));
        CreateMap<Pedido, PedidoResponseDto>()
            .ForMember(dest => dest.Fotos, opt => opt.MapFrom(src => src.PedidoFotos));

        CreateMap<Pago, PagoResponseDto>();

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
