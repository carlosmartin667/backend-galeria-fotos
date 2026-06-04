using Fotografia.Application.DTOs.Reportes;
using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IReporteVentasService
{
    Task<ApiResponse<ReporteVentasResumenDto>> GetVentasResumenAsync(DateTime? desde, DateTime? hasta, CancellationToken cancellationToken = default);
}
