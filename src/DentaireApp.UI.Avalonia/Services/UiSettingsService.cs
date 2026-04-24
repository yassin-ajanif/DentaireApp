using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DentaireApp.UI.Avalonia.Services;

public sealed class UiSettings
{
    [JsonPropertyName("averageTimeSpentWithPatient")]
    public int AverageTimeSpentWithPatient { get; set; } = 15;

    /// <summary>Local instant when the last consultation was marked done; used as prediction anchor. Cleared when the calendar day changes.</summary>
    [JsonPropertyName("lastTerminatedAt")]
    public DateTime? LastTerminatedAt { get; set; }
}

public sealed class UiSettingsFileService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    private readonly string settingsFilePath;

    public UiSettingsFileService(string? settingsFilePath = null)
    {
        this.settingsFilePath = settingsFilePath
            ?? Path.Combine(AppContext.BaseDirectory, "settings.json");
    }

    public async Task<UiSettings> LoadAsync()
    {
        if (!File.Exists(settingsFilePath))
        {
            var defaults = new UiSettings();
            await SaveAsync(defaults);
            return defaults;
        }

        try
        {
            var json = await File.ReadAllTextAsync(settingsFilePath);
            var settings = JsonSerializer.Deserialize<UiSettings>(json, JsonOptions) ?? new UiSettings();
            if (ClearLastTerminatedIfStale(settings))
            {
                await SaveAsync(settings);
            }

            return settings;
        }
        catch (JsonException)
        {
            var defaults = new UiSettings();
            await SaveAsync(defaults);
            return defaults;
        }
    }

    public async Task SaveAsync(UiSettings settings)
    {
        ClearLastTerminatedIfStale(settings);
        var normalized = new UiSettings
        {
            AverageTimeSpentWithPatient = settings.AverageTimeSpentWithPatient <= 0
                ? 15
                : settings.AverageTimeSpentWithPatient,
            LastTerminatedAt = settings.LastTerminatedAt,
        };
        var json = JsonSerializer.Serialize(normalized, JsonOptions);
        await File.WriteAllTextAsync(settingsFilePath, json);
    }

    /// <summary>Returns true if <paramref name="settings"/> was modified (caller may persist).</summary>
    private static bool ClearLastTerminatedIfStale(UiSettings settings)
    {
        if (!settings.LastTerminatedAt.HasValue)
        {
            return false;
        }

        var today = DateOnly.FromDateTime(DateTime.Now);
        var anchorDay = DateOnly.FromDateTime(settings.LastTerminatedAt.Value);
        if (anchorDay == today)
        {
            return false;
        }

        settings.LastTerminatedAt = null;
        return true;
    }
}
