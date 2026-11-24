using Microsoft.Extensions.Hosting.WindowsServices;
using SecurityMonitor.Core.Configuration;
using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Services;
using SecurityMonitor.Infrastructure.Common.Firmware;
using SecurityMonitor.Infrastructure.Common.Hosting;
using SecurityMonitor.Infrastructure.Data;
using SecurityMonitor.Infrastructure.Windows.Firmware;
using SecurityMonitor.Infrastructure.Windows.Webcam;
using SecurityMonitor.Infrastructure.Windows.WindowsEventLog;
using Serilog;

namespace SecurityMonitor.Host.Windows;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWindowsHost(this IServiceCollection services, IConfiguration config)
    {
        services.AddWindowsService(options => options.ServiceName = "SecurityMonitor");
        services.AddSerilog(serilog => serilog.ReadFrom.Configuration(config));
        services.AddSecurityMonitorCore(config);
        services.AddWindowsServices(config);
        services.AddIncidentDatabase(config);

        return services;
    }

    private static IServiceCollection AddWindowsServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddHostedService<DataDirectoryProtector>();
        services.AddSingleton<IFailedLoginWatcher, WindowsFailedLoginWatcher>();
        services.AddSingleton<IFailedLoginClassifier, WindowsFailedLoginClassifier>();
        
        if (WindowsServiceHelpers.IsWindowsService())
            services.AddSingleton<IWebcamCapture, ServiceSessionWebcamCapture>();
        else
            services.AddSingleton<IWebcamCapture, WindowsWebcamCapture>();

        services.AddBootOrderCheck(config);

        return services;
    }

    private static IServiceCollection AddBootOrderCheck(this IServiceCollection services, IConfiguration config)
    {
        DualBootOptions options = config.GetSection(DualBootOptions.SectionName).Get<DualBootOptions>() ?? new DualBootOptions();

        if (!options.Enabled)
            return services;

        services.AddSingleton<IUefiVariables, WindowsUefiVariables>();
        services.AddSingleton<IBootConfigurationCheck, UefiBootOrderCheck>();
        services.AddSingleton<BootConfigurationMonitor>();
        services.AddHostedService<BootConfigurationWorker>();

        return services;
    }
}