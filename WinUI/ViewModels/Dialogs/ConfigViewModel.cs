using System;
using System.Threading.Tasks;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinUI.ViewModels;

namespace WinUI.ViewModels.Dialogs;

public partial class ConfigViewModel : LocalizedViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IConfigurationService _configService;

    [ObservableProperty]
    public partial string TitleText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ServerAddressLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ServerAddressPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyPropertyChangedFor(nameof(CanSaveExecute))]
    public partial string ServerAddress { get; set; }

    [ObservableProperty]
    public partial string PortLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PortPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyPropertyChangedFor(nameof(CanSaveExecute))]
    public partial string Port { get; set; }

    [ObservableProperty]
    public partial bool RememberMe { get; set; }

    [ObservableProperty]
    public partial string RememberMeLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SaveButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CloseTooltipText { get; set; } = string.Empty;

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
        Port = _configService.Port.ToString();
        RememberMe = _configService.RememberMe;

        RefreshLocalizedText();
    }

    protected override void RefreshLocalizedText()
    {
        TitleText = LocalizationService.GetString("ConfigDialogTitleText");
        ServerAddressLabelText = LocalizationService.GetString("ConfigDialogServerAddressLabelText");
        ServerAddressPlaceholderText = LocalizationService.GetString("ConfigDialogServerAddressPlaceholderText");
        PortLabelText = LocalizationService.GetString("ConfigDialogPortLabelText");
        PortPlaceholderText = LocalizationService.GetString("ConfigDialogPortPlaceholderText");
        RememberMeLabelText = LocalizationService.GetString("ConfigDialogRememberMeLabelText");
        SaveButtonText = LocalizationService.GetString("ConfigDialogSaveButtonText");
        CloseTooltipText = LocalizationService.GetString("CloseTooltipText");
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        IsSaving = true;
        try
        {
            if (!TryGetPort(out int port))
                return;

            await _configService.SaveAsync(ServerAddress.Trim(), port, RememberMe);

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
        TryGetPort(out _);

    private bool CanClose() => !IsSaving;

    private bool TryGetPort(out int port) =>
        int.TryParse(Port, out port) && port is >= 1 and <= 65535;
}
