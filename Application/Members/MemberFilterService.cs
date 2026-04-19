using Application.Services.Members;

namespace Application.Members;

public sealed class MemberFilterService : IMemberFilterService
{
    public IReadOnlyList<TMember> Apply<TMember>(IEnumerable<TMember> members, MemberFilter filter)
        where TMember : IMemberFilterable
    {
        ArgumentNullException.ThrowIfNull(members);
        ArgumentNullException.ThrowIfNull(filter);

        return members
            .Where(member => MatchesMembershipRank(member, filter) && MatchesTotalSpent(member, filter))
            .ToList();
    }

    private static bool MatchesMembershipRank(IMemberFilterable member, MemberFilter filter)
    {
        return filter.MembershipRank is null
            || string.Equals(
                member.MembershipRank?.Name,
                filter.MembershipRank.Name,
                StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesTotalSpent(IMemberFilterable member, MemberFilter filter)
    {
        return (filter.TotalSpentMin is null || member.TotalSpentAmount >= filter.TotalSpentMin.Value)
            && (filter.TotalSpentMax is null || member.TotalSpentAmount <= filter.TotalSpentMax.Value);
    }
}
