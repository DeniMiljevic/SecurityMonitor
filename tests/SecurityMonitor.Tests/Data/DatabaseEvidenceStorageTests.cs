using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SecurityMonitor.Core.Models;
using SecurityMonitor.Infrastructure.Data.Entities;
using SecurityMonitor.Infrastructure.Data.Storage;
using SecurityMonitor.Tests.Fakes;
using SecurityMonitor.Tests.TestData;

namespace SecurityMonitor.Tests.Data;

public class DatabaseEvidenceStorageTests
{
    private static readonly SecurityIncident Incident = new(
        TestEvents.PinFailure(), AuthenticationMethod.Pin, AttemptsInWindow: 3, CaptureResult.Failed("No camera found"));

    [Fact]
    public async Task SaveAsync_AddsOneRowPointingToTheEvidenceFolder()
    {
        using var database = FakeDatabase.Migrated();
        var storage = CreateStorage(FakeEvidenceStorage.Working(), database);

        string folder = await storage.SaveAsync(Incident);

        await using var db = database.CreateDbContext();
        IncidentEntity row = Assert.Single(await db.Incidents.ToListAsync());
        Assert.Equal(folder, row.EvidenceFolder);
        Assert.Equal("Pin", row.AuthenticationMethod);
        Assert.Equal(3, row.AttemptsInWindow);
        Assert.False(row.CaptureSucceeded);
        Assert.Equal("No camera found", row.CaptureFailureReason);
    }

    [Fact]
    public async Task SaveAsync_StoresTheTimestampInUtc()
    {
        using var database = FakeDatabase.Migrated();
        var storage = CreateStorage(FakeEvidenceStorage.Working(), database);

        await storage.SaveAsync(Incident);

        await using var db = database.CreateDbContext();
        IncidentEntity row = await db.Incidents.SingleAsync();
        Assert.Equal(new DateTime(2026, 10, 10, 20, 0, 0), row.TimestampUtc);
    }

    [Fact]
    public async Task SaveAsync_DatabaseFails_StillSavesTheEvidence()
    {
        using var database = FakeDatabase.WithoutTables();
        var files = FakeEvidenceStorage.Working();
        var storage = CreateStorage(files, database);

        string folder = await storage.SaveAsync(Incident);

        Assert.Equal(@"C:\fake\evidence", folder);
        Assert.Single(files.Saved);
    }

    [Fact]
    public async Task SaveAsync_FileStorageFails_ThrowsAndAddsNoRow()
    {
        using var database = FakeDatabase.Migrated();
        var storage = CreateStorage(FakeEvidenceStorage.ThatThrows(), database);

        await Assert.ThrowsAsync<IOException>(() => storage.SaveAsync(Incident));

        await using var db = database.CreateDbContext();
        Assert.Empty(await db.Incidents.ToListAsync());
    }

    [Fact]
    public void Model_MatchesTheLatestMigration()
    {
        using var database = FakeDatabase.WithoutTables();
        using var db = database.CreateDbContext();

        Assert.False(db.Database.HasPendingModelChanges());
    }

    private static DatabaseEvidenceStorage CreateStorage(FakeEvidenceStorage files, FakeDatabase database) =>
        new(files, database, NullLogger<DatabaseEvidenceStorage>.Instance);
}
