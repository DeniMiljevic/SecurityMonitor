using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Services;
using SecurityMonitor.Host;
using SecurityMonitor.Infrastructure.WindowsEventLog;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog(config => config.ReadFrom.Configuration(builder.Configuration));

builder.Services.AddSingleton<FailedLoginClassifier>();
builder.Services.AddSingleton<IFailedLoginWatcher, WindowsFailedLoginWatcher>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
