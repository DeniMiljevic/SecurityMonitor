using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Services;
using SecurityMonitor.Infrastructure.Windows.Webcam;
using SecurityMonitor.Infrastructure.Windows.WindowsEventLog;

namespace SecurityMonitor.Host.Windows;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWindowsServices(this IServiceCollection services)
    {
        services.AddSingleton<IFailedLoginWatcher, WindowsFailedLoginWatcher>();
        services.AddSingleton<IFailedLoginClassifier, WindowsFailedLoginClassifier>();
        services.AddSingleton<IWebcamCapture, WindowsWebcamCapture>();

        return services;
    }
}