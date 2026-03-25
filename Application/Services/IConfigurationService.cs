namespace Application.Services;

public interface IConfigurationService
{
    string ServerAddress { get; }
    string ApiKey { get; }
    bool RememberMe { get; }
    bool IsConfigured { get; }

    Task LoadAsync();
    Task SaveAsync(string serverAddress, string apiKey, bool rememberMe);
}
