using AutoMapper;
using Fotografia.Application.DTOs.Fotos;
using Fotografia.Application.DTOs.SesionesPrivadas;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Services;

public sealed class SesionPrivadaService(
    AppDbContext dbContext,
    IMapper mapper,
    ICurrentUserService currentUser) : ISesionPrivadaService
{
    public async Task<ApiResponse<IReadOnlyCollection<SesionPrivadaResponseDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return ApiResponse<IReadOnlyCollection<SesionPrivadaResponseDto>>.Forbidden("Debe iniciar sesion.");
        }

        var query = QuerySesiones().AsNoTracking();

        if (!currentUser.IsAdmin)
        {
            query = query.Where(x => x.Activa && x.Cliente != null && x.Cliente.UsuarioId == currentUser.UserId.Value);
        }

        var sesiones = await query
            .OrderByDescending(x => x.FechaSesionUtc)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<SesionPrivadaResponseDto>>.Ok(
            mapper.Map<List<SesionPrivadaResponseDto>>(sesiones));
    }

    public async Task<ApiResponse<SesionPrivadaResponseDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var sesion = await QuerySesiones().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (sesion is null)
        {
            return ApiResponse<SesionPrivadaResponseDto>.NotFound("Sesion privada no encontrada.");
        }

        if (!CanAccess(sesion))
        {
            return ApiResponse<SesionPrivadaResponseDto>.Forbidden("No puede consultar sesiones privadas de otro usuario.");
        }

        return ApiResponse<SesionPrivadaResponseDto>.Ok(mapper.Map<SesionPrivadaResponseDto>(sesion));
    }

    public async Task<ApiResponse<SesionPrivadaResponseDto>> CreateAsync(
        CrearSesionPrivadaRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<SesionPrivadaResponseDto>.Forbidden("Solo un administrador puede crear sesiones privadas.");
        }

        var clienteExists = await dbContext.Clientes.AnyAsync(x => x.Id == request.ClienteId, cancellationToken);
        if (!clienteExists)
        {
            return ApiResponse<SesionPrivadaResponseDto>.NotFound("Cliente no encontrado.");
        }

        var sesion = new SesionPrivada
        {
            ClienteId = request.ClienteId,
            Titulo = request.Titulo.Trim(),
            Descripcion = Normalize(request.Descripcion),
            FechaSesionUtc = request.FechaSesionUtc,
            Estado = request.Estado.Trim(),
            PrecioPaquete = request.PrecioPaquete,
            Activa = true,
            FechaCreacionUtc = DateTime.UtcNow
        };

        dbContext.SesionesPrivadas.Add(sesion);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<SesionPrivadaResponseDto>.Ok(
            mapper.Map<SesionPrivadaResponseDto>(sesion),
            "Sesion privada creada.");
    }

    public async Task<ApiResponse<SesionPrivadaResponseDto>> UpdateAsync(
        Guid id,
        ActualizarSesionPrivadaRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<SesionPrivadaResponseDto>.Forbidden("Solo un administrador puede editar sesiones privadas.");
        }

        var sesion = await dbContext.SesionesPrivadas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (sesion is null)
        {
            return ApiResponse<SesionPrivadaResponseDto>.NotFound("Sesion privada no encontrada.");
        }

        sesion.Titulo = request.Titulo.Trim();
        sesion.Descripcion = Normalize(request.Descripcion);
        sesion.FechaSesionUtc = request.FechaSesionUtc;
        sesion.Estado = request.Estado.Trim();
        sesion.PrecioPaquete = request.PrecioPaquete;
        sesion.Activa = request.Activa;
        sesion.FechaActualizacionUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<SesionPrivadaResponseDto>.Ok(
            mapper.Map<SesionPrivadaResponseDto>(sesion),
            "Sesion privada actualizada.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<bool>.Forbidden("Solo un administrador puede eliminar sesiones privadas.");
        }

        var sesion = await dbContext.SesionesPrivadas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (sesion is null)
        {
            return ApiResponse<bool>.NotFound("Sesion privada no encontrada.");
        }

        sesion.Activa = false;
        sesion.FechaActualizacionUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Sesion privada desactivada.");
    }

    public async Task<ApiResponse<IReadOnlyCollection<FotoPrivadaResponseDto>>> GetFotosAsync(
        Guid sesionId,
        CancellationToken cancellationToken = default)
    {
        var sesion = await QuerySesiones().AsNoTracking().FirstOrDefaultAsync(x => x.Id == sesionId, cancellationToken);
        if (sesion is null)
        {
            return ApiResponse<IReadOnlyCollection<FotoPrivadaResponseDto>>.NotFound("Sesion privada no encontrada.");
        }

        if (!CanAccess(sesion))
        {
            return ApiResponse<IReadOnlyCollection<FotoPrivadaResponseDto>>.Forbidden("No puede consultar fotos privadas de otro usuario.");
        }

        var fotosQuery = dbContext.FotosPrivadas
            .AsNoTracking()
            .Where(x => x.SesionPrivadaId == sesionId);

        if (!currentUser.IsAdmin)
        {
            fotosQuery = fotosQuery.Where(x => x.Activa);
        }

        var fotos = await fotosQuery
            .OrderByDescending(x => x.FechaCreacionUtc)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<FotoPrivadaResponseDto>>.Ok(
            mapper.Map<List<FotoPrivadaResponseDto>>(fotos));
    }

    public async Task<ApiResponse<StorageKeyResponseDto>> GenerateStorageKeyAsync(
        Guid sesionId,
        GenerarStorageKeyFotoPrivadaRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<StorageKeyResponseDto>.Forbidden("Solo un administrador puede generar storage keys.");
        }

        var exists = await dbContext.SesionesPrivadas.AnyAsync(x => x.Id == sesionId, cancellationToken);
        if (!exists)
        {
            return ApiResponse<StorageKeyResponseDto>.NotFound("Sesion privada no encontrada.");
        }

        return ApiResponse<StorageKeyResponseDto>.Ok(new StorageKeyResponseDto
        {
            StorageKey = CreatePrivateStorageKey(sesionId, request.NombreArchivo)
        });
    }

    public async Task<ApiResponse<FotoPrivadaResponseDto>> CreateFotoMetadataAsync(
        Guid sesionId,
        CrearFotoPrivadaMetadataRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<FotoPrivadaResponseDto>.Forbidden("Solo un administrador puede crear fotos privadas.");
        }

        var sesion = await dbContext.SesionesPrivadas.FirstOrDefaultAsync(x => x.Id == sesionId, cancellationToken);
        if (sesion is null)
        {
            return ApiResponse<FotoPrivadaResponseDto>.NotFound("Sesion privada no encontrada.");
        }

        if (!FileHelper.IsSupportedImageContentType(request.ContentType))
        {
            return ApiResponse<FotoPrivadaResponseDto>.Fail("Formato de imagen no soportado. Use JPEG, PNG o WebP.");
        }

        var duplicated = await dbContext.FotosPrivadas
            .AnyAsync(x => x.SesionPrivadaId == sesionId && x.StorageKey == request.StorageKey, cancellationToken);
        if (duplicated)
        {
            return ApiResponse<FotoPrivadaResponseDto>.Fail("Ya existe una foto privada con ese StorageKey en la sesion.");
        }

        var foto = new FotoPrivada
        {
            SesionPrivadaId = sesion.Id,
            ClienteId = sesion.ClienteId,
            NombreArchivo = request.NombreArchivo.Trim(),
            ContentType = request.ContentType.Trim(),
            StorageKey = request.StorageKey.Trim(),
            PreviewUrl = Normalize(request.PreviewUrl),
            MarcaAguaStorageKey = Normalize(request.MarcaAguaStorageKey),
            SizeInBytes = request.SizeInBytes,
            Width = request.Width,
            Height = request.Height,
            PrecioUnitario = request.PrecioUnitario,
            Activa = true,
            FechaCreacionUtc = DateTime.UtcNow
        };

        dbContext.FotosPrivadas.Add(foto);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<FotoPrivadaResponseDto>.Ok(
            mapper.Map<FotoPrivadaResponseDto>(foto),
            "Metadata de foto privada creada.");
    }

    public async Task<ApiResponse<FotoPrivadaResponseDto>> UpdateFotoAsync(
        Guid fotoPrivadaId,
        ActualizarFotoPrivadaRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<FotoPrivadaResponseDto>.Forbidden("Solo un administrador puede editar fotos privadas.");
        }

        var foto = await dbContext.FotosPrivadas.FirstOrDefaultAsync(x => x.Id == fotoPrivadaId, cancellationToken);
        if (foto is null)
        {
            return ApiResponse<FotoPrivadaResponseDto>.NotFound("Foto privada no encontrada.");
        }

        foto.NombreArchivo = request.NombreArchivo.Trim();
        foto.PreviewUrl = Normalize(request.PreviewUrl);
        foto.MarcaAguaStorageKey = Normalize(request.MarcaAguaStorageKey);
        foto.PrecioUnitario = request.PrecioUnitario;
        foto.Activa = request.Activa;
        foto.FechaActualizacionUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<FotoPrivadaResponseDto>.Ok(
            mapper.Map<FotoPrivadaResponseDto>(foto),
            "Foto privada actualizada.");
    }

    public async Task<ApiResponse<bool>> DeleteFotoAsync(Guid fotoPrivadaId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ApiResponse<bool>.Forbidden("Solo un administrador puede eliminar fotos privadas.");
        }

        var foto = await dbContext.FotosPrivadas.FirstOrDefaultAsync(x => x.Id == fotoPrivadaId, cancellationToken);
        if (foto is null)
        {
            return ApiResponse<bool>.NotFound("Foto privada no encontrada.");
        }

        foto.Activa = false;
        foto.FechaActualizacionUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Foto privada desactivada.");
    }

    private IQueryable<SesionPrivada> QuerySesiones()
    {
        return dbContext.SesionesPrivadas
            .Include(x => x.Cliente)
            .Include(x => x.Fotos);
    }

    private bool CanAccess(SesionPrivada sesion)
    {
        return currentUser.IsAdmin
            || (sesion.Activa && sesion.Cliente?.UsuarioId is not null && sesion.Cliente.UsuarioId == currentUser.UserId);
    }

    private static string CreatePrivateStorageKey(Guid sesionId, string fileName)
    {
        var extension = Path.GetExtension(fileName);
        var name = Path.GetFileNameWithoutExtension(fileName);
        var safeName = FileHelper.CreateSlug(string.IsNullOrWhiteSpace(name) ? "foto-privada" : name);
        var safeExtension = string.IsNullOrWhiteSpace(extension) ? ".jpg" : extension.ToLowerInvariant();

        return $"privadas/{sesionId:N}/fotos/{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}-{safeName}{safeExtension}";
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
