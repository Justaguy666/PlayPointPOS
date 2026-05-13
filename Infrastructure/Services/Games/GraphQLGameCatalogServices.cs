using Application.Games;
using Application.Services;
using Application.Services.Games;
using Domain.Entities;

namespace Infrastructure.Services.Games;

public sealed class GraphQLGameCatalogService : IGameCatalogService
{
    private readonly IManagementApiService _managementApiService;

    public GraphQLGameCatalogService(IManagementApiService managementApiService)
    {
        _managementApiService = managementApiService;
    }

    public IReadOnlyList<GameRecord> GetGames()
    {
        return Task.Run(async () => await _managementApiService.GetGamesAsync().ConfigureAwait(false))
            .GetAwaiter()
            .GetResult();
    }
}

public sealed class GraphQLGameTypeCatalogService : IGameTypeCatalogService
{
    private readonly IManagementApiService _managementApiService;

    public GraphQLGameTypeCatalogService(IManagementApiService managementApiService)
    {
        _managementApiService = managementApiService;
    }

    public IReadOnlyList<GameType> GetGameTypes()
    {
        return Task.Run(async () => await _managementApiService.GetGameTypesAsync().ConfigureAwait(false))
            .GetAwaiter()
            .GetResult();
    }
}
