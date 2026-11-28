namespace SecurityMonitor.Contracts.Monitor;

public sealed record MonitorStatus(string Version, DateTimeOffset StartedAtUtc, int IncidentCount);