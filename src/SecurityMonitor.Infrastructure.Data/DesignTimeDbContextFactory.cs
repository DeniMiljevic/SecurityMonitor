using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SecurityMonitor.Infrastructure.Data;

internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SecurityMonitorDbContext>
{
    public SecurityMonitorDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<SecurityMonitorDbContext>()
            .UseSqlite("Data Source=design-time.db")
            .Options);
}
