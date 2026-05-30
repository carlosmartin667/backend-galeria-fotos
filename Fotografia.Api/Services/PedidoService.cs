using AutoMapper;
using Fotografia.Api.Data;
using Fotografia.Api.DTOs.Pedidos;
using Fotografia.Api.Entities;
using Fotografia.Api.Helpers;
using Fotografia.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Api.Services;

public sealed class PedidoService(AppDbContext dbContext, IMapper mapper) : IPedidoService
{
    public async Task<ApiResponse<IReadOnlyCollection<PedidoResponseDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var pedidos = await QueryPedidos()
            .AsNoTracking()
            .OrderByDescending(x => x.CreadoEnUtc)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<PedidoResponseDto>>.Ok(mapper.Map<List<PedidoResponseDto>>(pedidos));
    }

    public async Task<ApiResponse<PedidoResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var pedido = await QueryPedidos()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return pedido is null
            ? ApiResponse<PedidoResponseDto>.Fail("Pedido no encontrado.")
            : ApiResponse<PedidoResponseDto>.Ok(mapper.Map<PedidoResponseDto>(pedido));
    }

    public async Task<ApiResponse<PedidoResponseDto>> CreateAsync(CrearPedidoRequestDto request, CancellationToken cancellationToken = default)
    {
        var clienteExists = await dbContext.Clientes.AnyAsync(x => x.Id == request.ClienteId, cancellationToken);
        if (!clienteExists)
        {
            return ApiResponse<PedidoResponseDto>.Fail("Cliente no encontrado.");
        }

        var eventoExists = await dbContext.Eventos.AnyAsync(x => x.Id == request.EventoId, cancellationToken);
        if (!eventoExists)
        {
            return ApiResponse<PedidoResponseDto>.Fail("Evento no encontrado.");
        }

        var fotoIds = request.FotoIds.Distinct().ToList();
        var fotos = await dbContext.Fotos
            .Where(x => fotoIds.Contains(x.Id) && x.EventoId == request.EventoId && x.Activa)
            .ToListAsync(cancellationToken);

        if (fotos.Count != fotoIds.Count)
        {
            return ApiResponse<PedidoResponseDto>.Fail("Una o mas fotos no existen, no pertenecen al evento o no estan activas.");
        }

        var pedido = new Pedido
        {
            EventoId = request.EventoId,
            ClienteId = request.ClienteId,
            Total = fotos.Sum(x => x.PrecioUnitario),
            PedidoFotos = fotos.Select(foto => new PedidoFoto
            {
                FotoId = foto.Id,
                PrecioUnitario = foto.PrecioUnitario,
                Cantidad = 1
            }).ToList()
        };

        dbContext.Pedidos.Add(pedido);
        await dbContext.SaveChangesAsync(cancellationToken);

        var created = await QueryPedidos()
            .AsNoTracking()
            .FirstAsync(x => x.Id == pedido.Id, cancellationToken);

        return ApiResponse<PedidoResponseDto>.Ok(mapper.Map<PedidoResponseDto>(created), "Pedido creado.");
    }

    private IQueryable<Pedido> QueryPedidos()
    {
        return dbContext.Pedidos
            .Include(x => x.PedidoFotos)
            .ThenInclude(x => x.Foto);
    }
}
