using Application.Games;
using Application.Services.Games;
using Domain.Enums;

namespace Infrastructure.Services.Games;

public sealed class MockGameCatalogService : IGameCatalogService
{
    private readonly IGameTypeCatalogService _gameTypeCatalogService;

    public MockGameCatalogService(IGameTypeCatalogService gameTypeCatalogService)
    {
        _gameTypeCatalogService = gameTypeCatalogService;
    }

    public IReadOnlyList<GameRecord> GetGames()
    {
        IReadOnlyList<GameType> gameTypes = _gameTypeCatalogService.GetGameTypes();

        GameType strategyType = gameTypes.First(type => type.Name == "Strategy");
        GameType familyType = gameTypes.First(type => type.Name == "Family");
        GameType cooperativeType = gameTypes.First(type => type.Name == "Cooperative");
        GameType classicType = gameTypes.First(type => type.Name == "Classic");
        GameType partyType = gameTypes.First(type => type.Name == "Party");

        return
        [
            new GameRecord { Name = "7 Wonders", HourlyPrice = 50000m, MinPlayers = 2, MaxPlayers = 7, Type = strategyType, Difficulty = GameDifficulty.Medium, StockQuantity = 3 },
            new GameRecord { Name = "Azul", HourlyPrice = 45000m, MinPlayers = 2, MaxPlayers = 4, Type = familyType, Difficulty = GameDifficulty.Medium, StockQuantity = 4 },
            new GameRecord { Name = "Catan", HourlyPrice = 50000m, MinPlayers = 3, MaxPlayers = 4, Type = strategyType, Difficulty = GameDifficulty.Medium, StockQuantity = 5 },
            new GameRecord { Name = "Codenames", HourlyPrice = 35000m, MinPlayers = 2, MaxPlayers = 8, Type = partyType, Difficulty = GameDifficulty.Easy, StockQuantity = 6 },
            new GameRecord { Name = "Dixit", HourlyPrice = 40000m, MinPlayers = 3, MaxPlayers = 6, Type = familyType, Difficulty = GameDifficulty.Easy, StockQuantity = 5 },
            new GameRecord { Name = "Exploding Kittens", HourlyPrice = 30000m, MinPlayers = 2, MaxPlayers = 5, Type = partyType, Difficulty = GameDifficulty.Easy, StockQuantity = 8 },
            new GameRecord { Name = "Monopoly", HourlyPrice = 30000m, MinPlayers = 2, MaxPlayers = 8, Type = classicType, Difficulty = GameDifficulty.Easy, StockQuantity = 10 },
            new GameRecord { Name = "Pandemic", HourlyPrice = 45000m, MinPlayers = 2, MaxPlayers = 4, Type = cooperativeType, Difficulty = GameDifficulty.Medium, StockQuantity = 4 },
            new GameRecord { Name = "Splendor", HourlyPrice = 40000m, MinPlayers = 2, MaxPlayers = 4, Type = strategyType, Difficulty = GameDifficulty.Medium, StockQuantity = 6 },
            new GameRecord { Name = "Ticket to Ride", HourlyPrice = 40000m, MinPlayers = 2, MaxPlayers = 5, Type = familyType, Difficulty = GameDifficulty.Easy, StockQuantity = 3 },
            new GameRecord { Name = "Risk", HourlyPrice = 35000m, MinPlayers = 2, MaxPlayers = 6, Type = classicType, Difficulty = GameDifficulty.Easy, StockQuantity = 8 },
            new GameRecord { Name = "Scythe", HourlyPrice = 60000m, MinPlayers = 1, MaxPlayers = 5, Type = strategyType, Difficulty = GameDifficulty.Hard, StockQuantity = 2 },
            new GameRecord { Name = "Terraforming Mars", HourlyPrice = 65000m, MinPlayers = 1, MaxPlayers = 5, Type = strategyType, Difficulty = GameDifficulty.Hard, StockQuantity = 3 },
            new GameRecord { Name = "Brass: Birmingham", HourlyPrice = 70000m, MinPlayers = 2, MaxPlayers = 4, Type = strategyType, Difficulty = GameDifficulty.Hard, StockQuantity = 2 },
            new GameRecord { Name = "Cascadia", HourlyPrice = 42000m, MinPlayers = 1, MaxPlayers = 4, Type = familyType, Difficulty = GameDifficulty.Easy, StockQuantity = 5 },
        ];
    }
}
