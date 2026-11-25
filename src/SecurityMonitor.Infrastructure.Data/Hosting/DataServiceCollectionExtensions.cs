using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Infrastructure.Common.Storage;
using SecurityMonitor.Infrastructure.Data.Queries;
using SecurityMonitor.Infrastructure.Data.Storage;

namespace SecurityMonitor.Infrastructure.Data.Hosting;

public static class DataServiceCollectionExtensions
{
    public static IServiceCollection AddIncidentDatabase(this IServiceCollection services, IConfiguration config)
    {
        string connectionString = Environment.ExpandEnvironmentVariables(config.GetConnectionString("SecurityMonitor")
            ?? throw new InvalidOperationException("Connection string 'SecurityMonitor' is missing."));

        services.AddDbContextFactory<SecurityMonitorDbContext>(options => options.UseSqlite(connectionString));
        services.AddHostedService<DatabaseMigrator>();
        services.AddSingleton<FileEvidenceStorage>();
        services.AddSingleton<IncidentReader>();

        services.Replace(ServiceDescriptor.Singleton<IEvidenceStorage>(provider => new DatabaseEvidenceStorage(
            provider.GetRequiredService<FileEvidenceStorage>(),
            provider.GetRequiredService<IDbContextFactory<SecurityMonitorDbContext>>(),
            provider.GetRequiredService<ILogger<DatabaseEvidenceStorage>>())));

        return services;
    }
}
