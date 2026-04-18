using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Application.Games;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Entities;
using Domain.Enums;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels;

namespace WinUI.ViewModels.Dialogs.Management;

public partial class GameFilterViewModel : LocalizedViewModelBase
{
    private const string AllOptionValue = "";
    private const string DifficultyEasyValue = "easy";
    private const string DifficultyMediumValue = "medium";
    private const string DifficultyHardValue = "hard";

    private IReadOnlyList<GameType> _availableGameTypes = [];
    private Func<BoardGameFilter, Task>? _onSubmittedAsync;
    private bool _isUpdatingSelectionOptions;
    private event Action? CloseRequestedInternal;

    public GameFilterViewModel(ILocalizationService localizationService)
        : base(localizationService)
    {
        Icon = new IconState
        {
            Kind = IconKind.Filter,
            Size = 24,
            AlwaysFilled = true,
        };

        RefreshLocalizedText();
    }

    [ObservableProperty]
    public partial IconState Icon { get; set; } = new();

    [ObservableProperty]
    public partial string TitleText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string GameTypeLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DifficultyLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PlayerCountLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PlayerCountPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string HourlyPriceRangeLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string HourlyPriceFromPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string HourlyPriceToPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AllOptionsText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ResetButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ApplyButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CloseTooltipText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanApply))]
    public partial string? SelectedGameTypeName { get; set; } = AllOptionValue;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanApply))]
    public partial string? SelectedDifficultyValue { get; set; } = AllOptionValue;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanApply))]
    public partial string PlayerCountText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanApply))]
    public partial string HourlyPriceMinText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanApply))]
    public partial string HourlyPriceMaxText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string ErrorMessage { get; set; } = string.Empty;

    public ObservableCollection<LocalizationOptionModel> GameTypeOptions { get; } = [];

    public ObservableCollection<LocalizationOptionModel> DifficultyOptions { get; } = [];

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool CanApply => HasValidNumericRanges();

    public event Action? CloseRequested
    {
        add => CloseRequestedInternal += value;
        remove => CloseRequestedInternal -= value;
    }

    public void Configure(GameFilterDialogRequest? request)
    {
        _availableGameTypes = request?.AvailableGameTypes?.ToList() ?? [];
        _onSubmittedAsync = request?.OnSubmittedAsync;

        BoardGameFilter criteria = request?.InitialCriteria ?? new BoardGameFilter();
        SelectedGameTypeName = criteria.GameType?.Name ?? AllOptionValue;
        SelectedDifficultyValue = MapDifficultyToValue(criteria.GameDifficulty);
        PlayerCountText = criteria.PlayerCount?.ToString(LocalizationService.Culture) ?? string.Empty;
        HourlyPriceMinText = criteria.HourlyPriceMin?.ToString("0.##", LocalizationService.Culture) ?? string.Empty;
        HourlyPriceMaxText = criteria.HourlyPriceMax?.ToString("0.##", LocalizationService.Culture) ?? string.Empty;
        ErrorMessage = string.Empty;

        RefreshSelectionOptions();
    }

    protected override void RefreshLocalizedText()
    {
        TitleText = LocalizationService.GetString("GameFilterDialogTitleText");
        GameTypeLabelText = LocalizationService.GetString("GameFilterDialogGameTypeLabelText");
        DifficultyLabelText = LocalizationService.GetString("GameFilterDialogDifficultyLabelText");
        PlayerCountLabelText = LocalizationService.GetString("GameFilterDialogPlayerCountLabelText");
        PlayerCountPlaceholderText = LocalizationService.GetString("GameFilterDialogPlayerCountPlaceholderText");
        HourlyPriceRangeLabelText = LocalizationService.GetString("GameFilterDialogHourlyPriceRangeLabelText");
        HourlyPriceFromPlaceholderText = LocalizationService.GetString("GameFilterDialogHourlyPriceFromPlaceholderText");
        HourlyPriceToPlaceholderText = LocalizationService.GetString("GameFilterDialogHourlyPriceToPlaceholderText");
        AllOptionsText = LocalizationService.GetString("GameFilterDialogAllOptionsText");
        ResetButtonText = LocalizationService.GetString("GameFilterDialogResetButtonText");
        ApplyButtonText = LocalizationService.GetString("GameFilterDialogApplyButtonText");
        CloseTooltipText = LocalizationService.GetString("CloseTooltipText");

        RefreshSelectionOptions();
    }

    [RelayCommand(CanExecute = nameof(CanApply))]
    private async Task ApplyAsync()
    {
        if (!TryBuildCriteria(out BoardGameFilter criteria))
        {
            ErrorMessage = LocalizationService.GetString("GameFilterDialogInvalidRangeText");
            return;
        }

        ErrorMessage = string.Empty;

        if (_onSubmittedAsync is not null)
        {
            await _onSubmittedAsync(criteria);
        }

        CloseRequestedInternal?.Invoke();
    }

    [RelayCommand]
    private void Reset()
    {
        SelectedGameTypeName = AllOptionValue;
        SelectedDifficultyValue = AllOptionValue;
        PlayerCountText = string.Empty;
        HourlyPriceMinText = string.Empty;
        HourlyPriceMaxText = string.Empty;
        ErrorMessage = string.Empty;
        NotifyFilterInputChanged();
    }

    [RelayCommand]
    private void Close()
    {
        CloseRequestedInternal?.Invoke();
    }

    partial void OnSelectedGameTypeNameChanged(string? value)
    {
        if (!_isUpdatingSelectionOptions)
        {
            NotifyFilterInputChanged();
        }
    }

    partial void OnSelectedDifficultyValueChanged(string? value)
    {
        if (!_isUpdatingSelectionOptions)
        {
            NotifyFilterInputChanged();
        }
    }

    partial void OnPlayerCountTextChanged(string value) => NotifyFilterInputChanged();

    partial void OnHourlyPriceMinTextChanged(string value) => NotifyFilterInputChanged();

    partial void OnHourlyPriceMaxTextChanged(string value) => NotifyFilterInputChanged();

    private void RefreshSelectionOptions()
    {
        _isUpdatingSelectionOptions = true;

        try
        {
            string currentGameTypeName = SelectedGameTypeName ?? AllOptionValue;
            string currentDifficultyValue = SelectedDifficultyValue ?? AllOptionValue;

            ReplaceOptions(
                GameTypeOptions,
                [
                    new LocalizationOptionModel { Value = AllOptionValue, DisplayName = AllOptionsText },
                    .. _availableGameTypes.Select(gameType => new LocalizationOptionModel
                    {
                        Value = gameType.Name,
                        DisplayName = gameType.Name,
                    }),
                ]);

            ReplaceOptions(
                DifficultyOptions,
                [
                    new LocalizationOptionModel { Value = AllOptionValue, DisplayName = AllOptionsText },
                    new LocalizationOptionModel { Value = DifficultyEasyValue, DisplayName = LocalizationService.GetString("GameDifficultyEasyText") },
                    new LocalizationOptionModel { Value = DifficultyMediumValue, DisplayName = LocalizationService.GetString("GameDifficultyMediumText") },
                    new LocalizationOptionModel { Value = DifficultyHardValue, DisplayName = LocalizationService.GetString("GameDifficultyHardText") },
                ]);

            if (!GameTypeOptions.Any(option => option.Value == currentGameTypeName))
            {
                currentGameTypeName = AllOptionValue;
            }

            if (!DifficultyOptions.Any(option => option.Value == currentDifficultyValue))
            {
                currentDifficultyValue = AllOptionValue;
            }

            SelectedGameTypeName = currentGameTypeName;
            SelectedDifficultyValue = currentDifficultyValue;
        }
        finally
        {
            _isUpdatingSelectionOptions = false;
        }
    }

    private void NotifyFilterInputChanged()
    {
        if (!string.IsNullOrWhiteSpace(ErrorMessage))
        {
            ErrorMessage = string.Empty;
        }

        ApplyCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanApply));
    }

    private bool HasValidNumericRanges()
    {
        return TryParseOptionalPositiveInt(PlayerCountText, out _)
            && TryParseOptionalDecimal(HourlyPriceMinText, out _)
            && TryParseOptionalDecimal(HourlyPriceMaxText, out _);
    }

    private bool TryBuildCriteria(out BoardGameFilter criteria)
    {
        criteria = new BoardGameFilter();

        if (!TryParseOptionalPositiveInt(PlayerCountText, out int? playerCount)
            || !TryParseOptionalDecimal(HourlyPriceMinText, out decimal? hourlyPriceMin)
            || !TryParseOptionalDecimal(HourlyPriceMaxText, out decimal? hourlyPriceMax))
        {
            return false;
        }

        (decimal? normalizedHourlyPriceMin, decimal? normalizedHourlyPriceMax) = NormalizeRange(hourlyPriceMin, hourlyPriceMax);

        criteria = new BoardGameFilter
        {
            GameType = ResolveSelectedGameType(),
            GameDifficulty = ResolveSelectedDifficulty(),
            PlayerCount = playerCount,
            HourlyPriceMin = normalizedHourlyPriceMin,
            HourlyPriceMax = normalizedHourlyPriceMax,
        };

        return true;
    }

    private GameType? ResolveSelectedGameType()
    {
        if (string.IsNullOrWhiteSpace(SelectedGameTypeName))
        {
            return null;
        }

        return _availableGameTypes.FirstOrDefault(gameType =>
            string.Equals(gameType.Name, SelectedGameTypeName, StringComparison.OrdinalIgnoreCase));
    }

    private GameDifficulty? ResolveSelectedDifficulty()
    {
        return SelectedDifficultyValue switch
        {
            DifficultyEasyValue => GameDifficulty.Easy,
            DifficultyMediumValue => GameDifficulty.Medium,
            DifficultyHardValue => GameDifficulty.Hard,
            _ => null,
        };
    }

    private string MapDifficultyToValue(GameDifficulty? difficulty)
    {
        return difficulty switch
        {
            GameDifficulty.Easy => DifficultyEasyValue,
            GameDifficulty.Medium => DifficultyMediumValue,
            GameDifficulty.Hard => DifficultyHardValue,
            _ => AllOptionValue,
        };
    }

    private bool TryParseOptionalPositiveInt(string? text, out int? value)
    {
        const NumberStyles styles = NumberStyles.Integer;
        string trimmedText = text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(trimmedText))
        {
            value = null;
            return true;
        }

        bool success =
            int.TryParse(trimmedText, styles, LocalizationService.Culture, out int parsedValue)
            || int.TryParse(trimmedText, styles, CultureInfo.InvariantCulture, out parsedValue);

        value = success && parsedValue > 0 ? parsedValue : null;
        return success;
    }

    private bool TryParseOptionalDecimal(string? text, out decimal? value)
    {
        const NumberStyles styles = NumberStyles.Number;
        string trimmedText = text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(trimmedText))
        {
            value = null;
            return true;
        }

        bool success =
            decimal.TryParse(trimmedText, styles, LocalizationService.Culture, out decimal parsedValue)
            || decimal.TryParse(trimmedText, styles, CultureInfo.InvariantCulture, out parsedValue);

        value = success ? Math.Max(0m, parsedValue) : null;
        return success;
    }

    private static (T? Min, T? Max) NormalizeRange<T>(T? min, T? max)
        where T : struct, IComparable<T>
    {
        if (min.HasValue && max.HasValue && min.Value.CompareTo(max.Value) > 0)
        {
            return (max, min);
        }

        return (min, max);
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
