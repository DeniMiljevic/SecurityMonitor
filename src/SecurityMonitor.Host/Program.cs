using SecurityMonitor.Core.Configuration;
using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Services;
using SecurityMonitor.Host;
using SecurityMonitor.Infrastructure.WindowsEventLog;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog(config => config.ReadFrom.Configuration(builder.Configuration));

SecurityMonitorOptions monitorOptions = builder.Configuration.GetSection(SecurityMonitorOptions.SectionName).Get<SecurityMonitorOptions>() ?? new SecurityMonitorOptions();

builder.Services.AddSingleton(monitorOptions);

builder.Services.AddSingleton<FailedLoginClassifier>();
builder.Services.AddSingleton<FailedLoginPolicy>();
builder.Services.AddSingleton<IFailedLoginWatcher, WindowsFailedLoginWatcher>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
