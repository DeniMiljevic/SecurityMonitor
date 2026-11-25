using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SecurityMonitor.Host.Windows;
using SecurityMonitor.Host.Windows.Workers;
using GenericHost = Microsoft.Extensions.Hosting.Host;

namespace SecurityMonitor.Tests.Windows;

public class WindowsHostCompositionTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void AllRegistrations_CanBeCreated(bool telegram, bool dualBoot)
    {
        HostApplicationBuilder builder = CreateBuilder(telegram, dualBoot);

        using IHost host = builder.Build();

        Assert.NotNull(host);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BootOrderCheck_IsRegisteredOnlyWithDualBoot(bool dualBoot)
    {
        HostApplicationBuilder builder = CreateBuilder(telegram: false, dualBoot);

        bool registered = builder.Services.Any(s => s.ImplementationType == typeof(BootConfigurationWorker));

        Assert.Equal(dualBoot, registered);
    }

    private static HostApplicationBuilder CreateBuilder(bool telegram, bool dualBoot)
    {
        var builder = GenericHost.CreateApplicationBuilder(new HostApplicationBuilderSettings { DisableDefaults = true });

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Telegram:Enabled"] = telegram.ToString(),
            ["Telegram:BotToken"] = "test-token",
            ["Telegram:ChatId"] = "1",
            ["DualBoot:Enabled"] = dualBoot.ToString(),
            ["ConnectionStrings:SecurityMonitor"] = "Data Source=:memory:"
        });

        builder.ConfigureContainer(new DefaultServiceProviderFactory(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        }));

        builder.Services.AddWindowsHost(builder.Configuration);
        return builder;
    }
}
