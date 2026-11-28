using SecurityMonitor.Agent.Windows;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Services.AddAgent(builder.Configuration);

var host = builder.Build();
host.Run();
