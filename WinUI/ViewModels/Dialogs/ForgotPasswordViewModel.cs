using System;
using System.Threading.Tasks;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinUI.Helpers.Validations;
using WinUI.ViewModels;

namespace WinUI.ViewModels.Dialogs;

public partial class ForgotPasswordViewModel : LocalizedViewModelBase
{
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    public partial string TitleDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EmailLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EmailPlaceholderDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendOtpCommand))]
    [NotifyPropertyChangedFor(nameof(CanSendOtpExecute))]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SendOtpButtonDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string BackButtonDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CloseTooltipDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendOtpCommand))]
    [NotifyPropertyChangedFor(nameof(CanSendOtpExecute))]
    [NotifyPropertyChangedFor(nameof(IsNotSubmitting))]
    public partial bool IsSubmitting { get; set; }

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasError { get; set; }

    private event Action? CloseRequestedInternal;

    public bool IsNotSubmitting => !IsSubmitting;
    public bool CanSendOtpExecute => CanSendOtp();

    public event Action? CloseRequested
    {
        add => CloseRequestedInternal += value;
        remove => CloseRequestedInternal -= value;
    }

    public ForgotPasswordViewModel(
        ILocalizationService localizationService,
        IDialogService dialogService)
        : base(localizationService)
    {
        _dialogService = dialogService;

        RefreshLocalizedText();
    }

    protected override void RefreshLocalizedText()
    {
        TitleDisplay = LocalizationService.GetString("ForgotPasswordDialogTitleText");
        EmailLabelDisplay = LocalizationService.GetString("ForgotPasswordDialogEmailLabelText");
        EmailPlaceholderDisplay = LocalizationService.GetString("ForgotPasswordDialogEmailPlaceholderText");
        SendOtpButtonDisplay = LocalizationService.GetString("ForgotPasswordDialogSendOtpButtonText");
        BackButtonDisplay = LocalizationService.GetString("ForgotPasswordDialogBackButtonText");
        CloseTooltipDisplay = LocalizationService.GetString("CloseTooltipText");
    }

    [RelayCommand(CanExecute = nameof(CanSendOtp))]
    private async Task SendOtpAsync()
    {
        IsSubmitting = true;
        HasError = false;
        ErrorMessage = string.Empty;

        try
        {
            if (!EmailValidation.IsValid(Email))
            {
                HasError = true;
                ErrorMessage = LocalizationService.GetString("ForgotPasswordDialogInvalidEmailText");
                return;
            }

            CloseRequestedInternal?.Invoke();
            await Task.Yield();
            await _dialogService.ShowDialogAsync("Otp");
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    [RelayCommand]
    private async Task BackAsync()
    {
        CloseRequestedInternal?.Invoke();
        await Task.Yield();
        await _dialogService.ShowDialogAsync("Login");
    }

    [RelayCommand]
    private void Close()
    {
        CloseRequestedInternal?.Invoke();
    }

    private bool CanSendOtp() => !IsSubmitting && !string.IsNullOrWhiteSpace(Email);
}
