using SecurityMonitor.Core.Configuration;
using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Services;
using SecurityMonitor.Infrastructure.Common.Notifications;
using SecurityMonitor.Infrastructure.Common.Storage;
using SecurityMonitor.Infrastructure.Windows.Webcam;
using SecurityMonitor.Infrastructure.Windows.WindowsEventLog;

namespace SecurityMonitor.Host.Windows;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSecurityMonitor(this IServiceCollection services, IConfiguration config)
    {
        SecurityMonitorOptions monitorOptions = config.GetSection(SecurityMonitorOptions.SectionName).Get<SecurityMonitorOptions>() ?? new SecurityMonitorOptions();
        services.AddSingleton(monitorOptions);

        TelegramOptions telegramOptions = config.GetSection(TelegramOptions.SectionName).Get<TelegramOptions>() ?? new TelegramOptions();
        services.AddSingleton(telegramOptions);

        services.AddSingleton(new NotificationThrottle(TimeSpan.FromMinutes(telegramOptions.NotificationCooldownMinutes)));

        services.AddSingleton<FailedLoginClassifier>();
        services.AddSingleton<FailedLoginPolicy>();
        services.AddSingleton<FailedLoginMonitor>();
        services.AddSingleton<IFailedLoginWatcher, WindowsFailedLoginWatcher>();
        services.AddSingleton<IWebcamCapture, WindowsWebcamCapture>();
        services.AddSingleton<IEvidenceStorage, FileEvidenceStorage>();

        services.AddNotifier(telegramOptions);

        return services;
    }

    private static IServiceCollection AddNotifier(this IServiceCollection services, TelegramOptions telegram)
    {
        if (telegram.Enabled)
            services.AddHttpClient<INotifier, TelegramNotifier>(client => client.Timeout = TimeSpan.FromSeconds(15));
        else
            services.AddSingleton<INotifier, NullNotifier>();

        return services;
    }
}