using SecurityMonitor.Host.Windows;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Configuration.AddJsonFile(WindowsPaths.SecretsFile, optional: true, reloadOnChange: false);
builder.Services.AddWindowsHost(builder.Configuration);

var host = builder.Build();
host.Run();