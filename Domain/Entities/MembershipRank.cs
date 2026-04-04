namespace Domain.Entities;

public class MembershipRank : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string Color { get; set; } = string.Empty;
    public decimal DiscountRate { get; set; }
    public decimal MinSpentAmount { get; set; }
    public bool IsDefault { get; set; }
}
