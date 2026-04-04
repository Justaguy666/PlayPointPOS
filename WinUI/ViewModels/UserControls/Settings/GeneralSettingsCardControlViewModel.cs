using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinUI.UIModels;
using WinUI.ViewModels;

namespace WinUI.ViewModels.UserControls.Settings;

public partial class GeneralSettingsCardControlViewModel : LocalizedViewModelBase
{
    private const string DefaultDateFormat = LocalizationPreferences.DefaultDateFormat;

    private readonly ILocalizationPreferencesService _preferencesService;
    private readonly INotificationService _notificationService;
    private string _appliedDateFormat = DefaultDateFormat;
    private bool _isRefreshingOptions;
    private bool _isApplyingPreferences;
    private bool _isImmediateApplyQueued;

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CurrencyLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TimeZoneLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LanguageLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DateFormatLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ApplyButtonDisplay { get; set; } = string.Empty;

    public ObservableCollection<LocalizationOptionModel> AvailableCurrencies { get; } = [];

    public ObservableCollection<LocalizationOptionModel> AvailableTimeZones { get; } = [];

    public ObservableCollection<LocalizationOptionModel> AvailableLanguages { get; } = [];

    public ObservableCollection<LocalizationOptionModel> AvailableDateFormats { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    public partial string? SelectedCurrencyValue { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    public partial string? SelectedTimeZoneValue { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    public partial string? SelectedLanguageValue { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    public partial string? SelectedDateFormatValue { get; set; }

    public GeneralSettingsCardControlViewModel(
        ILocalizationService localizationService,
        ILocalizationPreferencesService preferencesService,
        INotificationService notificationService)
        : base(localizationService)
    {
        _preferencesService = preferencesService ?? throw new ArgumentNullException(nameof(preferencesService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _appliedDateFormat = _preferencesService.Preferences.DateFormat;
        RefreshLocalizedText();
    }

    protected override void RefreshLocalizedText()
    {
        Title = LocalizationService.GetString("SettingsPageGeneralTitle");
        CurrencyLabel = LocalizationService.GetString("SettingsPageCurrencyLabel");
        TimeZoneLabel = LocalizationService.GetString("SettingsPageTimeZoneLabel");
        LanguageLabel = LocalizationService.GetString("SettingsPageLanguageLabel");
        DateFormatLabel = LocalizationService.GetString("SettingsPageDateFormatLabel");
        ApplyButtonDisplay = LocalizationService.GetString("SettingsPageApplyButton");

        RefreshOptions();
        ApplyCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanApply))]
    private async Task ApplyAsync()
        => await ApplyPreferencesAsync(showNotification: true);

    partial void OnSelectedCurrencyValueChanged(string? value)
        => TriggerImmediateApply();

    partial void OnSelectedTimeZoneValueChanged(string? value)
        => TriggerImmediateApply();

    partial void OnSelectedLanguageValueChanged(string? value)
        => TriggerImmediateApply();

    partial void OnSelectedDateFormatValueChanged(string? value)
        => TriggerImmediateApply();

    private async Task ApplyPreferencesAsync(bool showNotification)
    {
        if (!CanApply())
        {
            return;
        }

        _isApplyingPreferences = true;

        string language = SelectedLanguageValue ?? LocalizationService.Language;
        string currency = SelectedCurrencyValue ?? LocalizationService.Currency;
        string timeZone = SelectedTimeZoneValue ?? LocalizationService.TimeZone;
        string dateFormat = SelectedDateFormatValue ?? _appliedDateFormat;

        try
        {
            LocalizationService.ApplyPreferences(language, currency, timeZone);
            await _preferencesService.SaveAsync(new LocalizationPreferences
            {
                Language = language,
                Currency = currency,
                TimeZone = timeZone,
                DateFormat = dateFormat,
            });
            _appliedDateFormat = dateFormat;

            if (showNotification)
            {
                await _notificationService.SendAsync(
                    LocalizationService.GetString("SettingsLocalizationSavedTitle"),
                    string.Format(LocalizationService.Culture, LocalizationService.GetString("SettingsLocalizationSavedMessage"), dateFormat),
                    NotificationType.Success);
            }
        }
        finally
        {
            _isApplyingPreferences = false;
            ApplyCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanApply()
        => !string.IsNullOrWhiteSpace(SelectedCurrencyValue)
        && !string.IsNullOrWhiteSpace(SelectedTimeZoneValue)
        && !string.IsNullOrWhiteSpace(SelectedLanguageValue)
        && !string.IsNullOrWhiteSpace(SelectedDateFormatValue)
        && (SelectedCurrencyValue != LocalizationService.Currency
            || SelectedTimeZoneValue != LocalizationService.TimeZone
            || SelectedLanguageValue != LocalizationService.Language
            || SelectedDateFormatValue != _appliedDateFormat);

    private void RefreshOptions()
    {
        _isRefreshingOptions = true;

        string selectedCurrencyValue = SelectedCurrencyValue ?? LocalizationService.Currency;
        string selectedTimeZoneValue = SelectedTimeZoneValue ?? LocalizationService.TimeZone;
        string selectedLanguageValue = SelectedLanguageValue ?? LocalizationService.Language;
        string selectedDateFormatValue = SelectedDateFormatValue ?? _preferencesService.Preferences.DateFormat;

        try
        {
            ReplaceOptions(
                AvailableCurrencies,
                [
                    new LocalizationOptionModel { Value = "VND", DisplayName = LocalizationService.GetString("LocalizationCurrencyVndOptionText") },
                    new LocalizationOptionModel { Value = "USD", DisplayName = LocalizationService.GetString("LocalizationCurrencyUsdOptionText") },
                ]);

            ReplaceOptions(
                AvailableTimeZones,
                [
                    new LocalizationOptionModel { Value = "-5", DisplayName = LocalizationService.GetString("LocalizationTimezoneMinus5OptionText") },
                    new LocalizationOptionModel { Value = "0", DisplayName = LocalizationService.GetString("LocalizationTimezoneUtcOptionText") },
                    new LocalizationOptionModel { Value = "+7", DisplayName = LocalizationService.GetString("LocalizationTimezonePlus7OptionText") },
                    new LocalizationOptionModel { Value = "+9", DisplayName = LocalizationService.GetString("LocalizationTimezonePlus9OptionText") },
                ]);

            ReplaceOptions(
                AvailableLanguages,
                [
                    new LocalizationOptionModel { Value = "en-US", DisplayName = LocalizationService.GetString("LocalizationLanguageEnglishOptionText") },
                    new LocalizationOptionModel { Value = "vi-VN", DisplayName = LocalizationService.GetString("LocalizationLanguageVietnameseOptionText") },
                ]);

            ReplaceOptions(
                AvailableDateFormats,
                [
                    new LocalizationOptionModel { Value = "dd/MM/yyyy", DisplayName = "dd/MM/yyyy" },
                    new LocalizationOptionModel { Value = "MM/dd/yyyy", DisplayName = "MM/dd/yyyy" },
                    new LocalizationOptionModel { Value = "yyyy-MM-dd", DisplayName = "yyyy-MM-dd" },
                ]);

            SelectedCurrencyValue = ResolveAvailableValue(AvailableCurrencies, selectedCurrencyValue, LocalizationService.Currency);
            SelectedTimeZoneValue = ResolveAvailableValue(AvailableTimeZones, selectedTimeZoneValue, LocalizationService.TimeZone);
            SelectedLanguageValue = ResolveAvailableValue(AvailableLanguages, selectedLanguageValue, LocalizationService.Language);
            SelectedDateFormatValue = ResolveAvailableValue(AvailableDateFormats, selectedDateFormatValue, _appliedDateFormat);
        }
        finally
        {
            _isRefreshingOptions = false;
        }
    }

    private void TriggerImmediateApply()
    {
        if (_isRefreshingOptions || _isApplyingPreferences || _isImmediateApplyQueued || !CanApply())
        {
            return;
        }

        _isImmediateApplyQueued = true;
        _ = ApplyPreferencesDeferredAsync();
    }

    private async Task ApplyPreferencesDeferredAsync()
    {
        try
        {
            await Task.Yield();

            if (_isRefreshingOptions || _isApplyingPreferences || !CanApply())
            {
                return;
            }

            await ApplyPreferencesAsync(showNotification: false);
        }
        finally
        {
            _isImmediateApplyQueued = false;
            ApplyCommand.NotifyCanExecuteChanged();
        }
    }

    private static string ResolveAvailableValue(
        ObservableCollection<LocalizationOptionModel> options,
        string preferredValue,
        string fallbackValue)
    {
        foreach (LocalizationOptionModel option in options)
        {
            if (option.Value == preferredValue)
            {
                return option.Value;
            }
        }

        foreach (LocalizationOptionModel option in options)
        {
            if (option.Value == fallbackValue)
            {
                return option.Value;
            }
        }

        return options.Count > 0 ? options[0].Value : string.Empty;
    }

    private static void ReplaceOptions(
        ObservableCollection<LocalizationOptionModel> collection,
        IReadOnlyList<LocalizationOptionModel> items)
    {
        collection.Clear();
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }
}
