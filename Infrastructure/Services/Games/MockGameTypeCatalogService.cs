using Application.Services.Games;
using Domain.Entities;
using System.Collections.Generic;

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
