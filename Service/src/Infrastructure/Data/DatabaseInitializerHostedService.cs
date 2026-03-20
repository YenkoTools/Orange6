using Microsoft.Extensions.Hosting;

namespace Infrastructure.Data;

/// <summary>
/// Hosted service that initializes the SQLite database schema on application startup.
/// </summary>
public class DatabaseInitializerHostedService : IHostedService
{
    private readonly DatabaseInitializer _initializer;

    public DatabaseInitializerHostedService(DatabaseInitializer initializer)
    {
        _initializer = initializer;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _initializer.Initialize();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
