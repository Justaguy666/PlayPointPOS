namespace Application.Services;

public interface IConfigurationService
{
    string ServerAddress { get; }
    int Port { get; }
    bool RememberMe { get; }
    string RememberedEmail { get; }
    ShopProfile ShopProfile { get; }
    bool IsConfigured { get; }

    Task LoadAsync();
    Task SaveAsync(string serverAddress, int port, bool rememberMe);
    Task SaveRememberedLoginAsync(string email, bool rememberMe);
    Task SaveShopProfileAsync(ShopProfile shopProfile);
}
