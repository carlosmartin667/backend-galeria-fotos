namespace Fotografia.Application.Services.Interfaces;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
