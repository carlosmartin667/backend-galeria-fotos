using Fotografia.Api.Helpers;

namespace Fotografia.Api.Services.Interfaces;

public interface IEmailService
{
    Task<ApiResponse<bool>> SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
