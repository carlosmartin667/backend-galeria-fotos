using Fotografia.Application.Helpers;

namespace Fotografia.Application.Services.Interfaces;

public interface IEmailService
{
    Task<ApiResponse<bool>> SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
