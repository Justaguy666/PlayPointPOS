using Application.Services;

namespace Infrastructure.Services.Auth;

public sealed class AuthStateService : IAuthStateService
{
    public string? ShopId { get; set; }

    public void Clear()
    {
        ShopId = null;
    }
}
