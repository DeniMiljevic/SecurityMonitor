using Microsoft.EntityFrameworkCore;
using SecurityMonitor.Contracts;
using SecurityMonitor.Infrastructure.Data.Entities;

namespace SecurityMonitor.Infrastructure.Data.Queries;

public sealed class IncidentReader
{
    private readonly IDbContextFactory<SecurityMonitorDbContext> _dbFactory;

    public IncidentReader(IDbContextFactory<SecurityMonitorDbContext> dbFactory) => _dbFactory = dbFactory;

    public async Task<int> CountAsync(CancellationToken ct)
    {
        await using SecurityMonitorDbContext db = await _dbFactory.CreateDbContextAsync(ct);

        return await db.Incidents.CountAsync(ct);
    }

    public async Task<IReadOnlyList<IncidentSummary>> GetRecentAsync(int take, CancellationToken ct)
    {
        await using SecurityMonitorDbContext db = await _dbFactory.CreateDbContextAsync(ct);

        List<IncidentEntity> incidents = await db.Incidents
            .AsNoTracking()
            .OrderByDescending(incident => incident.TimestampUtc)
            .ThenByDescending(incident => incident.Id)
            .Take(Math.Clamp(take, 1, IMonitorService.MaxIncidents))
            .ToListAsync(ct);

        return incidents.Select(ToSummary).ToList();
    }

    private static IncidentSummary ToSummary(IncidentEntity incident) => new(
        incident.Id,
        new DateTimeOffset(DateTime.SpecifyKind(incident.TimestampUtc, DateTimeKind.Utc)),
        incident.AuthenticationMethod,
        incident.UserName,
        incident.ComputerName,
        incident.AttemptsInWindow,
        incident.CaptureSucceeded,
        incident.CaptureFailureReason);
}
