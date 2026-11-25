using SecurityMonitor.Contracts;
using SecurityMonitor.Infrastructure.Data.Entities;
using SecurityMonitor.Infrastructure.Data.Queries;
using SecurityMonitor.Tests.Fakes;

namespace SecurityMonitor.Tests.Data;

public class IncidentReaderTests
{
    private static readonly DateTime Older = new(2026, 10, 10, 20, 0, 0);
    private static readonly DateTime Newer = new(2026, 10, 11, 8, 30, 0);

    [Fact]
    public async Task GetRecentAsync_ReturnsTheNewestFirst()
    {
        using var database = FakeDatabase.Migrated();
        await AddIncidentsAsync(database, Older, Newer);

        IReadOnlyList<IncidentSummary> incidents = await new IncidentReader(database).GetRecentAsync(take: 10, CancellationToken.None);

        Assert.Equal([Newer, Older], incidents.Select(incident => incident.TimestampUtc.UtcDateTime));
    }

    [Fact]
    public async Task GetRecentAsync_ReturnsAtMostTake()
    {
        using var database = FakeDatabase.Migrated();
        await AddIncidentsAsync(database, Older, Newer, Newer);

        IReadOnlyList<IncidentSummary> incidents = await new IncidentReader(database).GetRecentAsync(take: 2, CancellationToken.None);

        Assert.Equal(2, incidents.Count);
    }

    [Fact]
    public async Task GetRecentAsync_ReturnsTheTimestampInUtc()
    {
        using var database = FakeDatabase.Migrated();
        await AddIncidentsAsync(database, Older);

        IncidentSummary incident = Assert.Single(await new IncidentReader(database).GetRecentAsync(take: 10, CancellationToken.None));

        Assert.Equal(TimeSpan.Zero, incident.TimestampUtc.Offset);
        Assert.Equal(new DateTimeOffset(Older, TimeSpan.Zero), incident.TimestampUtc);
    }

    [Fact]
    public async Task CountAsync_CountsEveryIncident()
    {
        using var database = FakeDatabase.Migrated();
        await AddIncidentsAsync(database, Older, Newer, Newer);

        Assert.Equal(3, await new IncidentReader(database).CountAsync(CancellationToken.None));
    }

    private static async Task AddIncidentsAsync(FakeDatabase database, params DateTime[] timestampsUtc)
    {
        await using var db = database.CreateDbContext();

        foreach (DateTime timestamp in timestampsUtc)
            db.Incidents.Add(new IncidentEntity
            {
                TimestampUtc = timestamp,
                AuthenticationMethod = "Pin",
                ComputerName = "PC",
                EvidenceFolder = @"C:\evidence"
            });

        await db.SaveChangesAsync();
    }
}