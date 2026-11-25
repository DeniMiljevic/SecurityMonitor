using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SecurityMonitor.Contracts;
using SecurityMonitor.Infrastructure.Data;

namespace SecurityMonitor.Host.Windows.Pipe;

internal sealed class MonitorService : IMonitorService
{
    private static readonly DateTimeOffset StartedAtUtc = Process.GetCurrentProcess().StartTime.ToUniversalTime();
    private readonly IDbContextFactory<SecurityMonitorDbContext> _dbFactory;

    public MonitorService(IDbContextFactory<SecurityMonitorDbContext> dbFactory) => _dbFactory = dbFactory;

    public async Task<MonitorStatus> GetStatusAsync(CancellationToken ct)
    {
        await using SecurityMonitorDbContext db = await _dbFactory.CreateDbContextAsync(ct);

        return new MonitorStatus(
            Version: typeof(MonitorService).Assembly.GetName().Version?.ToString() ?? "unknown",
            StartedAtUtc: StartedAtUtc,
            IncidentCount: await db.Incidents.CountAsync(ct));
    }
}
