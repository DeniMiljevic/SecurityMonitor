using SecurityMonitor.Host.Windows;
using SecurityMonitor.Infrastructure.Common.Hosting;
using Serilog;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Configuration.AddJsonFile(Path.Combine(Environment.ExpandEnvironmentVariables("%ProgramData%"), "SecurityMonitor", "secrets.json"), optional: true, reloadOnChange: false);

builder.Services.AddSerilog(config => config.ReadFrom.Configuration(builder.Configuration));
builder.Services.AddSecurityMonitorCore(builder.Configuration);
builder.Services.AddWindowsServices();

var host = builder.Build();
host.Run();