using System.Collections.Generic;
using System.Linq;
using Application.Games;
using Domain.Enums;
using WinUI.UIModels.Management;

namespace WinUI.Services.Factories;

public sealed class GameDraftFactory
{
    public GameModel Create(IReadOnlyList<GameType> availableGameTypes)
    {
        return new GameModel
        {
            Name = string.Empty,
            GameType = availableGameTypes.FirstOrDefault() ?? new GameType { Name = string.Empty },
            GameDifficulty = GameDifficulty.Medium,
            MinPlayers = 2,
            MaxPlayers = 4,
            HourlyPrice = 0m,
            StockQuantity = 1,
            BorrowedQuantity = 0,
            ImageUri = "ms-appx:///Assets/Mock.png",
        };
    }
}
