using AutoMapper;
using Fotografia.Infrastructure.Data;
using Fotografia.Application.DTOs.Clientes;
using Fotografia.Domain.Entities;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Services;

public sealed class ClienteService(AppDbContext dbContext, IMapper mapper, ICurrentUserService currentUser) : IClienteService
{
    public async Task<ApiResponse<IReadOnlyCollection<ClienteResponseDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<IReadOnlyCollection<ClienteResponseDto>>.Forbidden("Solo un administrador puede listar clientes.");
        }

        var clientes = await dbContext.Clientes
            .AsNoTracking()
            .OrderBy(x => x.Nombre)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<ClienteResponseDto>>.Ok(mapper.Map<List<ClienteResponseDto>>(clientes));
    }

    public async Task<ApiResponse<ClienteResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cliente = await dbContext.Clientes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (cliente is null)
        {
            return ApiResponse<ClienteResponseDto>.NotFound("Cliente no encontrado.");
        }

        if (!CanAccess(cliente))
        {
            return ApiResponse<ClienteResponseDto>.Forbidden("No puede consultar datos de otro cliente.");
        }

        return ApiResponse<ClienteResponseDto>.Ok(mapper.Map<ClienteResponseDto>(cliente));
    }

    public async Task<ApiResponse<ClienteResponseDto>> CreateAsync(CrearClienteRequestDto request, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<ClienteResponseDto>.Forbidden("Solo un administrador puede crear clientes.");
        }

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
            return ApiResponse<ClienteResponseDto>.NotFound("Cliente no encontrado.");
        }

        if (!CanAccess(cliente))
        {
            return ApiResponse<ClienteResponseDto>.Forbidden("No puede editar datos de otro cliente.");
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
            return ApiResponse<bool>.NotFound("Cliente no encontrado.");
        }

        if (!CanAccess(cliente))
        {
            return ApiResponse<bool>.Forbidden("No puede eliminar datos de otro cliente.");
        }

        dbContext.Clientes.Remove(cliente);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Cliente eliminado.");
    }

    private bool CanAccess(Cliente cliente)
    {
        return currentUser.IsAdmin || (cliente.UsuarioId is not null && cliente.UsuarioId == currentUser.UserId);
    }
}
