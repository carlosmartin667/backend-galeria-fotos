using AutoMapper;
using Fotografia.Application.DTOs.Auth;
using Fotografia.Application.DTOs.Clientes;
using Fotografia.Application.DTOs.Eventos;
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
