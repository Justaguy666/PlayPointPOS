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
    private OtpDialogMode _mode = OtpDialogMode.ResetPassword;
    private string _pendingEmail = string.Empty;
    private Func<Task>? _verifiedCallback;

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
    public partial bool ShowPasswordFields { get; set; } = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(VerifyCommand))]
    [NotifyPropertyChangedFor(nameof(CanVerifyExecute))]
    public partial string ConfirmPassword { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool ShowChangeEmailButton { get; set; } = true;

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
        if (_mode == OtpDialogMode.VerifyEmailChange)
        {
            TitleDisplay = LocalizationService.GetString("OtpDialogEmailChangeTitleText");
            ShowPasswordFields = false;
            ShowChangeEmailButton = false;
            OtpLabelDisplay = LocalizationService.GetString("OtpDialogOtpLabelText");
            OtpPlaceholderDisplay = LocalizationService.GetString("OtpDialogOtpPlaceholderText");
            NewPasswordLabelDisplay = LocalizationService.GetString("OtpDialogNewPasswordLabelText");
            NewPasswordPlaceholderDisplay = LocalizationService.GetString("OtpDialogNewPasswordPlaceholderText");
            ConfirmPasswordLabelDisplay = LocalizationService.GetString("OtpDialogConfirmPasswordLabelText");
            ConfirmPasswordPlaceholderDisplay = LocalizationService.GetString("OtpDialogConfirmPasswordPlaceholderText");
            ChangeEmailButtonDisplay = LocalizationService.GetString("OtpDialogChangeEmailButtonText");
            VerifyButtonDisplay = LocalizationService.GetString("OtpDialogEmailChangeVerifyButtonText");
        }
        else if (_mode == OtpDialogMode.VerifyRegistration)
        {
            TitleDisplay = LocalizationService.GetString("OtpDialogRegistrationTitleText");
            ShowPasswordFields = false;
            ShowChangeEmailButton = false;
            OtpLabelDisplay = LocalizationService.GetString("OtpDialogOtpLabelText");
            OtpPlaceholderDisplay = LocalizationService.GetString("OtpDialogOtpPlaceholderText");
            NewPasswordLabelDisplay = LocalizationService.GetString("OtpDialogNewPasswordLabelText");
            NewPasswordPlaceholderDisplay = LocalizationService.GetString("OtpDialogNewPasswordPlaceholderText");
            ConfirmPasswordLabelDisplay = LocalizationService.GetString("OtpDialogConfirmPasswordLabelText");
            ConfirmPasswordPlaceholderDisplay = LocalizationService.GetString("OtpDialogConfirmPasswordPlaceholderText");
            ChangeEmailButtonDisplay = LocalizationService.GetString("OtpDialogChangeEmailButtonText");
            VerifyButtonDisplay = LocalizationService.GetString("OtpDialogRegistrationVerifyButtonText");
        }
        else
        {
            TitleDisplay = LocalizationService.GetString("OtpDialogTitleText");
            ShowPasswordFields = true;
            ShowChangeEmailButton = true;
            OtpLabelDisplay = LocalizationService.GetString("OtpDialogOtpLabelText");
            OtpPlaceholderDisplay = LocalizationService.GetString("OtpDialogOtpPlaceholderText");
            NewPasswordLabelDisplay = LocalizationService.GetString("OtpDialogNewPasswordLabelText");
            NewPasswordPlaceholderDisplay = LocalizationService.GetString("OtpDialogNewPasswordPlaceholderText");
            ConfirmPasswordLabelDisplay = LocalizationService.GetString("OtpDialogConfirmPasswordLabelText");
            ConfirmPasswordPlaceholderDisplay = LocalizationService.GetString("OtpDialogConfirmPasswordPlaceholderText");
            ChangeEmailButtonDisplay = LocalizationService.GetString("OtpDialogChangeEmailButtonText");
            VerifyButtonDisplay = LocalizationService.GetString("OtpDialogVerifyButtonText");
        }

        CloseTooltipDisplay = LocalizationService.GetString("CloseTooltipText");
    }

    public void Configure(OtpDialogRequest? request)
    {
        _mode = request?.Mode ?? OtpDialogMode.ResetPassword;
        _pendingEmail = request?.PendingEmail ?? string.Empty;
        _verifiedCallback = request?.OnVerifiedAsync;

        OtpCode = string.Empty;
        NewPassword = string.Empty;
        ConfirmPassword = string.Empty;
        HasError = false;
        ErrorMessage = string.Empty;
        IsWorking = false;

        RefreshLocalizedText();
        VerifyCommand.NotifyCanExecuteChanged();
        ChangeEmailCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanVerifyExecute));
    }

    [RelayCommand(CanExecute = nameof(CanVerify))]
    private async Task VerifyAsync()
    {
        HasError = false;
        ErrorMessage = string.Empty;

        if (_mode == OtpDialogMode.ResetPassword && NewPassword != ConfirmPassword)
        {
            HasError = true;
            ErrorMessage = LocalizationService.GetString("OtpDialogPasswordMismatchText");
            return;
        }

        CloseRequestedInternal?.Invoke();

        await Task.Yield();

        if (_verifiedCallback is not null)
        {
            await _verifiedCallback();
        }
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
        if (IsWorking || string.IsNullOrWhiteSpace(OtpCode))
        {
            return false;
        }

        return _mode is OtpDialogMode.VerifyEmailChange or OtpDialogMode.VerifyRegistration
            || (!string.IsNullOrWhiteSpace(NewPassword)
                && !string.IsNullOrWhiteSpace(ConfirmPassword));
    }

    private bool CanChangeEmail() => !IsWorking && _mode == OtpDialogMode.ResetPassword;
}
