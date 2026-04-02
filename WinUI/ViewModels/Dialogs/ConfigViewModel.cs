using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Application.Services;
using WinUI.ViewModels;

namespace WinUI.ViewModels.Dialogs;

public partial class ConfigViewModel : LocalizedViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IConfigurationService _configService;

    [ObservableProperty]
    public partial string TitleDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ServerAddressLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ServerAddressPlaceholderDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyPropertyChangedFor(nameof(CanSaveExecute))]
    public partial string ServerAddress { get; set; }

    [ObservableProperty]
    public partial string ApiKeyLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ApiKeyPlaceholderDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyPropertyChangedFor(nameof(CanSaveExecute))]
    public partial string ApiKey { get; set; }

    [ObservableProperty]
    public partial bool RememberMe { get; set; }

    [ObservableProperty]
    public partial string RememberMeLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SaveButtonDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CloseTooltipDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyCanExecuteChangedFor(nameof(CloseCommand))]
    [NotifyPropertyChangedFor(nameof(CanSaveExecute))]
    [NotifyPropertyChangedFor(nameof(IsNotSaving))]
    public partial bool IsSaving { get; set; }

    [ObservableProperty]
    public partial bool Confirmed { get; set; }

    private event Action? CloseRequestedInternal;

    public bool IsNotSaving => !IsSaving;

    public bool CanSaveExecute => CanSave();

    public event Action? CloseRequested
    {
        add => CloseRequestedInternal += value;
        remove => CloseRequestedInternal -= value;
    }

    public ConfigViewModel(ILocalizationService localizationService, IDialogService dialogService, IConfigurationService configService)
        : base(localizationService)
    {
        _dialogService = dialogService;
        _configService = configService;

        ServerAddress = _configService.ServerAddress;
        ApiKey = _configService.ApiKey;
        RememberMe = _configService.RememberMe;

        RefreshLocalizedText();
    }

    protected override void RefreshLocalizedText()
    {
        TitleDisplay = LocalizationService.GetString("ConfigDialogTitleText");
        ServerAddressLabelDisplay = LocalizationService.GetString("ConfigDialogServerAddressLabelText");
        ServerAddressPlaceholderDisplay = LocalizationService.GetString("ConfigDialogServerAddressPlaceholderText");
        ApiKeyLabelDisplay = LocalizationService.GetString("ConfigDialogApiKeyLabelText");
        ApiKeyPlaceholderDisplay = LocalizationService.GetString("ConfigDialogApiKeyPlaceholderText");
        RememberMeLabelDisplay = LocalizationService.GetString("ConfigDialogRememberMeLabelText");
        SaveButtonDisplay = LocalizationService.GetString("ConfigDialogSaveButtonText");
        CloseTooltipDisplay = LocalizationService.GetString("CloseTooltipText");
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        IsSaving = true;
        try
        {
            await _configService.SaveAsync(ServerAddress, ApiKey, RememberMe);

            Confirmed = true;
            CloseRequestedInternal?.Invoke();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync(ex.Message);
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanClose))]
    private void Close()
    {
        Confirmed = false;
        CloseRequestedInternal?.Invoke();
    }

    private bool CanSave() =>
        !IsSaving &&
        !string.IsNullOrWhiteSpace(ServerAddress) &&
        !string.IsNullOrWhiteSpace(ApiKey);

    private bool CanClose() => !IsSaving;
}
