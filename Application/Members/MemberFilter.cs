using Domain.Entities;

namespace Application.Members;

public sealed record MemberFilter
{
    public MembershipRank? MembershipRank { get; init; }

    public decimal? TotalSpentMin { get; init; }

    public decimal? TotalSpentMax { get; init; }
}
