namespace SecurityMonitor.Host.Windows;

internal static class WindowsPaths
{
    public static string DataDirectory { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SecurityMonitor");
    public static string SecretsFile { get; } = Path.Combine(DataDirectory, "secrets.json");
}
