using Domain.Entities;

namespace Application.Services.Members;

public interface IMembershipRankCatalogService
{
    IReadOnlyList<MembershipRank> GetMembershipRanks();
}
