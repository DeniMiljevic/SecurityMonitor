using SecurityMonitor.Core.Configuration;
using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Services;
using SecurityMonitor.Infrastructure.Common.Firmware;
using SecurityMonitor.Infrastructure.Windows.Firmware;
using SecurityMonitor.Infrastructure.Windows.Webcam;
using SecurityMonitor.Infrastructure.Windows.WindowsEventLog;

namespace SecurityMonitor.Host.Windows;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWindowsServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IFailedLoginWatcher, WindowsFailedLoginWatcher>();
        services.AddSingleton<IFailedLoginClassifier, WindowsFailedLoginClassifier>();
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
        services.AddSingleton<BootConfigurationWorker>();
        services.AddHostedService<BootConfigurationWorker>();

        return services;
    }
}