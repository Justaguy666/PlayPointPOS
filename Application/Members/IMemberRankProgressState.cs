namespace Application.Members;

public interface IMemberRankProgressState
{
    decimal TotalSpentAmount { get; }

    MembershipRank? MembershipRank { get; set; }

    MembershipRank? NextMembershipRank { get; set; }

    int ProgressPercentage { get; set; }
}
