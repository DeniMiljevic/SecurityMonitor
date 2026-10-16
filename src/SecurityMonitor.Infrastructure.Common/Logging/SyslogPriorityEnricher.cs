using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace SecurityMonitor.Infrastructure.Common.Logging;

public sealed class SyslogPriorityEnricher : ILogEventEnricher
{
    public const string PropertyName = "SyslogPriority";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory) =>
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(PropertyName, Prefix(logEvent.Level)));

    public static string Prefix(LogEventLevel level) => level switch
    {
        LogEventLevel.Fatal => "<2>",
        LogEventLevel.Error => "<3>",
        LogEventLevel.Warning => "<4>",
        LogEventLevel.Information => "<6>",
        _ => "<7>"
    };
}

public static class SyslogPriorityEnricherExtensions
{
    public static LoggerConfiguration WithSyslogPriority(this LoggerEnrichmentConfiguration enrich) =>
        enrich.With<SyslogPriorityEnricher>();
}
