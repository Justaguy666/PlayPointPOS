using System.Collections.ObjectModel;
using System.Linq;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinUI.UIModels;
using WinUI.ViewModels;

namespace WinUI.ViewModels.Pages;

public partial class SettingsPageViewModel : LocalizedViewModelBase
{
    [ObservableProperty]
    public partial string TitleDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DescriptionDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LanguageLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CurrencyLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TimeZoneLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    public partial string Language { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    public partial string Currency { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    public partial string TimeZone { get; set; }

    [ObservableProperty]
    public partial string ApplyButtonDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<LocalizationOptionModel> LanguageOptions { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<LocalizationOptionModel> CurrencyOptions { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<LocalizationOptionModel> TimeZoneOptions { get; set; } = [];

    public SettingsPageViewModel(ILocalizationService localizationService)
        : base(localizationService)
    {
        Language = LocalizationService.Language;
        Currency = LocalizationService.Currency;
        TimeZone = LocalizationService.TimeZone;
        RefreshLocalizedText();
    }

    protected override void RefreshLocalizedText()
    {
        TitleDisplay = LocalizationService.GetString("SettingsPageLocalizationTitle");
        DescriptionDisplay = LocalizationService.GetString("SettingsPageLocalizationDescription");
        LanguageLabelDisplay = LocalizationService.GetString("SettingsPageLanguageLabel");
        CurrencyLabelDisplay = LocalizationService.GetString("SettingsPageCurrencyLabel");
        TimeZoneLabelDisplay = LocalizationService.GetString("SettingsPageTimeZoneLabel");
        ApplyButtonDisplay = LocalizationService.GetString("SettingsPageApplyButton");

        RefreshOptions();
    }

    [RelayCommand(CanExecute = nameof(CanApply))]
    private void Apply()
    {
        LocalizationService.ApplyPreferences(Language, Currency, TimeZone);
    }

    private void RefreshOptions()
    {
        string selectedLanguage = Language;
        string selectedCurrency = Currency;
        string selectedTimeZone = TimeZone;

        LanguageOptions =
        [
            new LocalizationOptionModel { Value = "en-US", DisplayName = LocalizationService.GetString("LocalizationLanguageEnglishOptionText") },
            new LocalizationOptionModel { Value = "vi-VN", DisplayName = LocalizationService.GetString("LocalizationLanguageVietnameseOptionText") },
        ];

        CurrencyOptions =
        [
            new LocalizationOptionModel { Value = "VND", DisplayName = LocalizationService.GetString("LocalizationCurrencyVndOptionText") },
            new LocalizationOptionModel { Value = "USD", DisplayName = LocalizationService.GetString("LocalizationCurrencyUsdOptionText") },
        ];

        TimeZoneOptions =
        [
            new LocalizationOptionModel { Value = "-5", DisplayName = LocalizationService.GetString("LocalizationTimezoneMinus5OptionText") },
            new LocalizationOptionModel { Value = "+0", DisplayName = LocalizationService.GetString("LocalizationTimezoneUtcOptionText") },
            new LocalizationOptionModel { Value = "+7", DisplayName = LocalizationService.GetString("LocalizationTimezonePlus7OptionText") },
            new LocalizationOptionModel { Value = "+9", DisplayName = LocalizationService.GetString("LocalizationTimezonePlus9OptionText") },
        ];

        Language = ResolveSelection(LanguageOptions, selectedLanguage, LocalizationService.Language);
        Currency = ResolveSelection(CurrencyOptions, selectedCurrency, LocalizationService.Currency);
        TimeZone = ResolveSelection(TimeZoneOptions, selectedTimeZone, LocalizationService.TimeZone);
    }

    private bool CanApply() =>
        !string.IsNullOrWhiteSpace(Language) &&
        !string.IsNullOrWhiteSpace(Currency) &&
        !string.IsNullOrWhiteSpace(TimeZone);

    private static string ResolveSelection(
        ObservableCollection<LocalizationOptionModel> options,
        string currentValue,
        string fallbackValue)
    {
        if (options.Any(option => option.Value == currentValue))
            return currentValue;

        if (options.Any(option => option.Value == fallbackValue))
            return fallbackValue;

        return options.First().Value;
    }
}
