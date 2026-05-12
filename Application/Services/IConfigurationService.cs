namespace Application.Services;

public interface IConfigurationService
{
    string ServerAddress { get; }
    int Port { get; }
    bool RememberMe { get; }
    bool IsConfigured { get; }

    Task LoadAsync();
    Task SaveAsync(string serverAddress, int port, bool rememberMe);
}
