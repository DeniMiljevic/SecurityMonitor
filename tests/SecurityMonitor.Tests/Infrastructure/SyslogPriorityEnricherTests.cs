using SecurityMonitor.Infrastructure.Common.Logging;
using Serilog.Events;

namespace SecurityMonitor.Tests.Infrastructure;

public class SyslogPriorityEnricherTests
{
    [Theory]
    [InlineData(LogEventLevel.Fatal, "<2>")]
    [InlineData(LogEventLevel.Error, "<3>")]
    [InlineData(LogEventLevel.Warning, "<4>")]
    [InlineData(LogEventLevel.Information, "<6>")]
    [InlineData(LogEventLevel.Debug, "<7>")]
    public void Prefix_MatchesSyslogPriority(LogEventLevel level, string expected)
    {
        Assert.Equal(expected, SyslogPriorityEnricher.Prefix(level));
    }
}
