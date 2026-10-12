namespace SecurityMonitor.Core.Configuration;

public sealed class TelegramOptions
{
    public const string SectionName = "Telegram";
    public bool Enabled { get; init; } = false;
    public string BotToken { get; init; } = "";
    public string ChatId { get; init; } = "";
    public int NotificationCooldownMinutes { get; init; } = 0;
}
