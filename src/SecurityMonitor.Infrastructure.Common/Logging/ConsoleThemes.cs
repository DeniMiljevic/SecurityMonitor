using Serilog.Sinks.SystemConsole.Themes;

namespace SecurityMonitor.Infrastructure.Common.Logging;

public static class ConsoleThemes
{
    // ANSI colour codes: 32 green, 33 yellow, 31 red, 90 grey, 0 reset.
    // Serilog only uses them when the output is a real terminal, never in the journal or the log file.
    public static AnsiConsoleTheme Levels { get; } = new(new Dictionary<ConsoleThemeStyle, string>
    {
        [ConsoleThemeStyle.Text] = "\u001b[0m",
        [ConsoleThemeStyle.SecondaryText] = "\u001b[90m",
        [ConsoleThemeStyle.TertiaryText] = "\u001b[90m",
        [ConsoleThemeStyle.LevelVerbose] = "\u001b[90m",
        [ConsoleThemeStyle.LevelDebug] = "\u001b[90m",
        [ConsoleThemeStyle.LevelInformation] = "\u001b[32m",
        [ConsoleThemeStyle.LevelWarning] = "\u001b[33m",
        [ConsoleThemeStyle.LevelError] = "\u001b[31m",
        [ConsoleThemeStyle.LevelFatal] = "\u001b[1;31m"
    });
}
