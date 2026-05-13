using Application.Areas;
using Application.Services;
using Application.Services.Areas;

namespace Infrastructure.Services.Areas;

public sealed class GraphQLAreaCatalogService : IAreaCatalogService
{
    private readonly IManagementApiService _managementApiService;

    public GraphQLAreaCatalogService(IManagementApiService managementApiService)
    {
        _managementApiService = managementApiService;
    }

    public IReadOnlyList<AreaRecord> GetAreas()
    {
        return Task.Run(async () => await _managementApiService.GetAreasAsync().ConfigureAwait(false))
            .GetAwaiter()
            .GetResult();
    }
}
