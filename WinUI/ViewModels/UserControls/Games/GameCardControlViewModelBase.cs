using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Enums;
using Microsoft.UI.Xaml.Media;
using WinUI.Resources;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.UIModels.Management;
using WinUI.ViewModels;

namespace WinUI.ViewModels.UserControls.Games;

public abstract partial class GameCardControlViewModelBase : LocalizedViewModelBase
{
    private readonly Func<GameModel, Task>? _editAction;
    private readonly Func<GameModel, Task>? _deleteAction;
    private readonly Action<GameModel>? _increaseStockAction;
    private readonly Action<GameModel>? _decreaseStockAction;
    private readonly Brush _typeBadgeBackgroundBrush;
    private readonly Brush _typeBadgeForegroundBrush;
    private readonly Brush _easyDifficultyBackgroundBrush;
    private readonly Brush _mediumDifficultyBackgroundBrush;
    private readonly Brush _hardDifficultyBackgroundBrush;
    private readonly Brush _difficultyForegroundBrush;
    private bool _isDisposed;

    protected GameCardControlViewModelBase(
        ILocalizationService localizationService,
        GameModel model,
        Func<GameModel, Task>? editAction,
        Func<GameModel, Task>? deleteAction,
        Action<GameModel>? increaseStockAction,
        Action<GameModel>? decreaseStockAction)
        : base(localizationService)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
        _editAction = editAction;
        _deleteAction = deleteAction;
        _increaseStockAction = increaseStockAction;
        _decreaseStockAction = decreaseStockAction;

        _typeBadgeBackgroundBrush = AppResourceLookup.GetBrush("InfoBlueBrush", AppColors.InfoBlue);
        _typeBadgeForegroundBrush = AppResourceLookup.GetBrush("WhiteBrush", AppColors.White);
        _easyDifficultyBackgroundBrush = AppResourceLookup.GetBrush("SuccessGreenBrush", AppColors.SuccessGreen);
        _mediumDifficultyBackgroundBrush = AppResourceLookup.GetBrush("WarningAmberBrush", AppColors.WarningAmber);
        _hardDifficultyBackgroundBrush = AppResourceLookup.GetBrush("DangerButtonBackgroundBrush", AppColors.DangerButtonBackground);
        _difficultyForegroundBrush = AppResourceLookup.GetBrush("WhiteBrush", AppColors.White);

        EditCommand = new AsyncRelayCommand(ExecuteEditAsync);
        DeleteCommand = new AsyncRelayCommand(ExecuteDeleteAsync);
        IncreaseStockCommand = new RelayCommand(ExecuteIncreaseStock);
        DecreaseStockCommand = new RelayCommand(ExecuteDecreaseStock, () => CanDecreaseStock);

