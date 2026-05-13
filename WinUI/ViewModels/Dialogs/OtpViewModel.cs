using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinUI.ViewModels;

namespace WinUI.ViewModels.Dialogs;

public partial class OtpViewModel : LocalizedViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IAuthApiService _authApiService;
    private OtpDialogMode _mode = OtpDialogMode.ResetPassword;
    private string _pendingEmail = string.Empty;
    private Func<Task>? _verifiedCallback;
    private Func<string, Task>? _verifiedWithOtpCallback;
    private CancellationTokenSource? _resendCooldownCts;

    [ObservableProperty]
    public partial string TitleText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string OtpLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string OtpPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(VerifyCommand))]
    [NotifyPropertyChangedFor(nameof(CanVerifyExecute))]
    public partial string OtpCode { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewPasswordLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewPasswordPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(VerifyCommand))]
    [NotifyPropertyChangedFor(nameof(CanVerifyExecute))]
    public partial string NewPassword { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ConfirmPasswordLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ConfirmPasswordPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool ShowPasswordFields { get; set; } = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(VerifyCommand))]
    [NotifyPropertyChangedFor(nameof(CanVerifyExecute))]
    public partial string ConfirmPassword { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool ShowChangeEmailButton { get; set; } = true;

    [ObservableProperty]
    public partial string ChangeEmailButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string VerifyButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SendOtpButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendOtpAgainCommand))]
    [NotifyPropertyChangedFor(nameof(CanSendOtpAgainExecute))]
    [NotifyPropertyChangedFor(nameof(IsSendOtpEnabled))]
    public partial int ResendSecondsRemaining { get; set; }

    [ObservableProperty]
    public partial string CloseTooltipText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(VerifyCommand))]
    [NotifyCanExecuteChangedFor(nameof(ChangeEmailCommand))]
    [NotifyCanExecuteChangedFor(nameof(SendOtpAgainCommand))]
    [NotifyPropertyChangedFor(nameof(CanVerifyExecute))]
    [NotifyPropertyChangedFor(nameof(IsNotWorking))]
    [NotifyPropertyChangedFor(nameof(IsSendOtpEnabled))]
    public partial bool IsWorking { get; set; }

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasError { get; set; }

    private event Action? CloseRequestedInternal;

    public bool CanVerifyExecute => CanVerify();
    public bool CanSendOtpAgainExecute => CanSendOtpAgain();
    public bool IsNotWorking => !IsWorking;
    public bool IsSendOtpEnabled => CanSendOtpAgain();

    public event Action? CloseRequested
    {
        add => CloseRequestedInternal += value;
        remove => CloseRequestedInternal -= value;
    }

    public OtpViewModel(
        ILocalizationService localizationService,
        IDialogService dialogService,
        IAuthApiService authApiService)
        : base(localizationService)
    {
        _dialogService = dialogService;
        _authApiService = authApiService;

        RefreshLocalizedText();
    }

    protected override void RefreshLocalizedText()
    {
        if (_mode == OtpDialogMode.VerifyEmailChange)
        {
            TitleText = LocalizationService.GetString("OtpDialogEmailChangeTitleText");
            ShowPasswordFields = false;
            ShowChangeEmailButton = false;
            OtpLabelText = LocalizationService.GetString("OtpDialogOtpLabelText");
            OtpPlaceholderText = LocalizationService.GetString("OtpDialogOtpPlaceholderText");
            NewPasswordLabelText = LocalizationService.GetString("OtpDialogNewPasswordLabelText");
            NewPasswordPlaceholderText = LocalizationService.GetString("OtpDialogNewPasswordPlaceholderText");
            ConfirmPasswordLabelText = LocalizationService.GetString("OtpDialogConfirmPasswordLabelText");
            ConfirmPasswordPlaceholderText = LocalizationService.GetString("OtpDialogConfirmPasswordPlaceholderText");
            ChangeEmailButtonText = LocalizationService.GetString("OtpDialogChangeEmailButtonText");
            VerifyButtonText = LocalizationService.GetString("OtpDialogEmailChangeVerifyButtonText");
        }
        else if (_mode == OtpDialogMode.VerifyRegistration)
        {
            TitleText = LocalizationService.GetString("OtpDialogRegistrationTitleText");
            ShowPasswordFields = false;
            ShowChangeEmailButton = false;
            OtpLabelText = LocalizationService.GetString("OtpDialogOtpLabelText");
            OtpPlaceholderText = LocalizationService.GetString("OtpDialogOtpPlaceholderText");
            NewPasswordLabelText = LocalizationService.GetString("OtpDialogNewPasswordLabelText");
            NewPasswordPlaceholderText = LocalizationService.GetString("OtpDialogNewPasswordPlaceholderText");
            ConfirmPasswordLabelText = LocalizationService.GetString("OtpDialogConfirmPasswordLabelText");
            ConfirmPasswordPlaceholderText = LocalizationService.GetString("OtpDialogConfirmPasswordPlaceholderText");
            ChangeEmailButtonText = LocalizationService.GetString("OtpDialogChangeEmailButtonText");
            VerifyButtonText = LocalizationService.GetString("OtpDialogRegistrationVerifyButtonText");
        }
        else
        {
            TitleText = LocalizationService.GetString("OtpDialogTitleText");
            ShowPasswordFields = true;
            ShowChangeEmailButton = true;
            OtpLabelText = LocalizationService.GetString("OtpDialogOtpLabelText");
            OtpPlaceholderText = LocalizationService.GetString("OtpDialogOtpPlaceholderText");
            NewPasswordLabelText = LocalizationService.GetString("OtpDialogNewPasswordLabelText");
            NewPasswordPlaceholderText = LocalizationService.GetString("OtpDialogNewPasswordPlaceholderText");
            ConfirmPasswordLabelText = LocalizationService.GetString("OtpDialogConfirmPasswordLabelText");
            ConfirmPasswordPlaceholderText = LocalizationService.GetString("OtpDialogConfirmPasswordPlaceholderText");
            ChangeEmailButtonText = LocalizationService.GetString("OtpDialogChangeEmailButtonText");
            VerifyButtonText = LocalizationService.GetString("OtpDialogVerifyButtonText");
        }

        SendOtpButtonText = BuildSendOtpButtonText();
        CloseTooltipText = LocalizationService.GetString("CloseTooltipText");
    }

    public void Configure(OtpDialogRequest? request)
    {
        _mode = request?.Mode ?? OtpDialogMode.ResetPassword;
        _pendingEmail = request?.PendingEmail ?? string.Empty;
        _verifiedCallback = request?.OnVerifiedAsync;
        _verifiedWithOtpCallback = request?.OnVerifiedWithOtpAsync;

        OtpCode = string.Empty;
        NewPassword = string.Empty;
        ConfirmPassword = string.Empty;
        HasError = false;
        ErrorMessage = string.Empty;
        IsWorking = false;
        ResendSecondsRemaining = 0;

        RefreshLocalizedText();
        VerifyCommand.NotifyCanExecuteChanged();
        ChangeEmailCommand.NotifyCanExecuteChanged();
        SendOtpAgainCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanVerifyExecute));

        if (_mode == OtpDialogMode.ResetPassword && !string.IsNullOrWhiteSpace(_pendingEmail))
        {
            _ = StartResendCooldownAsync();
        }
    }

    public event Action? DialogHideRequested;
    public event Action? DialogShowRequested;

    [RelayCommand(CanExecute = nameof(CanVerify))]
    private async Task VerifyAsync()
    {
        HasError = false;
        ErrorMessage = string.Empty;

        if (_mode == OtpDialogMode.ResetPassword)
        {
            if (NewPassword != ConfirmPassword)
            {
                HasError = true;
                ErrorMessage = LocalizationService.GetString("OtpDialogPasswordMismatchText");
                return;
            }

            DialogHideRequested?.Invoke();

            bool isConfirmed = await _dialogService.ShowConfirmationAsync(
                titleKey: "ConfirmChangePasswordTitle",
                messageKey: "ConfirmChangePasswordMessage",
                confirmButtonTextKey: "ConfirmChangePasswordButton",
                cancelButtonTextKey: "CancelButtonText"
            );

            if (!isConfirmed)
            {
                DialogShowRequested?.Invoke();
                return;
            }

            IsWorking = true;
            try
            {
                var result = await _authApiService.ResetPasswordAsync(
                    _pendingEmail.Trim(),
                    NewPassword.Trim(),
                    OtpCode.Trim());
                if (!result.Success)
                {
                    HasError = true;
                    ErrorMessage = result.Message;
                    return;
                }
            }
            finally
            {
                IsWorking = false;
            }

            CloseRequestedInternal?.Invoke();
            return;
        }

        CloseRequestedInternal?.Invoke();

        await Task.Yield();

        if (_verifiedWithOtpCallback is not null)
        {
            await _verifiedWithOtpCallback(OtpCode.Trim());
            return;
        }

        if (_verifiedCallback is not null)
        {
            await _verifiedCallback();
        }
    }

    [RelayCommand(CanExecute = nameof(CanSendOtpAgain))]
    private async Task SendOtpAgainAsync()
    {
        if (_mode != OtpDialogMode.ResetPassword)
        {
            return;
        }

        HasError = false;
        ErrorMessage = string.Empty;
        IsWorking = true;
        try
        {
            var sendOtpResult = await _authApiService.SendPasswordResetOtpAsync(_pendingEmail.Trim());
            if (!sendOtpResult.Success)
            {
                HasError = true;
                ErrorMessage = sendOtpResult.Message;
                return;
            }

            await StartResendCooldownAsync();
        }
        finally
        {
            IsWorking = false;
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

    private bool CanSendOtpAgain()
    {
        return !IsWorking
            && _mode == OtpDialogMode.ResetPassword
            && !string.IsNullOrWhiteSpace(_pendingEmail)
            && ResendSecondsRemaining <= 0;
    }

    private async Task StartResendCooldownAsync()
    {
        _resendCooldownCts?.Cancel();
        _resendCooldownCts = new CancellationTokenSource();
        CancellationToken token = _resendCooldownCts.Token;

        ResendSecondsRemaining = 60;
        SendOtpButtonText = BuildSendOtpButtonText();

        try
        {
            while (ResendSecondsRemaining > 0 && !token.IsCancellationRequested)
            {
                await Task.Delay(1000, token);
                if (token.IsCancellationRequested)
                {
                    break;
                }

                ResendSecondsRemaining--;
                SendOtpButtonText = BuildSendOtpButtonText();
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellation because a newer cooldown already started.
        }
        finally
        {
            if (ResendSecondsRemaining <= 0)
            {
                ResendSecondsRemaining = 0;
                SendOtpButtonText = BuildSendOtpButtonText();
            }

            SendOtpAgainCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(CanSendOtpAgainExecute));
            OnPropertyChanged(nameof(IsSendOtpEnabled));
        }
    }

    private string BuildSendOtpButtonText()
    {
        if (ResendSecondsRemaining > 0)
        {
            string format = LocalizationService.GetString("OtpDialogResendCooldownFormat");
            return string.Format(LocalizationService.Culture, format, ResendSecondsRemaining);
        }

        return LocalizationService.GetString("OtpDialogSendButtonText");
    }
}
