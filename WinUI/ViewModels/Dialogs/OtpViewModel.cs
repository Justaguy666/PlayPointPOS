using System;
using System.Threading.Tasks;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinUI.ViewModels;

namespace WinUI.ViewModels.Dialogs;

public partial class OtpViewModel : LocalizedViewModelBase
{
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    public partial string TitleDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string OtpLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string OtpPlaceholderDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(VerifyCommand))]
    [NotifyPropertyChangedFor(nameof(CanVerifyExecute))]
    public partial string OtpCode { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewPasswordLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewPasswordPlaceholderDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(VerifyCommand))]
    [NotifyPropertyChangedFor(nameof(CanVerifyExecute))]
    public partial string NewPassword { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ConfirmPasswordLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ConfirmPasswordPlaceholderDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(VerifyCommand))]
    [NotifyPropertyChangedFor(nameof(CanVerifyExecute))]
    public partial string ConfirmPassword { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ChangeEmailButtonDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string VerifyButtonDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CloseTooltipDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(VerifyCommand))]
    [NotifyCanExecuteChangedFor(nameof(ChangeEmailCommand))]
    [NotifyPropertyChangedFor(nameof(CanVerifyExecute))]
    [NotifyPropertyChangedFor(nameof(IsNotWorking))]
    public partial bool IsWorking { get; set; }

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasError { get; set; }

    private event Action? CloseRequestedInternal;

    public bool CanVerifyExecute => CanVerify();
    public bool IsNotWorking => !IsWorking;

    public event Action? CloseRequested
    {
        add => CloseRequestedInternal += value;
        remove => CloseRequestedInternal -= value;
    }

    public OtpViewModel(
        ILocalizationService localizationService,
        IDialogService dialogService)
        : base(localizationService)
    {
        _dialogService = dialogService;

        RefreshLocalizedText();
    }

    protected override void RefreshLocalizedText()
    {
        TitleDisplay = LocalizationService.GetString("OtpDialogTitleText");
        OtpLabelDisplay = LocalizationService.GetString("OtpDialogOtpLabelText");
        OtpPlaceholderDisplay = LocalizationService.GetString("OtpDialogOtpPlaceholderText");
        NewPasswordLabelDisplay = LocalizationService.GetString("OtpDialogNewPasswordLabelText");
        NewPasswordPlaceholderDisplay = LocalizationService.GetString("OtpDialogNewPasswordPlaceholderText");
        ConfirmPasswordLabelDisplay = LocalizationService.GetString("OtpDialogConfirmPasswordLabelText");
        ConfirmPasswordPlaceholderDisplay = LocalizationService.GetString("OtpDialogConfirmPasswordPlaceholderText");
        ChangeEmailButtonDisplay = LocalizationService.GetString("OtpDialogChangeEmailButtonText");
        VerifyButtonDisplay = LocalizationService.GetString("OtpDialogVerifyButtonText");
        CloseTooltipDisplay = LocalizationService.GetString("CloseTooltipText");
    }

    [RelayCommand(CanExecute = nameof(CanVerify))]
    private void Verify()
    {
        HasError = false;
        ErrorMessage = string.Empty;

        if (NewPassword != ConfirmPassword)
        {
            HasError = true;
            ErrorMessage = LocalizationService.GetString("OtpDialogPasswordMismatchText");
            return;
        }

        CloseRequestedInternal?.Invoke();
    }

    [RelayCommand(CanExecute = nameof(CanChangeEmail))]
    private async Task ChangeEmailAsync()
    {
        CloseRequestedInternal?.Invoke();
        await Task.Yield();
        await _dialogService.ShowDialogAsync("ForgotPassword");
    }

    [RelayCommand]
    private void Close()
    {
        CloseRequestedInternal?.Invoke();
    }

    private bool CanVerify()
    {
        return !IsWorking &&
               !string.IsNullOrWhiteSpace(OtpCode) &&
               !string.IsNullOrWhiteSpace(NewPassword) &&
               !string.IsNullOrWhiteSpace(ConfirmPassword);
    }

    private bool CanChangeEmail() => !IsWorking;
}
