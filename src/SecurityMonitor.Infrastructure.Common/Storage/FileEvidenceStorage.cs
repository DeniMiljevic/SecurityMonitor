using System.Globalization;
using System.Text.Json;
using SecurityMonitor.Core.Configuration;
using SecurityMonitor.Core.Interfaces;
using SecurityMonitor.Core.Models;

namespace SecurityMonitor.Infrastructure.Common.Storage;

public sealed class FileEvidenceStorage : IEvidenceStorage
{
    private const string EventFileName = "event.json";
    private const string PhotoFileName = "photo.jpg";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _rootDirectory;
    private readonly Lock _folderLock = new();

    public FileEvidenceStorage(SecurityMonitorOptions options)
    {
        _rootDirectory = Path.GetFullPath(Environment.ExpandEnvironmentVariables(options.EvidenceDirectory));
    }

    public async Task<string> SaveAsync(SecurityIncident incident, CancellationToken ct = default)
    {
        string folder = CreateIncidentFolder(incident.TriggeringEvent.Timestamp);

        string? photoFile = null;
        if (incident.Capture.Success && incident.Capture.JpegData is { Length: > 0 } jpeg)
        {
            await File.WriteAllBytesAsync(Path.Combine(folder, PhotoFileName), jpeg, ct);
            photoFile = PhotoFileName;
        }

        EvidenceRecord record = EvidenceRecord.From(incident, photoFile);
        string json = JsonSerializer.Serialize(record, JsonOptions);
        await File.WriteAllTextAsync(Path.Combine(folder, EventFileName), json, ct);

        return folder;
    }

    private string CreateIncidentFolder(DateTimeOffset timestamp)
    {
        DateTimeOffset local = timestamp.ToLocalTime();
        string dayFolder = Path.Combine(_rootDirectory, local.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        string baseName = local.ToString("HH-mm-ss", CultureInfo.InvariantCulture);

        lock (_folderLock)
        {
            string folder = Path.Combine(dayFolder, baseName);
            for (int suffix = 2; Directory.Exists(folder); suffix++)
                folder = Path.Combine(dayFolder, $"{baseName}_{suffix}");

            Directory.CreateDirectory(folder);
            return folder;
        }
    }
}
