using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SecurityMonitor.Infrastructure.Data;

namespace SecurityMonitor.Tests.Fakes;

internal sealed class FakeDatabase : IDbContextFactory<SecurityMonitorDbContext>, IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly DbContextOptions<SecurityMonitorDbContext> _options;

    private FakeDatabase()
    {
        _connection.Open();
        _options = new DbContextOptionsBuilder<SecurityMonitorDbContext>().UseSqlite(_connection).Options;
    }

    public static FakeDatabase Migrated()
    {
        var database = new FakeDatabase();
        using SecurityMonitorDbContext db = database.CreateDbContext();
        db.Database.Migrate();
        return database;
    }

    public static FakeDatabase WithoutTables() => new();

    public SecurityMonitorDbContext CreateDbContext() => new(_options);

    public void Dispose() => _connection.Dispose();
}
