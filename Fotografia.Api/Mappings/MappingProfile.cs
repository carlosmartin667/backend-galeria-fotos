using AutoMapper;
using Fotografia.Api.DTOs.Auth;
using Fotografia.Api.DTOs.Clientes;
using Fotografia.Api.DTOs.Eventos;
using Fotografia.Api.DTOs.Fotos;
using Fotografia.Api.DTOs.Pagos;
using Fotografia.Api.DTOs.Pedidos;
using Fotografia.Api.Entities;

namespace Fotografia.Api.Mappings;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Usuario, UsuarioResponseDto>();

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
    }
}
