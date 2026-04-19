using Domain.Entities;

namespace Application.Members;

public sealed record MemberRankProgressSnapshot(
    MembershipRank? CurrentRank,
    MembershipRank? NextRank,
    int ProgressPercentage);
