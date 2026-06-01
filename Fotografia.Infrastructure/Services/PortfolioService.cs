using AutoMapper;
using Fotografia.Application.DTOs.Portfolio;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Services;

public sealed class PortfolioService(
    AppDbContext dbContext,
    IMapper mapper,
    ICurrentUserService currentUser) : IPortfolioService
{
    public async Task<ApiResponse<IReadOnlyCollection<PortfolioItemResponseDto>>> GetPublicAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await dbContext.PortfolioItems
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Orden)
            .ThenByDescending(x => x.Destacado)
            .ThenBy(x => x.Titulo)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<PortfolioItemResponseDto>>.Ok(
            mapper.Map<List<PortfolioItemResponseDto>>(items));
    }

    public async Task<ApiResponse<PortfolioItemResponseDto>> GetPublicByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await dbContext.PortfolioItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.Activo, cancellationToken);

        return item is null
            ? ApiResponse<PortfolioItemResponseDto>.NotFound("Item de portfolio no encontrado.")
            : ApiResponse<PortfolioItemResponseDto>.Ok(mapper.Map<PortfolioItemResponseDto>(item));
    }

    public async Task<ApiResponse<IReadOnlyCollection<PortfolioItemResponseDto>>> GetAdminAsync(
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<IReadOnlyCollection<PortfolioItemResponseDto>>.Forbidden("Solo un administrador puede consultar el portfolio completo.");
        }

        var items = await dbContext.PortfolioItems
            .AsNoTracking()
            .OrderBy(x => x.Orden)
            .ThenBy(x => x.Titulo)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<PortfolioItemResponseDto>>.Ok(
            mapper.Map<List<PortfolioItemResponseDto>>(items));
    }

    public async Task<ApiResponse<PortfolioItemResponseDto>> CreateAsync(
        CrearPortfolioItemRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<PortfolioItemResponseDto>.Forbidden("Solo un administrador puede crear items de portfolio.");
        }

        var item = new PortfolioItem
        {
            Titulo = request.Titulo.Trim(),
            Descripcion = Normalize(request.Descripcion),
            ImagenUrl = Normalize(request.ImagenUrl),
            Categoria = Normalize(request.Categoria),
            Orden = request.Orden,
            Destacado = request.Destacado,
            Activo = request.Activo,
            FechaCreacionUtc = DateTime.UtcNow
        };

        dbContext.PortfolioItems.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<PortfolioItemResponseDto>.Ok(
            mapper.Map<PortfolioItemResponseDto>(item),
            "Item de portfolio creado.");
    }

    public async Task<ApiResponse<PortfolioItemResponseDto>> UpdateAsync(
        Guid id,
        ActualizarPortfolioItemRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<PortfolioItemResponseDto>.Forbidden("Solo un administrador puede editar items de portfolio.");
        }

        var item = await dbContext.PortfolioItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null)
        {
            return ApiResponse<PortfolioItemResponseDto>.NotFound("Item de portfolio no encontrado.");
        }

        item.Titulo = request.Titulo.Trim();
        item.Descripcion = Normalize(request.Descripcion);
        item.ImagenUrl = Normalize(request.ImagenUrl);
        item.Categoria = Normalize(request.Categoria);
        item.Orden = request.Orden;
        item.Destacado = request.Destacado;
        item.Activo = request.Activo;
        item.FechaActualizacionUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<PortfolioItemResponseDto>.Ok(
            mapper.Map<PortfolioItemResponseDto>(item),
            "Item de portfolio actualizado.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<bool>.Forbidden("Solo un administrador puede eliminar items de portfolio.");
        }

        var item = await dbContext.PortfolioItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null)
        {
            return ApiResponse<bool>.NotFound("Item de portfolio no encontrado.");
        }

        item.Activo = false;
        item.FechaActualizacionUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Item de portfolio desactivado.");
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
