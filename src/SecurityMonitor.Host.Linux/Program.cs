using SecurityMonitor.Host.Linux;
using SecurityMonitor.Infrastructure.Common.Hosting;
using Serilog;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Configuration.AddJsonFile("/etc/securitymonitor/secrets.json", optional: true, reloadOnChange: false);

builder.Services.AddSerilog(config => config.ReadFrom.Configuration(builder.Configuration));
builder.Services.AddSecurityMonitorCore(builder.Configuration);
builder.Services.AddLinuxServices();

var host = builder.Build();
host.Run();
