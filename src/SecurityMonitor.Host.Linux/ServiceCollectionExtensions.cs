using SecurityMonitor.Core.Configuration;
using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Services;
using SecurityMonitor.Infrastructure.Linux.Boot;
using SecurityMonitor.Infrastructure.Linux.Journal;
using SecurityMonitor.Infrastructure.Linux.Processes;
using SecurityMonitor.Infrastructure.Linux.Webcam;

namespace SecurityMonitor.Host.Linux;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLinuxServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IFailedLoginWatcher, LinuxFailedLoginWatcher>();
        services.AddSingleton<IFailedLoginClassifier, LinuxFailedLoginClassifier>();
        services.AddSingleton<ICommandRunner, ProcessCommandRunner>();

        LinuxWebcamOptions webcam = config.GetSection(LinuxWebcamOptions.SectionName).Get<LinuxWebcamOptions>() ?? new LinuxWebcamOptions();
        services.AddSingleton(webcam);
        services.AddSingleton<IWebcamCapture, LinuxWebcamCapture>();

        services.AddDualBoot(config);

        return services;
    }

    private static IServiceCollection AddDualBoot(this IServiceCollection services, IConfiguration config)
    {
        DualBootOptions options = config.GetSection(DualBootOptions.SectionName).Get<DualBootOptions>() ?? new DualBootOptions();

        if (!options.Enabled)
            return services;

        services.AddSingleton(options);
        services.AddSingleton<DualBootPolicy>();
        services.AddSingleton<DualBootMonitor>();
        services.AddSingleton<IBootSwitcher, EfiBootNextSwitcher>();
        services.AddSingleton<IBootConfigurationCheck, EfiBootConfigurationCheck>();
        services.AddSingleton<BootConfigurationMonitor>();
        services.AddSingleton<ISuccessfulLoginWatcher, LinuxSuccessfulLoginWatcher>();
        services.AddHostedService<DualBootWorker>();

        return services;
    }
}