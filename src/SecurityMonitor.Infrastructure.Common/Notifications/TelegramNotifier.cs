using System.Globalization;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using SecurityMonitor.Core.Configuration;
using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Infrastructure.Common.Notifications;

public sealed class TelegramNotifier : INotifier
{
    private readonly HttpClient _http;
    private readonly TelegramOptions _options;
    private readonly ILogger<TelegramNotifier> _logger;

    public TelegramNotifier(HttpClient http, TelegramOptions options, ILogger<TelegramNotifier> logger)
    {
        _http = http;
        _options = options;
        _logger = logger;
    }

    public async Task NotifyAsync(SecurityIncident incident, CancellationToken ct = default)
    {
        string text = BuildMessage(incident);

        if (incident.Capture.Success && incident.Capture.JpegData is { Length: > 0 } jpeg)
            await SendPhotoAsync(jpeg, text, ct);
        else
            await SendMessageAsync(text, ct);

        _logger.LogInformation("Telegram notification sent");
    }

    private static string BuildMessage(SecurityIncident incident)
    {
        FailedLoginEvent e = incident.TriggeringEvent;
        string time = e.Timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        string capture = incident.Capture.Success ? "photo attached" : $"no photo ({incident.Capture.FailureReason})";

        return $"\U0001F6A8 Failed login on {e.ComputerName}\n Method: {incident.Method}\n Time: {time}\n Attempts: {incident.AttemptsInWindow}\n Capture: {capture}";
    }

    private async Task SendMessageAsync(string text, CancellationToken ct)
    {
        string url = $"https://api.telegram.org/bot{_options.BotToken}/sendMessage";
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["chat_id"] = _options.ChatId,
            ["text"] = text
        });

        using HttpResponseMessage response = await _http.PostAsync(url, form, ct);
        await EnsureOkAsync(response, ct);
    }

    private async Task SendPhotoAsync(byte[] jpeg, string caption, CancellationToken ct)
    {
        string url = $"https://api.telegram.org/bot{_options.BotToken}/sendPhoto";

        using var form = new MultipartFormDataContent
        {
            { new StringContent(_options.ChatId), "chat_id" },
            { new StringContent(caption), "caption" }
        };

        var photo = new ByteArrayContent(jpeg);
        photo.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        form.Add(photo, "photo", "photo.jpg");

        using HttpResponseMessage response = await _http.PostAsync(url, form, ct);
        await EnsureOkAsync(response, ct);
    }

    private static async Task EnsureOkAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return;

        string body = await response.Content.ReadAsStringAsync(ct);
        throw new InvalidOperationException($"Telegram API returned {(int)response.StatusCode}: {body}");
    }
}
