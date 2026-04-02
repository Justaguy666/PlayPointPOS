using System.Text.Json;
using Application.Services;

namespace Infrastructure.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly string _configFilePath;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public string ServerAddress { get; private set; } = string.Empty;
    public string ApiKey { get; private set; } = string.Empty;
    public bool RememberMe { get; private set; }
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ServerAddress) && !string.IsNullOrWhiteSpace(ApiKey);

    public ConfigurationService(string configFilePath)
    {
        _configFilePath = configFilePath;
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

            ServerAddress = data.ServerAddress ?? string.Empty;
            RememberMe = data.RememberMe;

            if (data.RememberMe)
            {
                ApiKey = data.ApiKey ?? string.Empty;
            }
        }
        catch
        {
            // Config file corrupt - start fresh.
        }
    }

    public async Task SaveAsync(string serverAddress, string apiKey, bool rememberMe)
    {
        ServerAddress = serverAddress;
        ApiKey = apiKey;
        RememberMe = rememberMe;

        var data = new ConfigData
        {
            ServerAddress = serverAddress,
            ApiKey = rememberMe ? apiKey : string.Empty,
            RememberMe = rememberMe,
        };

        string? directory = Path.GetDirectoryName(_configFilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(data, _jsonOptions);
        await File.WriteAllTextAsync(_configFilePath, json);
    }

    private class ConfigData
    {
        public string ServerAddress { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}
