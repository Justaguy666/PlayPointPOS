namespace Application.UseCases.Auth.Contracts;

public sealed record AccountSummary
{
    public string Id { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string ShopName { get; init; } = string.Empty;
}
