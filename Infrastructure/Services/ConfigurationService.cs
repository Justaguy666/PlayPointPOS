using System.Text.Json;
using Application.Services;

namespace Infrastructure.Services;

public class ConfigurationService : IConfigurationService, ILocalizationPreferencesService
{
    private const int DefaultPort = 4000;
    private const string DefaultServerAddress = "http://localhost";

    private readonly string _configFilePath;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public string ServerAddress { get; private set; } = DefaultServerAddress;
    public int Port { get; private set; } = DefaultPort;
    public bool RememberMe { get; private set; }
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ServerAddress) &&
        IsValidPort(Port);

    public LocalizationPreferences Preferences { get; private set; }

    public ConfigurationService(string configFilePath, LocalizationPreferences? defaultPreferences = null)
    {
        _configFilePath = configFilePath;
        Preferences = ClonePreferences(defaultPreferences ?? new LocalizationPreferences());
    }

    public async Task LoadAsync()
    {
        if (!File.Exists(_configFilePath))
            return;

        try
        {
            var json = await File.ReadAllTextAsync(_configFilePath);
            if (string.IsNullOrWhiteSpace(json))
                return;

            var data = JsonSerializer.Deserialize<ConfigData>(json, _jsonOptions);
            if (data == null)
                return;

            ServerAddress = string.IsNullOrWhiteSpace(data.ServerAddress)
                ? DefaultServerAddress
                : data.ServerAddress;
            Port = IsValidPort(data.Port) ? data.Port : DefaultPort;
            RememberMe = data.RememberMe;
            Preferences = ClonePreferences(data.LocalizationPreferences, Preferences);
        }
        catch
        {
            // Config file corrupt - start fresh.
        }
    }

    public async Task SaveAsync(string serverAddress, int port, bool rememberMe)
    {
        ServerAddress = serverAddress;
        Port = port;
        RememberMe = rememberMe;

        var data = new ConfigData
        {
            ServerAddress = serverAddress,
            Port = port,
            RememberMe = rememberMe,
            LocalizationPreferences = ClonePreferences(Preferences),
        };

        string? directory = Path.GetDirectoryName(_configFilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(data, _jsonOptions);
        await File.WriteAllTextAsync(_configFilePath, json);
    }

    public async Task SaveAsync(LocalizationPreferences preferences)
    {
        Preferences = ClonePreferences(preferences);

        var data = new ConfigData
        {
            ServerAddress = ServerAddress,
            Port = Port,
            RememberMe = RememberMe,
            LocalizationPreferences = ClonePreferences(Preferences),
        };

        string? directory = Path.GetDirectoryName(_configFilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(data, _jsonOptions);
        await File.WriteAllTextAsync(_configFilePath, json);
    }

    private static LocalizationPreferences ClonePreferences(
        LocalizationPreferences? source,
        LocalizationPreferences? fallback = null)
    {
        LocalizationPreferences basePreferences = fallback ?? new LocalizationPreferences();

        return new LocalizationPreferences
        {
            Language = string.IsNullOrWhiteSpace(source?.Language)
                ? basePreferences.Language
                : source.Language.Trim(),
            Currency = string.IsNullOrWhiteSpace(source?.Currency)
                ? basePreferences.Currency
                : source.Currency.Trim().ToUpperInvariant(),
            TimeZone = string.IsNullOrWhiteSpace(source?.TimeZone)
                ? basePreferences.TimeZone
                : source.TimeZone.Trim(),
            DateFormat = string.IsNullOrWhiteSpace(source?.DateFormat)
                ? basePreferences.DateFormat
                : source.DateFormat.Trim(),
        };
    }

    private static bool IsValidPort(int port) => port is >= 1 and <= 65535;

    private class ConfigData
    {
        public string ServerAddress { get; set; } = string.Empty;
        public int Port { get; set; } = DefaultPort;
        public bool RememberMe { get; set; }
        public LocalizationPreferences LocalizationPreferences { get; set; } = new();
    }
}
