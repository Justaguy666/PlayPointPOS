using System;
using Application.Games;
using WinUI.UIModels.Management;

namespace WinUI.Services.Factories;

public sealed class GameModelFactory
{
    public GameModel Create(GameRecord source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new GameModel
        {
            Name = source.Name,
            HourlyPrice = source.HourlyPrice,
            MinPlayers = source.MinPlayers,
            MaxPlayers = source.MaxPlayers,
            GameType = source.Type,
            GameDifficulty = source.Difficulty,
            StockQuantity = source.StockQuantity,
            BorrowedQuantity = 0,
            ImageUri = "ms-appx:///Assets/Mock.png",
        };
    }

    public GameModel Clone(GameModel source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new GameModel
        {
            Name = source.Name,
            HourlyPrice = source.HourlyPrice,
            MinPlayers = source.MinPlayers,
            MaxPlayers = source.MaxPlayers,
            GameType = source.GameType,
            GameDifficulty = source.GameDifficulty,
            StockQuantity = source.StockQuantity,
            BorrowedQuantity = source.BorrowedQuantity,
            ImageUri = source.ImageUri,
        };
    }
}
