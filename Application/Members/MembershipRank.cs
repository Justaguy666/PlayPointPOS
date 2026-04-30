namespace Application.Members;

public sealed record MembershipRank
{
    public string Id { get; init; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int Priority { get; set; }

    public string Color { get; set; } = string.Empty;

    public decimal DiscountRate { get; set; }

    public decimal MinSpentAmount { get; set; }

    public bool IsDefault { get; set; }
}
