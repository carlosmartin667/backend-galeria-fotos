using AutoMapper;
using Fotografia.Application.DTOs.Eventos;
using Fotografia.Application.DTOs.Faq;
using Fotografia.Application.DTOs.Portfolio;
using Fotografia.Application.DTOs.Servicios;
using Fotografia.Application.DTOs.Sitio;
using Fotografia.Application.Helpers;
using Fotografia.Application.Services.Interfaces;
using Fotografia.Domain.Constants;
using Fotografia.Domain.Entities;
using Fotografia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fotografia.Infrastructure.Services;

public sealed class SitioPublicoService(AppDbContext dbContext, IMapper mapper) : ISitioPublicoService
{
    public async Task<ApiResponse<SitioHomeResponseDto>> GetHomeAsync(
        CancellationToken cancellationToken = default)
    {
        var perfil = await QueryPerfilActivo().FirstOrDefaultAsync(cancellationToken);
        var servicios = await dbContext.ServiciosFotografia
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Orden)
            .ThenBy(x => x.Nombre)
            .Take(6)
            .ToListAsync(cancellationToken);

        var portfolio = await dbContext.PortfolioItems
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderByDescending(x => x.Destacado)
            .ThenBy(x => x.Orden)
            .ThenBy(x => x.Titulo)
            .Take(6)
            .ToListAsync(cancellationToken);

        var preguntas = await dbContext.PreguntasFrecuentes
            .AsNoTracking()
            .Where(x => x.Activa)
            .OrderBy(x => x.Orden)
            .ThenBy(x => x.Pregunta)
            .Take(5)
            .ToListAsync(cancellationToken);

        var eventos = await dbContext.Eventos
            .AsNoTracking()
            .Include(x => x.Fotos)
            .Include(x => x.PortadaFoto)
            .Where(x =>
                x.Activo
                && x.Visibilidad == EventoVisibilidades.Publico
                && (x.Estado == EventoEstados.Publicado || x.Estado == EventoEstados.LegacyActivo))
            .OrderByDescending(x => x.FechaEventoUtc)
            .Take(4)
            .ToListAsync(cancellationToken);

        return ApiResponse<SitioHomeResponseDto>.Ok(new SitioHomeResponseDto
        {
            PerfilFotografa = MapPerfil(perfil),
            Servicios = mapper.Map<List<ServicioFotografiaResponseDto>>(servicios),
            Portfolio = mapper.Map<List<PortfolioItemResponseDto>>(portfolio),
            PreguntasFrecuentes = mapper.Map<List<PreguntaFrecuenteResponseDto>>(preguntas),
            EventosPublicosRecientes = mapper.Map<List<EventoResponseDto>>(eventos)
        });
    }

    public async Task<ApiResponse<SitioContactoResponseDto>> GetContactoAsync(
        CancellationToken cancellationToken = default)
    {
        var perfil = await QueryPerfilActivo().FirstOrDefaultAsync(cancellationToken);
        var perfilDto = MapPerfil(perfil);

        return ApiResponse<SitioContactoResponseDto>.Ok(new SitioContactoResponseDto
        {
            PerfilFotografa = perfilDto,
            WhatsAppUrl = perfilDto?.WhatsAppUrl,
            CorreoPublico = perfilDto?.CorreoPublico,
            Instagram = perfilDto?.Instagram,
            Facebook = perfilDto?.Facebook,
            TikTok = perfilDto?.TikTok,
            SitioWeb = perfilDto?.SitioWeb,
            Direccion = perfilDto?.Direccion,
            Ciudad = perfilDto?.Ciudad,
            Provincia = perfilDto?.Provincia,
            Pais = perfilDto?.Pais,
            TextoBienvenida = perfilDto?.TextoBienvenida
        });
    }

    private IQueryable<PerfilFotografa> QueryPerfilActivo()
    {
        return dbContext.PerfilesFotografa
            .AsNoTracking()
            .Where(x => x.Activa)
            .OrderBy(x => x.FechaCreacionUtc);
    }

    private PerfilFotografaResponseDto? MapPerfil(PerfilFotografa? perfil)
    {
        if (perfil is null)
        {
            return null;
        }

        var dto = mapper.Map<PerfilFotografaResponseDto>(perfil);
        dto.WhatsAppUrl = WhatsAppHelper.CreateWhatsAppUrl(dto.WhatsApp);
        return dto;
    }
}
