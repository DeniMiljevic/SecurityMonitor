using SecurityMonitor.Agent.Windows.Pipe;
using SecurityMonitor.Agent.Windows.Workers;
using SecurityMonitor.Contracts.Agent;
using Serilog;

namespace SecurityMonitor.Agent.Windows;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgent(this IServiceCollection services, IConfiguration config)
    {
        services.AddSerilog(serilog => serilog.ReadFrom.Configuration(config));
        services.AddSingleton<IAgentService, AgentService>();
        services.AddHostedService<MonitorConnector>();

        return services;
    }
}
