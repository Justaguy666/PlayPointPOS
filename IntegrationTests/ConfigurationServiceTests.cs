using Infrastructure.Services;

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
        await service.SaveAsync("https://server", "secret-key", rememberMe: true);

        var remembered = new ConfigurationService(configPath);
        await remembered.LoadAsync();

        Assert.Equal("https://server", remembered.ServerAddress);
        Assert.Equal("secret-key", remembered.ApiKey);
        Assert.True(remembered.RememberMe);
        Assert.True(remembered.IsConfigured);

        await service.SaveAsync("https://server", "secret-key", rememberMe: false);

        var notRemembered = new ConfigurationService(configPath);
        await notRemembered.LoadAsync();

        Assert.Equal("https://server", notRemembered.ServerAddress);
        Assert.Equal(string.Empty, notRemembered.ApiKey);
        Assert.False(notRemembered.RememberMe);
        Assert.False(notRemembered.IsConfigured);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }
}
