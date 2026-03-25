using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Application.Services;

namespace WinUI.ViewModels.Dialogs;

public partial class ConfigViewModel : ObservableObject
{
    private readonly ILocalizationService _loc;
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

    public ConfigViewModel(ILocalizationService loc, IDialogService dialogService, IConfigurationService configService)
    {
        _loc = loc;
        _dialogService = dialogService;
        _configService = configService;

        // Load saved config values
        ServerAddress = _configService.ServerAddress;
        ApiKey = _configService.ApiKey;
        RememberMe = _configService.RememberMe;

        _loc.LanguageChanged += UpdateTexts;
        UpdateTexts();
    }

    private void UpdateTexts()
    {
        TitleDisplay = _loc.GetString("ConfigDialogTitleText");
        ServerAddressLabelDisplay = _loc.GetString("ConfigDialogServerAddressLabelText");
        ServerAddressPlaceholderDisplay = _loc.GetString("ConfigDialogServerAddressPlaceholderText");
        ApiKeyLabelDisplay = _loc.GetString("ConfigDialogApiKeyLabelText");
        ApiKeyPlaceholderDisplay = _loc.GetString("ConfigDialogApiKeyPlaceholderText");
        RememberMeLabelDisplay = _loc.GetString("ConfigDialogRememberMeLabelText");
        SaveButtonDisplay = _loc.GetString("ConfigDialogSaveButtonText");
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