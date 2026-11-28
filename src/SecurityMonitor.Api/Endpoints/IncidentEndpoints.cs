using SecurityMonitor.Contracts.Monitor;

namespace SecurityMonitor.Api.Endpoints;

public static class IncidentEndpoints
{
    private const int DefaultTake = 50;

    public static IEndpointRouteBuilder MapIncidentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/incidents", GetIncidentsAsync);

        return app;
    }

    private static async Task<IResult> GetIncidentsAsync(
        IMonitorService monitor,
        ILoggerFactory loggerFactory,
        CancellationToken ct,
        int take = DefaultTake)
    {
        if (take is < 1 or > IMonitorService.MaxIncidents)
            return Results.Problem($"'take' must be between 1 and {IMonitorService.MaxIncidents}.", statusCode: StatusCodes.Status400BadRequest);
        
        try
        {
            return Results.Ok(await monitor.GetIncidentsAsync(take, ct));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            loggerFactory.CreateLogger(typeof(IncidentEndpoints)).LogWarning(ex, "The monitoring service could not be reached");
            return Results.Problem("The monitoring service could not be reached.", statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}
