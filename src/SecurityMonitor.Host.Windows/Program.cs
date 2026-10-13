using SecurityMonitor.Host.Windows;
using Serilog;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Configuration.AddJsonFile(Path.Combine(Environment.ExpandEnvironmentVariables("%ProgramData%"), "SecurityMonitor", "secrets.json"), optional: true, reloadOnChange: false);

builder.Services.AddSerilog(config => config.ReadFrom.Configuration(builder.Configuration));
builder.Services.AddSecurityMonitor(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();