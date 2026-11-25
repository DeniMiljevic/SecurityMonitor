namespace SecurityMonitor.Api.Security;

public sealed class ApiTokenMiddleware
{
    private const string Scheme = "Bearer ";

    private readonly RequestDelegate _next;
    private readonly ApiToken _token;

    public ApiTokenMiddleware(RequestDelegate next, ApiToken token)
    {
        _next = next;
        _token = token;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string? header = context.Request.Headers.Authorization;
        string? presented = header is not null && header.StartsWith(Scheme, StringComparison.Ordinal) ? header[Scheme.Length..] : null;
    
        if (!_token.Matches(presented))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        await _next(context);
    }
}
