using AutoMapper;
using Fotografia.Infrastructure.Data;
using Fotografia.Application.DTOs.Clientes;
using Fotografia.Domain.Entities;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Services;

public sealed class ClienteService(AppDbContext dbContext, IMapper mapper) : IClienteService
{
    public async Task<ApiResponse<IReadOnlyCollection<ClienteResponseDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var clientes = await dbContext.Clientes
            .AsNoTracking()
            .OrderBy(x => x.Nombre)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<ClienteResponseDto>>.Ok(mapper.Map<List<ClienteResponseDto>>(clientes));
    }

    public async Task<ApiResponse<ClienteResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cliente = await dbContext.Clientes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return cliente is null
            ? ApiResponse<ClienteResponseDto>.Fail("Cliente no encontrado.")
            : ApiResponse<ClienteResponseDto>.Ok(mapper.Map<ClienteResponseDto>(cliente));
    }

    public async Task<ApiResponse<ClienteResponseDto>> CreateAsync(CrearClienteRequestDto request, CancellationToken cancellationToken = default)
    {
        var cliente = mapper.Map<Cliente>(request);
        cliente.Email = cliente.Email.Trim().ToLowerInvariant();

        dbContext.Clientes.Add(cliente);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<ClienteResponseDto>.Ok(mapper.Map<ClienteResponseDto>(cliente), "Cliente creado.");
    }

    public async Task<ApiResponse<ClienteResponseDto>> UpdateAsync(Guid id, ActualizarClienteRequestDto request, CancellationToken cancellationToken = default)
    {
        var cliente = await dbContext.Clientes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (cliente is null)
        {
            return ApiResponse<ClienteResponseDto>.Fail("Cliente no encontrado.");
        }

        cliente.Nombre = request.Nombre.Trim();
        cliente.Email = request.Email.Trim().ToLowerInvariant();
        cliente.Telefono = request.Telefono;
        cliente.Documento = request.Documento;
        cliente.ActualizadoEnUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<ClienteResponseDto>.Ok(mapper.Map<ClienteResponseDto>(cliente), "Cliente actualizado.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cliente = await dbContext.Clientes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (cliente is null)
        {
            return ApiResponse<bool>.Fail("Cliente no encontrado.");
        }

        dbContext.Clientes.Remove(cliente);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Cliente eliminado.");
    }
}
