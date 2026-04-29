namespace Application.Services;

public interface IConfigurationService
{
    string ServerAddress { get; }
    int Port { get; }
    string ApiKey { get; }
    bool RememberMe { get; }
    bool IsConfigured { get; }

    Task LoadAsync();
    Task SaveAsync(string serverAddress, int port, string apiKey, bool rememberMe);
}
