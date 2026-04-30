using Application.Games;
using Application.Services.Games;

namespace Infrastructure.Services.Games;

public sealed class MockGameTypeCatalogService : IGameTypeCatalogService
{
    public IReadOnlyList<GameType> GetGameTypes()
    {
        return
        [
            new GameType { Name = "Strategy" },
            new GameType { Name = "Family" },
            new GameType { Name = "Cooperative" },
            new GameType { Name = "Classic" },
            new GameType { Name = "Party" }
        ];
    }
}
