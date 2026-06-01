using AutoMapper;
using Fotografia.Infrastructure.Data;
using Fotografia.Application.DTOs.Common;
using Fotografia.Application.DTOs.Fotos;
using Fotografia.Application.DTOs.Pexels;
using Fotografia.Domain.Entities;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fotografia.Infrastructure.Services;

public sealed class FotoService(
    AppDbContext dbContext,
    IMapper mapper,
    IStorageService storageService,
    IPexelsService pexelsService,
    ILogger<FotoService> logger) : IFotoService
{
    public async Task<ApiResponse<IReadOnlyCollection<FotoResponseDto>>> GetByEventoAsync(Guid eventoId, CancellationToken cancellationToken = default)
    {
        var fotos = await dbContext.Fotos
            .AsNoTracking()
            .Where(x => x.EventoId == eventoId && x.Activa)
            .OrderByDescending(x => x.SubidaEnUtc)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<FotoResponseDto>>.Ok(mapper.Map<List<FotoResponseDto>>(fotos));
    }

    public async Task<ApiResponse<PaginatedResponseDto<FotoResponseDto>>> GetByEventoPaginatedAsync(
        Guid eventoId,
        PaginationQueryDto pagination,
        CancellationToken cancellationToken = default)
    {
        var validationError = pagination.Validate();
        if (validationError is not null)
        {
            return ApiResponse<PaginatedResponseDto<FotoResponseDto>>.Fail(validationError);
        }

        var query = dbContext.Fotos
            .AsNoTracking()
            .Where(x => x.EventoId == eventoId && x.Activa)
            .OrderByDescending(x => x.SubidaEnUtc);

        var paginated = await query.ToPaginatedResponseAsync(pagination, cancellationToken);

        return ApiResponse<PaginatedResponseDto<FotoResponseDto>>.Ok(new PaginatedResponseDto<FotoResponseDto>
        {
            Items = mapper.Map<List<FotoResponseDto>>(paginated.Items),
            Page = paginated.Page,
            PageSize = paginated.PageSize,
            TotalItems = paginated.TotalItems,
            TotalPages = paginated.TotalPages,
            HasPreviousPage = paginated.HasPreviousPage,
            HasNextPage = paginated.HasNextPage,
            All = paginated.All
        });
    }

    public async Task<ApiResponse<FotoResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var foto = await dbContext.Fotos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.Activa, cancellationToken);

        return foto is null
            ? ApiResponse<FotoResponseDto>.NotFound("Foto no encontrada.")
            : ApiResponse<FotoResponseDto>.Ok(mapper.Map<FotoResponseDto>(foto));
    }

    public async Task<ApiResponse<FotoResponseDto>> CreateMetadataAsync(CrearFotoMetadataRequestDto request, CancellationToken cancellationToken = default)
    {
        var eventoExists = await dbContext.Eventos.AnyAsync(x => x.Id == request.EventoId, cancellationToken);
        if (!eventoExists)
        {
            return ApiResponse<FotoResponseDto>.NotFound("Evento no encontrado.");
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

    public async Task<ApiResponse<ImportarFotosPexelsResponseDto>> ImportarDesdePexelsAsync(
        ImportarFotosPexelsRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.EventoId == Guid.Empty)
        {
            return ApiResponse<ImportarFotosPexelsResponseDto>.Fail("EventoId es requerido.");
        }

        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return ApiResponse<ImportarFotosPexelsResponseDto>.Fail("Query es requerido.");
        }

        if (request.Cantidad is < 1 or > 200)
        {
            return ApiResponse<ImportarFotosPexelsResponseDto>.Fail("Cantidad debe estar entre 1 y 200.");
        }

        if (request.PrecioUnitario < 0)
        {
            return ApiResponse<ImportarFotosPexelsResponseDto>.Fail("PrecioUnitario debe ser mayor o igual a 0.");
        }

        var eventoExists = await dbContext.Eventos.AnyAsync(x => x.Id == request.EventoId, cancellationToken);
        if (!eventoExists)
        {
            return ApiResponse<ImportarFotosPexelsResponseDto>.NotFound("Evento no encontrado.");
        }

        IReadOnlyList<PexelsPhotoDto> pexelsPhotos;
        try
        {
            pexelsPhotos = await pexelsService.BuscarFotosAsync(request.Query.Trim(), request.Cantidad, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return ApiResponse<ImportarFotosPexelsResponseDto>.Fail(ex.Message);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Pexels photo import failed for event {EventoId}.", request.EventoId);
            return ApiResponse<ImportarFotosPexelsResponseDto>.Fail("No se pudieron obtener fotos desde Pexels.");
        }

        if (pexelsPhotos.Count == 0)
        {
            return ApiResponse<ImportarFotosPexelsResponseDto>.Ok(new ImportarFotosPexelsResponseDto
            {
                EventoId = request.EventoId,
                Query = request.Query.Trim(),
                CantidadSolicitada = request.Cantidad
            }, "Pexels no devolvio fotos para la busqueda.");
        }

        var storageKeys = pexelsPhotos.Select(photo => CreatePexelsStorageKey(request.EventoId, photo.Id)).ToList();
        var existingStorageKeys = await dbContext.Fotos
            .Where(x => x.EventoId == request.EventoId && storageKeys.Contains(x.StorageKey))
            .Select(x => x.StorageKey)
            .ToListAsync(cancellationToken);

        var existing = existingStorageKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var imported = new List<FotoImportadaPexelsDto>();
        var duplicated = 0;
        var now = DateTime.UtcNow;

        foreach (var pexelsPhoto in pexelsPhotos)
        {
            var storageKey = CreatePexelsStorageKey(request.EventoId, pexelsPhoto.Id);
            if (existing.Contains(storageKey))
            {
                duplicated++;
                continue;
            }

            var nombreArchivo = $"pexels-{pexelsPhoto.Id}.jpg";
            var foto = new Foto
            {
                EventoId = request.EventoId,
                NombreArchivo = nombreArchivo,
                ContentType = "image/jpeg",
                StorageKey = storageKey,
                PreviewUrl = pexelsPhoto.PreviewUrl,
                MarcaAguaStorageKey = null,
                SizeInBytes = 0,
                Width = pexelsPhoto.Width,
                Height = pexelsPhoto.Height,
                PrecioUnitario = request.PrecioUnitario,
                Activa = true,
                SubidaEnUtc = now
            };

            dbContext.Fotos.Add(foto);
            existing.Add(storageKey);

            imported.Add(new FotoImportadaPexelsDto
            {
                FotoId = foto.Id,
                NombreArchivo = foto.NombreArchivo,
                PreviewUrl = foto.PreviewUrl,
                StorageKey = foto.StorageKey,
                PrecioUnitario = foto.PrecioUnitario
            });
        }

        if (imported.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return ApiResponse<ImportarFotosPexelsResponseDto>.Ok(new ImportarFotosPexelsResponseDto
        {
            EventoId = request.EventoId,
            Query = request.Query.Trim(),
            CantidadSolicitada = request.Cantidad,
            CantidadImportada = imported.Count,
            CantidadDuplicada = duplicated,
            Fotos = imported
        }, "Importacion desde Pexels finalizada.");
    }

    public async Task<ApiResponse<FotoResponseDto>> UpdateAsync(Guid id, ActualizarFotoRequestDto request, CancellationToken cancellationToken = default)
    {
        var foto = await dbContext.Fotos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (foto is null)
        {
            return ApiResponse<FotoResponseDto>.NotFound("Foto no encontrada.");
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
            return ApiResponse<bool>.NotFound("Foto no encontrada.");
        }

        foto.Activa = false;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Foto desactivada.");
    }

    private static string CreatePexelsStorageKey(Guid eventoId, long pexelsId)
    {
        return $"demo/pexels/{eventoId}/pexels-{pexelsId}.jpg";
    }
}
