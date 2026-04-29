using Infrastructure.Services;
using Application.Services;

namespace IntegrationTests;

public class ConfigurationServiceTests : IDisposable
{
    private readonly string _tempDirectory;

    public ConfigurationServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "PlayPointPOS.Tests", Guid.NewGuid().ToString("N"));
    }

    [Fact]
    public async Task SaveAndLoadAsync_RetainsApiKeyOnlyWhenRememberMeIsEnabled()
    {
        string configPath = Path.Combine(_tempDirectory, "config.json");

        var service = new ConfigurationService(configPath);
        await service.SaveAsync("https://server", 3000, "secret-key", rememberMe: true);

        var remembered = new ConfigurationService(configPath);
        await remembered.LoadAsync();

        Assert.Equal("https://server", remembered.ServerAddress);
        Assert.Equal(3000, remembered.Port);
        Assert.Equal("secret-key", remembered.ApiKey);
        Assert.True(remembered.RememberMe);
        Assert.True(remembered.IsConfigured);

        await service.SaveAsync("https://server", 8080, "secret-key", rememberMe: false);

        var notRemembered = new ConfigurationService(configPath);
        await notRemembered.LoadAsync();

        Assert.Equal("https://server", notRemembered.ServerAddress);
        Assert.Equal(8080, notRemembered.Port);
        Assert.Equal(string.Empty, notRemembered.ApiKey);
        Assert.False(notRemembered.RememberMe);
        Assert.False(notRemembered.IsConfigured);
    }

    [Fact]
    public async Task SaveAndLoadAsync_RetainsLocalizationPreferences()
    {
        string configPath = Path.Combine(_tempDirectory, "config.json");

        var service = new ConfigurationService(configPath);
        await service.SaveAsync(new LocalizationPreferences
        {
            Language = "vi-VN",
            Currency = "USD",
            TimeZone = "+9",
            DateFormat = "yyyy-MM-dd",
        });

        var reloaded = new ConfigurationService(configPath);
        await reloaded.LoadAsync();

        Assert.Equal(3000, reloaded.Port);
        Assert.Equal("vi-VN", reloaded.Preferences.Language);
        Assert.Equal("USD", reloaded.Preferences.Currency);
        Assert.Equal("+9", reloaded.Preferences.TimeZone);
        Assert.Equal("yyyy-MM-dd", reloaded.Preferences.DateFormat);
    }

    [Fact]
    public async Task LoadAsync_UsesDefaultPortWhenPortIsMissing()
    {
        string configPath = Path.Combine(_tempDirectory, "config.json");
        Directory.CreateDirectory(_tempDirectory);
        await File.WriteAllTextAsync(
            configPath,
            "{\"serverAddress\":\"https://server\",\"apiKey\":\"secret-key\",\"rememberMe\":true}");

        var service = new ConfigurationService(configPath);
        await service.LoadAsync();

        Assert.Equal("https://server", service.ServerAddress);
        Assert.Equal(3000, service.Port);
        Assert.Equal("secret-key", service.ApiKey);
        Assert.True(service.IsConfigured);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }
}
