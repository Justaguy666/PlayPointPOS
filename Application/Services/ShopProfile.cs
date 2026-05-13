namespace Application.Services;

public sealed record ShopProfile
{
    public string ShopName { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
}
