using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Services;
using SecurityMonitor.Infrastructure.Linux.Journal;
using SecurityMonitor.Infrastructure.Linux.Webcam;

namespace SecurityMonitor.Host.Linux;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLinuxServices(this IServiceCollection services)
    {
        services.AddSingleton<IFailedLoginWatcher, LinuxFailedLoginWatcher>();
        services.AddSingleton<IFailedLoginClassifier, LinuxFailedLoginClassifier>();
        services.AddSingleton<IWebcamCapture, LinuxWebcamCapture>();

        return services;
    }
}
