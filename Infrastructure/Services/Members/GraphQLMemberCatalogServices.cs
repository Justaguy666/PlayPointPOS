using Application.Members;
using Application.Services;
using Application.Services.Members;
using Domain.Entities;

namespace Infrastructure.Services.Members;

public sealed class GraphQLMemberCatalogService : IMemberCatalogService
{
    private readonly IManagementApiService _managementApiService;

    public GraphQLMemberCatalogService(IManagementApiService managementApiService)
    {
        _managementApiService = managementApiService;
    }

    public IReadOnlyList<MemberRecord> GetMembers()
    {
        return Task.Run(async () => await _managementApiService.GetMembersAsync().ConfigureAwait(false))
            .GetAwaiter()
            .GetResult();
    }
}

public sealed class GraphQLMembershipRankCatalogService : IMembershipRankCatalogService
{
    private readonly IManagementApiService _managementApiService;

    public GraphQLMembershipRankCatalogService(IManagementApiService managementApiService)
    {
        _managementApiService = managementApiService;
    }

    public IReadOnlyList<MembershipRank> GetMembershipRanks()
    {
        return Task.Run(async () => await _managementApiService.GetMembershipRanksAsync().ConfigureAwait(false))
            .GetAwaiter()
            .GetResult();
    }
}
