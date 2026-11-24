using SecurityMonitor.Api.Endpoints;
using SecurityMonitor.Api.Security;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Services.AddWindowsService(options => options.ServiceName = "SecurityMonitorApi");
builder.Services.AddSingleton<ApiToken>();

var app = builder.Build();

app.UseMiddleware<ApiTokenMiddleware>();
app.MapStatusEndpoints();

app.Run();
