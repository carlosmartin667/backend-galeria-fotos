using AutoMapper;
using Fotografia.Infrastructure.Data;
using Fotografia.Application.DTOs.Fotos;
using Fotografia.Domain.Entities;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Services;

public sealed class FotoService(AppDbContext dbContext, IMapper mapper, IStorageService storageService) : IFotoService
{
    public async Task<ApiResponse<IReadOnlyCollection<FotoResponseDto>>> GetByEventoAsync(Guid eventoId, CancellationToken cancellationToken = default)
    {
        var fotos = await dbContext.Fotos
            .AsNoTracking()
            .Where(x => x.EventoId == eventoId)
            .OrderByDescending(x => x.SubidaEnUtc)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<FotoResponseDto>>.Ok(mapper.Map<List<FotoResponseDto>>(fotos));
    }

    public async Task<ApiResponse<FotoResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var foto = await dbContext.Fotos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return foto is null
            ? ApiResponse<FotoResponseDto>.Fail("Foto no encontrada.")
            : ApiResponse<FotoResponseDto>.Ok(mapper.Map<FotoResponseDto>(foto));
    }

    public async Task<ApiResponse<FotoResponseDto>> CreateMetadataAsync(CrearFotoMetadataRequestDto request, CancellationToken cancellationToken = default)
    {
        var eventoExists = await dbContext.Eventos.AnyAsync(x => x.Id == request.EventoId, cancellationToken);
        if (!eventoExists)
        {
            return ApiResponse<FotoResponseDto>.Fail("Evento no encontrado.");
        }

        if (!FileHelper.IsSupportedImageContentType(request.ContentType))
        {
            return ApiResponse<FotoResponseDto>.Fail("Formato de imagen no soportado. Use JPEG, PNG o WebP.");
        }

        var foto = mapper.Map<Foto>(request);
        dbContext.Fotos.Add(foto);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<FotoResponseDto>.Ok(mapper.Map<FotoResponseDto>(foto), "Metadata de foto creada.");
    }

    public async Task<ApiResponse<FotoResponseDto>> UpdateAsync(Guid id, ActualizarFotoRequestDto request, CancellationToken cancellationToken = default)
    {
        var foto = await dbContext.Fotos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (foto is null)
        {
            return ApiResponse<FotoResponseDto>.Fail("Foto no encontrada.");
        }

        foto.NombreArchivo = request.NombreArchivo.Trim();
        foto.PreviewUrl = request.PreviewUrl;
        foto.MarcaAguaStorageKey = request.MarcaAguaStorageKey;
        foto.PrecioUnitario = request.PrecioUnitario;
        foto.Activa = request.Activa;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<FotoResponseDto>.Ok(mapper.Map<FotoResponseDto>(foto), "Foto actualizada.");
    }

    public Task<ApiResponse<StorageKeyResponseDto>> GenerateStorageKeyAsync(GenerarStorageKeyRequestDto request)
    {
        return Task.FromResult(storageService.GenerateStorageKey(request.EventoId, request.NombreArchivo));
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var foto = await dbContext.Fotos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (foto is null)
        {
            return ApiResponse<bool>.Fail("Foto no encontrada.");
        }

        dbContext.Fotos.Remove(foto);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Metadata de foto eliminada.");
    }
}
