using Domain.Entities;

namespace Application.Members;

public static class MemberRankProgressCalculator
{
    public static MemberRankProgressSnapshot Calculate(decimal totalSpentAmount, IEnumerable<MembershipRank> membershipRanks)
    {
        ArgumentNullException.ThrowIfNull(membershipRanks);

        List<MembershipRank> orderedRanks = membershipRanks
            .OrderBy(rank => rank.MinSpentAmount)
            .ThenBy(rank => rank.Priority)
            .ToList();

        if (orderedRanks.Count == 0)
        {
            return new MemberRankProgressSnapshot(null, null, 0);
        }

        decimal normalizedTotalSpent = Math.Max(0m, totalSpentAmount);
        MembershipRank currentRank = orderedRanks
            .Where(rank => normalizedTotalSpent >= rank.MinSpentAmount)
            .LastOrDefault() ?? orderedRanks[0];

        MembershipRank? nextRank = orderedRanks
            .FirstOrDefault(rank => rank.MinSpentAmount > currentRank.MinSpentAmount);

        if (nextRank is null)
        {
            return new MemberRankProgressSnapshot(currentRank, null, 100);
        }

        decimal currentFloor = currentRank.MinSpentAmount;
        decimal nextFloor = nextRank.MinSpentAmount;
        decimal requiredAmount = Math.Max(1m, nextFloor - currentFloor);
        decimal progressedAmount = Math.Clamp(normalizedTotalSpent - currentFloor, 0m, requiredAmount);
        int progressPercentage = (int)Math.Round((progressedAmount / requiredAmount) * 100m, MidpointRounding.AwayFromZero);

        return new MemberRankProgressSnapshot(currentRank, nextRank, Math.Clamp(progressPercentage, 0, 100));
    }
}
