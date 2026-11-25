using SecurityMonitor.Api.Endpoints;
using SecurityMonitor.Api.Monitor;
using SecurityMonitor.Api.Security;
using SecurityMonitor.Contracts;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Services.AddWindowsService(options => options.ServiceName = "SecurityMonitorApi");
builder.Services.AddSingleton<ApiToken>();
builder.Services.AddSingleton<IMonitorService, MonitorClient>();

var app = builder.Build();

app.UseMiddleware<ApiTokenMiddleware>();
app.MapStatusEndpoints();

app.Run();
