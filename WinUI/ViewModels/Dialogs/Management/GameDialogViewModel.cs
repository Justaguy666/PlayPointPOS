using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Entities;
using Domain.Enums;
using WinUI.Services.Factories;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.UIModels.Management;

namespace WinUI.ViewModels.Dialogs.Management;

public partial class GameDialogViewModel : UpsertDialogViewModelBase
{
    private const string DifficultyEasyValue = "easy";
    private const string DifficultyMediumValue = "medium";
    private const string DifficultyHardValue = "hard";

    private readonly IDialogService _dialogService;
    private readonly GameModelFactory _gameModelFactory;
    private IReadOnlyList<GameType> _availableGameTypes = [];
    private GameModel _targetModel = new();
    private GameModel _initialModel = new();
    private Func<GameModel, Task>? _onSubmittedAsync;
    private bool _isUpdatingSelectionOptions;
    private event Action? CloseRequestedInternal;

    public event Action? DialogHideRequested;

    public event Action? DialogShowRequested;

    public GameDialogViewModel(
        ILocalizationService localizationService,
        IDialogService dialogService,
        GameModelFactory gameModelFactory,
        UpsertDialogMode mode)
        : base(localizationService, mode)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _gameModelFactory = gameModelFactory ?? throw new ArgumentNullException(nameof(gameModelFactory));
        ApplyModel(_initialModel);
    }

    [ObservableProperty]
    public partial string NameLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NamePlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string GameTypeLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DifficultyLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PlayerCountLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MinPlayersPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MaxPlayersPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string HourlyPriceLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string HourlyPricePlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StockQuantityLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StockQuantityPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ResetButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CloseTooltipText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    public partial string GameName { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    public partial string? SelectedGameTypeName { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    public partial string? SelectedDifficultyValue { get; set; } = DifficultyMediumValue;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    public partial string MinPlayersText { get; set; } = "2";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    public partial string MaxPlayersText { get; set; } = "4";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    public partial string HourlyPriceText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    public partial string StockQuantityText { get; set; } = "1";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string ErrorMessage { get; set; } = string.Empty;

    public bool ShowStockQuantity => Mode != UpsertDialogMode.Edit;

    public ObservableCollection<LocalizationOptionModel> GameTypeOptions { get; } = [];

    public ObservableCollection<LocalizationOptionModel> DifficultyOptions { get; } = [];

    protected override string CreateTitleLocKey => "GameDialogCreateTitleText";

    protected override string EditTitleLocKey => "GameDialogEditTitleText";

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool CanSubmit => TryGetParsedFormValues(
        out _,
        out _,
        out _,
        out _,
        out _,
        out _,
        out _);

    public event Action? CloseRequested
    {
        add => CloseRequestedInternal += value;
        remove => CloseRequestedInternal -= value;
    }

    protected override void RefreshLocalizedText()
    {
        base.RefreshLocalizedText();

        NameLabelText = LocalizationService.GetString("GameDialogNameLabelText");
        NamePlaceholderText = LocalizationService.GetString("GameDialogNamePlaceholderText");
        GameTypeLabelText = LocalizationService.GetString("GameDialogGameTypeLabelText");
        DifficultyLabelText = LocalizationService.GetString("GameDialogDifficultyLabelText");
        PlayerCountLabelText = LocalizationService.GetString("GameDialogPlayerCountLabelText");
        MinPlayersPlaceholderText = LocalizationService.GetString("GameDialogMinPlayersPlaceholderText");
        MaxPlayersPlaceholderText = LocalizationService.GetString("GameDialogMaxPlayersPlaceholderText");
        HourlyPriceLabelText = LocalizationService.GetString("GameDialogHourlyPriceLabelText");
        HourlyPricePlaceholderText = LocalizationService.GetString("GameDialogHourlyPricePlaceholderText");
        StockQuantityLabelText = LocalizationService.GetString("GameDialogStockQuantityLabelText");
        StockQuantityPlaceholderText = LocalizationService.GetString("GameDialogStockQuantityPlaceholderText");
        ResetButtonText = LocalizationService.GetString("GameDialogResetButtonText");
        CloseTooltipText = LocalizationService.GetString("CloseTooltipText");
        SubmitButtonText = IsEdit
            ? LocalizationService.GetString("SaveButtonText")
            : LocalizationService.GetString("GameDialogApplyButtonText");

        RefreshSelectionOptions();
    }

    public void Configure(GameDialogRequest? request)
    {
        _availableGameTypes = request?.AvailableGameTypes?.ToList() ?? [];
        _onSubmittedAsync = request?.OnSubmittedAsync;
        _targetModel = request?.Model ?? CreateDefaultModel();
        _initialModel = _gameModelFactory.Clone(_targetModel);
        ErrorMessage = string.Empty;

        RefreshSelectionOptions();
        ApplyModel(_initialModel);
    }

    public override Task SaveAsync() => SubmitAsync();

    [RelayCommand]
    private async Task CloseAsync()
    {
        DialogHideRequested?.Invoke();

        bool isConfirmed = await _dialogService.ShowConfirmationAsync(
            titleKey: "ConfirmCloseTitle",
            messageKey: "ConfirmCloseMessage",
            confirmButtonTextKey: "ConfirmCloseButton",
            cancelButtonTextKey: "CancelButtonText");

        if (isConfirmed)
        {
            CloseRequestedInternal?.Invoke();
            return;
        }

        DialogShowRequested?.Invoke();
    }

    [RelayCommand]
    private void Reset()
    {
        ErrorMessage = string.Empty;
        ApplyModel(_initialModel);
    }

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        if (!TryApplyToTargetModel(out GameModel model))
        {
            return;
        }

        DialogHideRequested?.Invoke();

        bool isConfirmed = await _dialogService.ShowConfirmationAsync(
            titleKey: IsEdit ? "ConfirmEditGameTitle" : "ConfirmAddGameTitle",
            messageKey: IsEdit ? "ConfirmEditGameMessage" : "ConfirmAddGameMessage",
            confirmButtonTextKey: IsEdit ? "ConfirmEditGameButton" : "ConfirmAddGameButton",
            cancelButtonTextKey: "CancelButtonText");

        if (!isConfirmed)
        {
            DialogShowRequested?.Invoke();
            return;
        }

        ErrorMessage = string.Empty;

        try
        {
            if (_onSubmittedAsync is not null)
            {
                await _onSubmittedAsync(model);
            }

            CloseRequestedInternal?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            DialogShowRequested?.Invoke();
        }
    }

    partial void OnGameNameChanged(string value) => NotifyFormStateChanged();

    partial void OnSelectedGameTypeNameChanged(string? value)
    {
        if (!_isUpdatingSelectionOptions)
        {
            NotifyFormStateChanged();
        }
    }

    partial void OnSelectedDifficultyValueChanged(string? value)
    {
        if (!_isUpdatingSelectionOptions)
        {
            NotifyFormStateChanged();
        }
    }

    partial void OnMinPlayersTextChanged(string value) => NotifyFormStateChanged();

    partial void OnMaxPlayersTextChanged(string value) => NotifyFormStateChanged();

    partial void OnHourlyPriceTextChanged(string value) => NotifyFormStateChanged();

    partial void OnStockQuantityTextChanged(string value) => NotifyFormStateChanged();

    private void RefreshSelectionOptions()
    {
        _isUpdatingSelectionOptions = true;

        try
        {
            string currentGameTypeName = SelectedGameTypeName ?? string.Empty;
            string currentDifficultyValue = SelectedDifficultyValue ?? DifficultyMediumValue;

            ReplaceOptions(
                GameTypeOptions,
                _availableGameTypes
                    .Select(gameType => new LocalizationOptionModel
                    {
                        Value = gameType.Name,
                        DisplayName = gameType.Name,
                    })
                    .ToList());

            ReplaceOptions(
                DifficultyOptions,
                [
                    new LocalizationOptionModel { Value = DifficultyEasyValue, DisplayName = LocalizationService.GetString("GameDifficultyEasyText") },
                    new LocalizationOptionModel { Value = DifficultyMediumValue, DisplayName = LocalizationService.GetString("GameDifficultyMediumText") },
                    new LocalizationOptionModel { Value = DifficultyHardValue, DisplayName = LocalizationService.GetString("GameDifficultyHardText") },
                ]);

            if (!GameTypeOptions.Any(option => option.Value == currentGameTypeName))
            {
                currentGameTypeName = GameTypeOptions.FirstOrDefault()?.Value ?? string.Empty;
            }

            if (!DifficultyOptions.Any(option => option.Value == currentDifficultyValue))
            {
                currentDifficultyValue = DifficultyMediumValue;
            }

            SelectedGameTypeName = currentGameTypeName;
            SelectedDifficultyValue = currentDifficultyValue;
        }
        finally
        {
            _isUpdatingSelectionOptions = false;
        }
    }

    private void ApplyModel(GameModel model)
    {
        GameName = model.Name;
        SelectedGameTypeName = ResolveGameTypeName(model.GameType);
        SelectedDifficultyValue = MapDifficultyToValue(model.GameDifficulty);
        MinPlayersText = Math.Max(1, model.MinPlayers).ToString(LocalizationService.Culture);
        MaxPlayersText = Math.Max(Math.Max(1, model.MinPlayers), model.MaxPlayers).ToString(LocalizationService.Culture);
        HourlyPriceText = model.HourlyPrice > 0m
            ? model.HourlyPrice.ToString("0.##", LocalizationService.Culture)
            : string.Empty;
        StockQuantityText = Math.Max(0, model.StockQuantity).ToString(LocalizationService.Culture);
        NotifyFormStateChanged();
    }

    private void NotifyFormStateChanged()
    {
        if (!string.IsNullOrWhiteSpace(ErrorMessage))
        {
            ErrorMessage = string.Empty;
        }

        SubmitCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanSubmit));
    }

    private GameModel CreateDefaultModel()
    {
        return new GameModel
        {
            GameType = _availableGameTypes.FirstOrDefault() ?? new GameType { Name = string.Empty },
            GameDifficulty = GameDifficulty.Medium,
            MinPlayers = 2,
            MaxPlayers = 4,
            StockQuantity = 1,
            BorrowedQuantity = 0,
            ImageUri = "ms-appx:///Assets/Mock.png",
        };
    }

    private string ResolveGameTypeName(GameType? gameType)
    {
        if (!string.IsNullOrWhiteSpace(gameType?.Name))
        {
            return gameType.Name;
        }

        return GameTypeOptions.FirstOrDefault()?.Value ?? string.Empty;
    }

    private bool TryApplyToTargetModel(out GameModel model)
    {
        model = _targetModel;

        if (!TryGetParsedFormValues(
                out string trimmedName,
                out GameType gameType,
                out GameDifficulty difficulty,
                out int minPlayers,
                out int maxPlayers,
                out decimal hourlyPrice,
                out int stockQuantity))
        {
            return false;
        }

        model.Name = trimmedName;
        model.GameType = gameType;
        model.GameDifficulty = difficulty;
        model.MinPlayers = minPlayers;
        model.MaxPlayers = maxPlayers;
        model.HourlyPrice = hourlyPrice;
        model.StockQuantity = stockQuantity;
        model.ImageUri = string.IsNullOrWhiteSpace(model.ImageUri) ? "ms-appx:///Assets/Mock.png" : model.ImageUri;

        return true;
    }

    private bool TryGetParsedFormValues(
        out string trimmedName,
        out GameType gameType,
        out GameDifficulty difficulty,
        out int minPlayers,
        out int maxPlayers,
        out decimal hourlyPrice,
        out int stockQuantity)
    {
        trimmedName = GameName?.Trim() ?? string.Empty;
        gameType = ResolveSelectedGameType();
        difficulty = ResolveSelectedDifficulty();

        if (string.IsNullOrWhiteSpace(trimmedName)
            || string.IsNullOrWhiteSpace(SelectedGameTypeName)
            || string.IsNullOrWhiteSpace(SelectedDifficultyValue))
        {
            minPlayers = 0;
            maxPlayers = 0;
            hourlyPrice = 0m;
            stockQuantity = 0;
            return false;
        }

        if (!TryParseRequiredInt(MinPlayersText, out minPlayers)
            || !TryParseRequiredInt(MaxPlayersText, out maxPlayers)
            || !TryParseRequiredDecimal(HourlyPriceText, out hourlyPrice)
            || !TryParseRequiredInt(StockQuantityText, out stockQuantity))
        {
            minPlayers = 0;
            maxPlayers = 0;
            hourlyPrice = 0m;
            stockQuantity = 0;
            return false;
        }

        (minPlayers, maxPlayers) = NormalizeRange(minPlayers, maxPlayers);

        return minPlayers > 0
            && maxPlayers > 0
            && hourlyPrice > 0m
            && stockQuantity >= 0;
    }

    private GameType ResolveSelectedGameType()
    {
        if (string.IsNullOrWhiteSpace(SelectedGameTypeName))
        {
            return _availableGameTypes.FirstOrDefault() ?? new GameType { Name = string.Empty };
        }

        return _availableGameTypes.FirstOrDefault(gameType =>
                   string.Equals(gameType.Name, SelectedGameTypeName, StringComparison.OrdinalIgnoreCase))
               ?? new GameType { Name = SelectedGameTypeName };
    }

    private GameDifficulty ResolveSelectedDifficulty()
    {
        return SelectedDifficultyValue switch
        {
            DifficultyEasyValue => GameDifficulty.Easy,
            DifficultyHardValue => GameDifficulty.Hard,
            _ => GameDifficulty.Medium,
        };
    }

    private string MapDifficultyToValue(GameDifficulty difficulty)
    {
        return difficulty switch
        {
            GameDifficulty.Easy => DifficultyEasyValue,
            GameDifficulty.Hard => DifficultyHardValue,
            _ => DifficultyMediumValue,
        };
    }

    private bool TryParseRequiredInt(string? text, out int value)
    {
        const NumberStyles styles = NumberStyles.Integer;
        string trimmedText = text?.Trim() ?? string.Empty;

        return (int.TryParse(trimmedText, styles, LocalizationService.Culture, out value)
                || int.TryParse(trimmedText, styles, CultureInfo.InvariantCulture, out value))
               && value >= 0;
    }

    private bool TryParseRequiredDecimal(string? text, out decimal value)
    {
        const NumberStyles styles = NumberStyles.Number;
        string trimmedText = text?.Trim() ?? string.Empty;

        return (decimal.TryParse(trimmedText, styles, LocalizationService.Culture, out value)
                || decimal.TryParse(trimmedText, styles, CultureInfo.InvariantCulture, out value))
               && value >= 0m;
    }

    private static (int Min, int Max) NormalizeRange(int min, int max)
    {
        return min > max ? (max, min) : (min, max);
    }

    private static void ReplaceOptions(
        ObservableCollection<LocalizationOptionModel> collection,
        IReadOnlyList<LocalizationOptionModel> options)
    {
        collection.Clear();
        foreach (LocalizationOptionModel option in options)
        {
            collection.Add(option);
        }
    }
}
