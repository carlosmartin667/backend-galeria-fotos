using AutoMapper;
using Fotografia.Application.DTOs.Common;
using Fotografia.Application.DTOs.Fotos;
using Fotografia.Application.DTOs.Pexels;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Fotografia.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fotografia.Infrastructure.Services;

public sealed class FotoService(
    AppDbContext dbContext,
    IMapper mapper,
    IStorageService storageService,
    IPexelsService pexelsService,
    ICurrentUserService currentUser,
    ILogger<FotoService> logger) : IFotoService
{
    public async Task<ApiResponse<IReadOnlyCollection<FotoResponseDto>>> GetByEventoAsync(Guid eventoId, CancellationToken cancellationToken = default)
    {
        var canAccess = await CanAccessEventoAsync(eventoId, cancellationToken);
        if (!canAccess)
        {
            return ApiResponse<IReadOnlyCollection<FotoResponseDto>>.NotFound("Evento no encontrado.");
        }

        var fotos = await dbContext.Fotos
            .AsNoTracking()
            .Where(x => x.EventoId == eventoId && x.Activa)
            .OrderByDescending(x => x.SubidaEnUtc)
            .ToListAsync(cancellationToken);

        var response = mapper.Map<List<FotoResponseDto>>(fotos);
        SanitizeStorageKeysForNonAdmin(response);

        return ApiResponse<IReadOnlyCollection<FotoResponseDto>>.Ok(response);
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

        var canAccess = await CanAccessEventoAsync(eventoId, cancellationToken);
        if (!canAccess)
        {
            return ApiResponse<PaginatedResponseDto<FotoResponseDto>>.NotFound("Evento no encontrado.");
        }

        var query = dbContext.Fotos
            .AsNoTracking()
            .Where(x => x.EventoId == eventoId && x.Activa)
            .OrderByDescending(x => x.SubidaEnUtc);

        var paginated = await query.ToPaginatedResponseAsync(pagination, cancellationToken);

        var items = mapper.Map<List<FotoResponseDto>>(paginated.Items);
        SanitizeStorageKeysForNonAdmin(items);

        return ApiResponse<PaginatedResponseDto<FotoResponseDto>>.Ok(new PaginatedResponseDto<FotoResponseDto>
        {
            Items = items,
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
        var foto = await dbContext.Fotos
            .AsNoTracking()
            .Include(x => x.Evento)
            .ThenInclude(x => x!.ClientePrincipal)
            .FirstOrDefaultAsync(x => x.Id == id && x.Activa, cancellationToken);

        if (foto is null || !CanAccessEvento(foto.Evento))
        {
            return ApiResponse<FotoResponseDto>.NotFound("Foto no encontrada.");
        }

        var response = mapper.Map<FotoResponseDto>(foto);
        SanitizeStorageKeyForNonAdmin(response);

        return ApiResponse<FotoResponseDto>.Ok(response);
    }

    public async Task<ApiResponse<FotoResponseDto>> CreateMetadataAsync(CrearFotoMetadataRequestDto request, CancellationToken cancellationToken = default)
    {
        var validationError = ValidateMetadataRequest(request);
        if (validationError is not null)
        {
            return ApiResponse<FotoResponseDto>.Fail(validationError);
        }

        var eventoExists = await dbContext.Eventos.AnyAsync(x => x.Id == request.EventoId, cancellationToken);
        if (!eventoExists)
        {
            return ApiResponse<FotoResponseDto>.NotFound("Evento no encontrado.");
        }

        var storageKey = request.StorageKey.Trim();
        var duplicateExists = await dbContext.Fotos.AnyAsync(
            x => x.EventoId == request.EventoId && x.StorageKey == storageKey,
            cancellationToken);

        if (duplicateExists)
        {
            return ApiResponse<FotoResponseDto>.Fail("Ya existe una foto con ese StorageKey para el evento.");
        }

        var foto = mapper.Map<Foto>(request);
        ApplyMetadata(foto, request);
        dbContext.Fotos.Add(foto);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<FotoResponseDto>.Ok(mapper.Map<FotoResponseDto>(foto), "Metadata de foto creada.");
    }

    public async Task<ApiResponse<FotoMetadataBulkResponseDto>> CreateMetadataBulkAsync(
        CrearFotoMetadataBulkRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.Fotos.Count == 0)
        {
            return ApiResponse<FotoMetadataBulkResponseDto>.Fail("Debe enviar al menos una foto.");
        }

        if (request.Fotos.Count > 500)
        {
            return ApiResponse<FotoMetadataBulkResponseDto>.Fail("El maximo permitido por lote es 500 fotos.");
        }

        var response = new FotoMetadataBulkResponseDto
        {
            Solicitadas = request.Fotos.Count
        };

        var eventIds = request.Fotos
            .Where(x => x.EventoId != Guid.Empty)
            .Select(x => x.EventoId)
            .Distinct()
            .ToList();

        var existingEventIds = await dbContext.Eventos
            .Where(x => eventIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var existingEvents = existingEventIds.ToHashSet();
        var existingStorageKeys = await dbContext.Fotos
            .Where(x => eventIds.Contains(x.EventoId))
            .Select(x => new { x.EventoId, x.StorageKey })
            .ToListAsync(cancellationToken);

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in existingStorageKeys)
        {
            seen.Add(CreateCompositeKey(item.EventoId, item.StorageKey));
        }

        var created = new List<Foto>();
        for (var index = 0; index < request.Fotos.Count; index++)
        {
            var item = request.Fotos[index];
            var validationError = ValidateMetadataRequest(item);
            if (validationError is not null)
            {
                response.ErroresDetalle.Add(new FotoBulkErrorDto { Index = index, Mensaje = validationError });
                continue;
            }

            if (!existingEvents.Contains(item.EventoId))
            {
                response.ErroresDetalle.Add(new FotoBulkErrorDto { Index = index, Mensaje = "Evento no encontrado." });
                continue;
            }

            var compositeKey = CreateCompositeKey(item.EventoId, item.StorageKey);
            if (!seen.Add(compositeKey))
            {
                response.OmitidasDetalle.Add(new FotoBulkOmitidaDto
                {
                    Index = index,
                    EventoId = item.EventoId,
                    StorageKey = item.StorageKey.Trim(),
                    Motivo = "Foto duplicada por EventoId + StorageKey."
                });
                continue;
            }

            var foto = mapper.Map<Foto>(item);
            ApplyMetadata(foto, item);
            dbContext.Fotos.Add(foto);
            created.Add(foto);
        }

        if (created.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            response.FotosCreadas = mapper.Map<List<FotoResponseDto>>(created);
        }

        response.Creadas = created.Count;
        response.Omitidas = response.OmitidasDetalle.Count;
        response.Errores = response.ErroresDetalle.Count;

        return ApiResponse<FotoMetadataBulkResponseDto>.Ok(response, "Carga masiva de metadata finalizada.");
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
                TieneMarcaAgua = false,
                Procesada = true,
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
        foto.TieneMarcaAgua = request.TieneMarcaAgua ?? !string.IsNullOrWhiteSpace(request.MarcaAguaStorageKey);
        foto.Procesada = request.Procesada ?? foto.Procesada;
        foto.Activa = request.Activa;
        foto.Destacado = request.Destacado;
        foto.OrdenDestacado = request.OrdenDestacado;
        foto.FechaActualizacionUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<FotoResponseDto>.Ok(mapper.Map<FotoResponseDto>(foto), "Foto actualizada.");
    }

    public Task<ApiResponse<StorageKeyResponseDto>> GenerateStorageKeyAsync(GenerarStorageKeyRequestDto request)
    {
        return Task.FromResult(storageService.GenerateStorageKey(request.EventoId, request.NombreArchivo));
    }

    public async Task<ApiResponse<StorageKeysBulkResponseDto>> GenerateStorageKeysBulkAsync(
        GenerarStorageKeysBulkRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.EventoId == Guid.Empty)
        {
            return ApiResponse<StorageKeysBulkResponseDto>.Fail("EventoId es requerido.");
        }

        if (!await dbContext.Eventos.AnyAsync(x => x.Id == request.EventoId, cancellationToken))
        {
            return ApiResponse<StorageKeysBulkResponseDto>.NotFound("Evento no encontrado.");
        }

        var response = new StorageKeysBulkResponseDto
        {
            EventoId = request.EventoId,
            Solicitadas = request.NombresArchivo.Count
        };

        for (var index = 0; index < request.NombresArchivo.Count; index++)
        {
            var nombreArchivo = request.NombresArchivo[index];
            if (string.IsNullOrWhiteSpace(nombreArchivo))
            {
                response.ErroresDetalle.Add(new FotoBulkErrorDto { Index = index, Mensaje = "NombreArchivo es requerido." });
                continue;
            }

            var result = storageService.GenerateStorageKey(request.EventoId, nombreArchivo);
            if (!result.Success || result.Data is null)
            {
                response.ErroresDetalle.Add(new FotoBulkErrorDto
                {
                    Index = index,
                    Mensaje = result.Message ?? "No se pudo generar el StorageKey."
                });
                continue;
            }

            response.Items.Add(new StorageKeyBulkItemDto
            {
                Index = index,
                NombreArchivo = nombreArchivo.Trim(),
                StorageKey = result.Data.StorageKey
            });
        }

        response.Generadas = response.Items.Count;
        response.Errores = response.ErroresDetalle.Count;

        return ApiResponse<StorageKeysBulkResponseDto>.Ok(response, "Storage keys generados.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var foto = await dbContext.Fotos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (foto is null)
        {
            return ApiResponse<bool>.NotFound("Foto no encontrada.");
        }

        foto.Activa = false;
        foto.FechaActualizacionUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Foto desactivada.");
    }

    private async Task<bool> CanAccessEventoAsync(Guid eventoId, CancellationToken cancellationToken)
    {
        var query = dbContext.Eventos.AsNoTracking().Where(x => x.Id == eventoId);

        if (currentUser.IsAdmin)
        {
            return await query.AnyAsync(cancellationToken);
        }

        if (currentUser.IsAuthenticated && currentUser.UserId is Guid userId)
        {
            return await query.AnyAsync(x =>
                x.Activo
                && ((x.Visibilidad == EventoVisibilidades.Publico
                        && (x.Estado == EventoEstados.Publicado || x.Estado == EventoEstados.LegacyActivo))
                    || x.CreadoPorUsuarioId == userId
                    || (x.ClientePrincipal != null && x.ClientePrincipal.UsuarioId == userId)),
                cancellationToken);
        }

        return await query.AnyAsync(x =>
            x.Activo
            && x.Visibilidad == EventoVisibilidades.Publico
            && (x.Estado == EventoEstados.Publicado || x.Estado == EventoEstados.LegacyActivo),
            cancellationToken);
    }

    private bool CanAccessEvento(Evento? evento)
    {
        if (evento is null)
        {
            return false;
        }

        if (currentUser.IsAdmin)
        {
            return true;
        }

        var isPublic = evento.Activo
            && evento.Visibilidad == EventoVisibilidades.Publico
            && (evento.Estado == EventoEstados.Publicado || evento.Estado == EventoEstados.LegacyActivo);

        if (isPublic)
        {
            return true;
        }

        return currentUser.IsAuthenticated
            && currentUser.UserId is Guid userId
            && (evento.CreadoPorUsuarioId == userId || evento.ClientePrincipal?.UsuarioId == userId);
    }

    private static string? ValidateMetadataRequest(CrearFotoMetadataRequestDto request)
    {
        if (request.EventoId == Guid.Empty)
        {
            return "EventoId es requerido.";
        }

        if (string.IsNullOrWhiteSpace(request.NombreArchivo))
        {
            return "NombreArchivo es requerido.";
        }

        if (string.IsNullOrWhiteSpace(request.ContentType))
        {
            return "ContentType es requerido.";
        }

        if (!FileHelper.IsSupportedImageContentType(request.ContentType))
        {
            return "Formato de imagen no soportado. Use JPEG, PNG o WebP.";
        }

        if (string.IsNullOrWhiteSpace(request.StorageKey))
        {
            return "StorageKey es requerido.";
        }

        if (request.PrecioUnitario < 0)
        {
            return "PrecioUnitario debe ser mayor o igual a 0.";
        }

        if (request.SizeInBytes < 0)
        {
            return "SizeInBytes debe ser mayor o igual a 0.";
        }

        return null;
    }

    private static void ApplyMetadata(Foto foto, CrearFotoMetadataRequestDto request)
    {
        foto.NombreArchivo = request.NombreArchivo.Trim();
        foto.ContentType = request.ContentType.Trim().ToLowerInvariant();
        foto.StorageKey = request.StorageKey.Trim();
        foto.PreviewUrl = request.PreviewUrl;
        foto.MarcaAguaStorageKey = request.MarcaAguaStorageKey;
        foto.SizeInBytes = request.SizeInBytes;
        foto.Width = request.Width;
        foto.Height = request.Height;
        foto.PrecioUnitario = request.PrecioUnitario;
        foto.TieneMarcaAgua = request.TieneMarcaAgua ?? !string.IsNullOrWhiteSpace(request.MarcaAguaStorageKey);
        foto.Procesada = request.Procesada ?? false;
        foto.Destacado = request.Destacado;
        foto.OrdenDestacado = request.OrdenDestacado;
        foto.Activa = true;
        foto.SubidaEnUtc = DateTime.UtcNow;
    }

    private static string CreateCompositeKey(Guid eventoId, string storageKey)
    {
        return $"{eventoId:N}|{storageKey.Trim()}";
    }

    private static string CreatePexelsStorageKey(Guid eventoId, long pexelsId)
    {
        return $"demo/pexels/{eventoId}/pexels-{pexelsId}.jpg";
    }

    private void SanitizeStorageKeysForNonAdmin(IEnumerable<FotoResponseDto> fotos)
    {
        if (currentUser.IsAdmin)
        {
            return;
        }

        foreach (var foto in fotos)
        {
            foto.StorageKey = string.Empty;
        }
    }

    private void SanitizeStorageKeyForNonAdmin(FotoResponseDto foto)
    {
        if (!currentUser.IsAdmin)
        {
            foto.StorageKey = string.Empty;
        }
    }
}
