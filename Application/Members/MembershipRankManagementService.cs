using Application.Services.Members;

namespace Application.Members;

public sealed class MembershipRankManagementService : IMembershipRankManagementService
{
    private const string DefaultRankColor = "#F09A44";

    public void NormalizeRanks(IList<MembershipRank> ranks)
    {
        ArgumentNullException.ThrowIfNull(ranks);

        List<MembershipRank> orderedRanks = ranks
            .OrderBy(rank => rank.MinSpentAmount)
            .ThenBy(rank => rank.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        ranks.Clear();
        for (int index = 0; index < orderedRanks.Count; index++)
        {
            MembershipRank rank = orderedRanks[index];
            rank.Priority = index + 1;
            rank.IsDefault = index == 0;
            rank.Color = string.IsNullOrWhiteSpace(rank.Color)
                ? ResolveRankColor(rank.Name, index)
                : NormalizeHexColor(rank.Color);

            ranks.Add(rank);
        }
    }

    public MembershipRank AddRank(
        IList<MembershipRank> ranks,
        string name,
        decimal minSpentAmount,
        decimal discountRate,
        string color)
    {
        ArgumentNullException.ThrowIfNull(ranks);

        string trimmedName = NormalizeName(name);
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            throw new ArgumentException("Membership rank name is required.", nameof(name));
        }

        var rank = new MembershipRank
        {
            Name = trimmedName,
            MinSpentAmount = Math.Max(0m, minSpentAmount),
            DiscountRate = Math.Max(0m, discountRate),
            Color = NormalizeHexColor(color),
        };

        ranks.Add(rank);
        NormalizeRanks(ranks);
        return rank;
    }

    public bool DeleteRank(IList<MembershipRank> ranks, MembershipRank rank)
    {
        ArgumentNullException.ThrowIfNull(ranks);
        ArgumentNullException.ThrowIfNull(rank);

        MembershipRank? target = ranks.FirstOrDefault(candidate => IsSameRank(candidate, rank));
        if (target is null || !ranks.Remove(target))
        {
            return false;
        }

        NormalizeRanks(ranks);
        return true;
    }

    public bool UpdateRank(
        MembershipRank rank,
        string name,
        decimal minSpentAmount,
        decimal discountRate,
        string color)
    {
        ArgumentNullException.ThrowIfNull(rank);

        string trimmedName = NormalizeName(name);
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            return false;
        }

        rank.Name = trimmedName;
        rank.MinSpentAmount = Math.Max(0m, minSpentAmount);
        rank.DiscountRate = Math.Max(0m, discountRate);
        rank.Color = NormalizeHexColor(color);
        return true;
    }

    public MemberRankProgressSnapshot CalculateProgress(decimal totalSpentAmount, IEnumerable<MembershipRank> ranks)
    {
        return MemberRankProgressCalculator.Calculate(totalSpentAmount, ranks);
    }

    public void ApplyMembershipState(IMemberRankProgressState member, IReadOnlyList<MembershipRank> ranks)
    {
        ArgumentNullException.ThrowIfNull(member);
        ArgumentNullException.ThrowIfNull(ranks);

        MemberRankProgressSnapshot snapshot = CalculateProgress(member.TotalSpentAmount, ranks);
        member.MembershipRank = null;
        member.MembershipRank = snapshot.CurrentRank;
        member.NextMembershipRank = null;
        member.NextMembershipRank = snapshot.NextRank;
        member.ProgressPercentage = snapshot.ProgressPercentage;
    }

    public void RefreshMembershipStates(IEnumerable<IMemberRankProgressState> members, IReadOnlyList<MembershipRank> ranks)
    {
        ArgumentNullException.ThrowIfNull(members);

        foreach (IMemberRankProgressState member in members)
        {
            ApplyMembershipState(member, ranks);
        }
    }

    public string ResolveRankColor(string? rankName, int index)
    {
        string normalizedRankName = NormalizeName(rankName);

        if (normalizedRankName.Equals("Diamond", StringComparison.OrdinalIgnoreCase))
        {
            return "#9B8BFF";
        }

        if (normalizedRankName.Equals("Platinum", StringComparison.OrdinalIgnoreCase))
        {
            return "#72C5E4";
        }

        if (normalizedRankName.Equals("Gold", StringComparison.OrdinalIgnoreCase))
        {
            return "#F9CC45";
        }

        if (normalizedRankName.Equals("Silver", StringComparison.OrdinalIgnoreCase))
        {
            return "#CBD3DF";
        }

        if (normalizedRankName.Equals("Bronze", StringComparison.OrdinalIgnoreCase))
        {
            return DefaultRankColor;
        }

        string[] palette =
        [
            DefaultRankColor,
            "#CBD3DF",
            "#F9CC45",
            "#72C5E4",
            "#9B8BFF",
        ];

        return palette[Math.Abs(index) % palette.Length];
    }

    private static bool IsSameRank(MembershipRank left, MembershipRank right)
    {
        return ReferenceEquals(left, right)
            || (!string.IsNullOrWhiteSpace(left.Id)
                && string.Equals(left.Id, right.Id, StringComparison.Ordinal))
            || string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeName(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    private static string NormalizeHexColor(string? color)
    {
        string value = NormalizeName(color);
        if (string.IsNullOrWhiteSpace(value))
        {
            return DefaultRankColor;
        }

        return value.StartsWith('#') ? value : $"#{value}";
    }
}
