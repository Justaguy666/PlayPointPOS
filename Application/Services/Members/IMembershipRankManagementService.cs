using Application.Members;

namespace Application.Services.Members;

public interface IMembershipRankManagementService
{
    void NormalizeRanks(IList<MembershipRank> ranks);

    MembershipRank AddRank(
        IList<MembershipRank> ranks,
        string name,
        decimal minSpentAmount,
        decimal discountRate,
        string color);

    bool DeleteRank(IList<MembershipRank> ranks, MembershipRank rank);

    bool UpdateRank(
        MembershipRank rank,
        string name,
        decimal minSpentAmount,
        decimal discountRate,
        string color);

    MemberRankProgressSnapshot CalculateProgress(decimal totalSpentAmount, IEnumerable<MembershipRank> ranks);

    void ApplyMembershipState(IMemberRankProgressState member, IReadOnlyList<MembershipRank> ranks);

    void RefreshMembershipStates(IEnumerable<IMemberRankProgressState> members, IReadOnlyList<MembershipRank> ranks);

    string ResolveRankColor(string? rankName, int index);
}