        Model.PropertyChanged += HandleModelPropertyChanged;
        RefreshLocalizedText();
    }

    public GameModel Model { get; }

    [ObservableProperty]
    public partial string HourlyPriceText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DifficultyDisplayText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PlayerCountLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PlayersSuffixText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DifficultyLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StockLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string HourlyPriceLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial Brush DifficultyBadgeBackground { get; set; } = new SolidColorBrush(AppColors.WarningAmber);

    public string Name => Model.Name;

    public string ImageUri => Model.ImageUri;

    public string TypeDisplayName => Model.GameType?.Name ?? string.Empty;

    public string PlayerRangeText => $"{Model.MinPlayers}-{Model.MaxPlayers}";

    public string StockSummaryText => $"{Model.BorrowedQuantity}/{Model.StockQuantity}";

    public bool CanDecreaseStock => Model.StockQuantity > 0;

    public Brush TypeBadgeBackground => _typeBadgeBackgroundBrush;

    public Brush TypeBadgeForeground => _typeBadgeForegroundBrush;

    public Brush DifficultyBadgeForeground => _difficultyForegroundBrush;

    public IconState PlayersIconState { get; } = new()
    {
        Kind = IconKind.Member,
        Size = 20,
        AlwaysFilled = true,
    };

    public IconState EditIconState { get; } = new()
    {
        Kind = IconKind.Update,
        Size = 20,
        AlwaysFilled = true,
    };

    public IconState DeleteIconState { get; } = new()
    {
        Kind = IconKind.Delete,
        Size = 20,
        AlwaysFilled = true,
    };

    public IAsyncRelayCommand EditCommand { get; }

    public IAsyncRelayCommand DeleteCommand { get; }

    public IRelayCommand IncreaseStockCommand { get; }

    public IRelayCommand DecreaseStockCommand { get; }

    protected override void RefreshLocalizedText()
    {
        HourlyPriceText = LocalizationService.FormatCurrency(Model.HourlyPrice);
        PlayerCountLabelText = LocalizationService.GetString("ListGameCardControlPlayersLabel");
        PlayersSuffixText = LocalizationService.GetString("GridGameCardControlPlayersSuffix");
        DifficultyLabelText = LocalizationService.GetString("GridGameCardControlDifficultyLabel");
        StockLabelText = LocalizationService.GetString("GridGameCardControlStockLabel");
        HourlyPriceLabelText = LocalizationService.GetString("GridGameCardControlPriceLabel");
        DifficultyDisplayText = Model.GameDifficulty switch
        {
            GameDifficulty.Easy => LocalizationService.GetString("GameDifficultyEasyText"),
            GameDifficulty.Hard => LocalizationService.GetString("GameDifficultyHardText"),
            _ => LocalizationService.GetString("GameDifficultyMediumText"),
        };
        DifficultyBadgeBackground = Model.GameDifficulty switch
        {
            GameDifficulty.Easy => _easyDifficultyBackgroundBrush,
            GameDifficulty.Hard => _hardDifficultyBackgroundBrush,
            _ => _mediumDifficultyBackgroundBrush,
        };
        NotifyModelPresentationChanged();
    }

    public new void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        Model.PropertyChanged -= HandleModelPropertyChanged;
        _isDisposed = true;
        base.Dispose();
    }

    private void HandleModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(GameModel.Name):
                OnPropertyChanged(nameof(Name));
                break;
            case nameof(GameModel.ImageUri):
                OnPropertyChanged(nameof(ImageUri));
                break;
            case nameof(GameModel.GameType):
                OnPropertyChanged(nameof(TypeDisplayName));
                break;
            case nameof(GameModel.MinPlayers):
            case nameof(GameModel.MaxPlayers):
                OnPropertyChanged(nameof(PlayerRangeText));
                break;
            case nameof(GameModel.StockQuantity):
            case nameof(GameModel.BorrowedQuantity):
                OnPropertyChanged(nameof(StockSummaryText));
                OnPropertyChanged(nameof(CanDecreaseStock));
                DecreaseStockCommand.NotifyCanExecuteChanged();
                break;
            case nameof(GameModel.HourlyPrice):
            case nameof(GameModel.GameDifficulty):
                RefreshLocalizedText();
                break;
        }
    }

    private Task ExecuteEditAsync()
    {
        return _editAction?.Invoke(Model) ?? Task.CompletedTask;
    }

    private Task ExecuteDeleteAsync()
    {
        return _deleteAction?.Invoke(Model) ?? Task.CompletedTask;
    }

    private void ExecuteIncreaseStock()
    {
        _increaseStockAction?.Invoke(Model);
    }

    private void ExecuteDecreaseStock()
    {
        _decreaseStockAction?.Invoke(Model);
    }

    private void NotifyModelPresentationChanged()
    {
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(ImageUri));
        OnPropertyChanged(nameof(TypeDisplayName));
        OnPropertyChanged(nameof(PlayerRangeText));
        OnPropertyChanged(nameof(StockSummaryText));
        OnPropertyChanged(nameof(CanDecreaseStock));
        DecreaseStockCommand.NotifyCanExecuteChanged();
    }
}
