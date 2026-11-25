using System.Diagnostics;
using SecurityMonitor.Contracts;

namespace SecurityMonitor.Api.Endpoints;

public static class StatusEndpoints
{
    private static readonly DateTimeOffset StartedAtUtc = Process.GetCurrentProcess().StartTime.ToUniversalTime();

    public static IEndpointRouteBuilder MapStatusEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/status", async (IMonitorService monitor, ILoggerFactory loggerFactory, CancellationToken ct) =>
            new StatusResponse(
                Service: "SecurityMonitor.Api",
                Version: typeof(StatusEndpoints).Assembly.GetName().Version?.ToString() ?? "unknown",
                StartedAtUtc: StartedAtUtc,
                Monitor: await TryGetMonitorStatusAsync(monitor, loggerFactory.CreateLogger(typeof(StatusEndpoints)), ct)));

        return app;
    }

    private static async Task<MonitorStatus?> TryGetMonitorStatusAsync(IMonitorService monitor, ILogger logger, CancellationToken ct)
    {
        try
        {
            return await monitor.GetStatusAsync(ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "The monitoring service could not be reached");
            return null;
        }
    }
}

public sealed record StatusResponse(string Service, string Version, DateTimeOffset StartedAtUtc, MonitorStatus? Monitor);
