using Microsoft.EntityFrameworkCore;

namespace Fotografia.Api.Data;

public static class DbInitializer
{
    public static async Task ApplyMigrationsAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (dbContext.Database.IsRelational())
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
    }
}
