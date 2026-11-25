namespace SecurityMonitor.Contracts;

public sealed record MonitorStatus(string Version, DateTimeOffset StartedAtUtc, int IncidentCount);