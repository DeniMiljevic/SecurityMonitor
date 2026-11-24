using System.Diagnostics;

namespace SecurityMonitor.Api.Endpoints;

public static class StatusEndpoints
{
    private static readonly DateTimeOffset StartedAtUtc = Process.GetCurrentProcess().StartTime.ToUniversalTime();

    public static IEndpointRouteBuilder MapStatusEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/status", () => new StatusResponse(
            Service: "SecurityMonitor.Api",
            Version: typeof(StatusEndpoints).Assembly.GetName().Version?.ToString() ?? "unknown",
            StartedAtUtc: StartedAtUtc));

        return app;
    }
}

public sealed record StatusResponse(string Service, string Version, DateTimeOffset StartedAtUtc);
