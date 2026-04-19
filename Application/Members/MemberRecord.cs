using Domain.Entities;

namespace Application.Members;

public sealed record MemberRecord
{
    public string Code { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public decimal TotalSpentAmount { get; set; }

    public required MembershipRank CurrentRank { get; set; }
}
