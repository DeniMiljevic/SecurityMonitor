using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SecurityMonitor.Infrastructure.Data;

public sealed class DatabaseMigrator : IHostedService
{
    private readonly IDbContextFactory<SecurityMonitorDbContext> _dbFactory;
    private readonly ILogger<DatabaseMigrator> _logger;

    public DatabaseMigrator(IDbContextFactory<SecurityMonitorDbContext> dbFactory, ILogger<DatabaseMigrator> logger)
    {
        _dbFactory = dbFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        try
        {
            await using SecurityMonitorDbContext db = await _dbFactory.CreateDbContextAsync(ct);
            await db.Database.MigrateAsync(ct);
            _logger.LogInformation("Incident database is up to date");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Could not create or update the incident database. Evidence is still saved as files");
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
