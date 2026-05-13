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
    private readonly IAuthApiService _authApiService;
    private ForgotPasswordDialogMode _mode = ForgotPasswordDialogMode.ForgotPassword;

    [ObservableProperty]
    public partial string TitleText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EmailLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EmailPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendOtpCommand))]
    [NotifyPropertyChangedFor(nameof(CanSendOtpExecute))]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SendOtpButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string BackButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CloseTooltipText { get; set; } = string.Empty;

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
        IDialogService dialogService,
        IAuthApiService authApiService)
        : base(localizationService)
    {
        _dialogService = dialogService;
        _authApiService = authApiService ?? throw new ArgumentNullException(nameof(authApiService));

        RefreshLocalizedText();
    }

    protected override void RefreshLocalizedText()
    {
        TitleText = LocalizationService.GetString(
            _mode == ForgotPasswordDialogMode.ChangePassword
                ? "ForgotPasswordDialogChangePasswordTitleText"
                : "ForgotPasswordDialogTitleText");
        EmailLabelText = LocalizationService.GetString("ForgotPasswordDialogEmailLabelText");
        EmailPlaceholderText = LocalizationService.GetString("ForgotPasswordDialogEmailPlaceholderText");
        SendOtpButtonText = LocalizationService.GetString("ForgotPasswordDialogSendOtpButtonText");
        BackButtonText = LocalizationService.GetString("ForgotPasswordDialogBackButtonText");
        CloseTooltipText = LocalizationService.GetString("CloseTooltipText");
    }

    public void Configure(ForgotPasswordDialogRequest? request)
    {
        _mode = request?.Mode ?? ForgotPasswordDialogMode.ForgotPassword;
        RefreshLocalizedText();
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

            string email = Email.Trim();
            var sendOtpResult = await _authApiService.SendPasswordResetOtpAsync(email);
            if (!sendOtpResult.Success)
            {
                HasError = true;
                ErrorMessage = sendOtpResult.Message;
                return;
            }

            CloseRequestedInternal?.Invoke();
            await Task.Yield();
            await _dialogService.ShowDialogAsync(
                "Otp",
                new OtpDialogRequest
                {
                    Mode = OtpDialogMode.ResetPassword,
                    PendingEmail = email,
                });
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
