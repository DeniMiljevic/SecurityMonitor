namespace SecurityMonitor.Host.Windows.Configuration;

internal sealed class ApiTokenOptions
{
    public const string SectionName = "Api";

    public string TokenFile { get; set; } = "";
    public string ServiceAccount { get; set; } = "";
    public List<string> DashboardUsers { get; set; } = [];
}
