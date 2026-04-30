using Application.Members;

namespace Application.Services.Members;

public interface IMembershipRankCatalogService
{
    IReadOnlyList<MembershipRank> GetMembershipRanks();
}
