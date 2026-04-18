using Application.Games;
using CommunityToolkit.Mvvm.ComponentModel;
using Domain.Entities;
using Domain.Enums;
using System;

namespace WinUI.UIModels.Management;

public sealed class GameModel : ObservableObject, IGameFilterable
{
    private string _name = string.Empty;
    private decimal _hourlyPrice;
    private int _minPlayers;
    private int _maxPlayers;
    private GameType _gameType = null!;
    private GameDifficulty _gameDifficulty;
    private int _stockQuantity;
    private int _borrowedQuantity;
    private string _imageUri = "ms-appx:///Assets/Mock.png";

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public decimal HourlyPrice
    {
        get => _hourlyPrice;
        set => SetProperty(ref _hourlyPrice, value);
    }

    public int MinPlayers
    {
        get => _minPlayers;
        set => SetProperty(ref _minPlayers, value);
    }

    public int MaxPlayers
    {
        get => _maxPlayers;
        set => SetProperty(ref _maxPlayers, value);
    }

    public GameType GameType
    {
        get => _gameType;
        set => SetProperty(ref _gameType, value);
    }

    public GameDifficulty GameDifficulty
    {
        get => _gameDifficulty;
        set => SetProperty(ref _gameDifficulty, value);
    }

    public int StockQuantity
    {
        get => _stockQuantity;
        set
        {
            int normalizedValue = Math.Max(value, 0);
            if (!SetProperty(ref _stockQuantity, normalizedValue))
            {
                return;
            }

            if (_borrowedQuantity > normalizedValue)
            {
                _borrowedQuantity = normalizedValue;
                OnPropertyChanged(nameof(BorrowedQuantity));
            }
        }
    }

    public int BorrowedQuantity
    {
        get => _borrowedQuantity;
        set => SetProperty(ref _borrowedQuantity, Math.Clamp(value, 0, StockQuantity));
    }

    public string ImageUri
    {
        get => _imageUri;
        set => SetProperty(ref _imageUri, string.IsNullOrWhiteSpace(value) ? "ms-appx:///Assets/Mock.png" : value);
    }
}
