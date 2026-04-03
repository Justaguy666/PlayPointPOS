using System;
using System.Threading.Tasks;
using Application.Services;
using Application.UseCases.Auth;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Entities;
using WinUI.Helpers.Validations;
using WinUI.ViewModels;

namespace WinUI.ViewModels.Dialogs;

public partial class RegisterViewModel : LocalizedViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly RegisterUserUseCase _registerUseCase;
    private readonly INotificationService _notificationService;

    [ObservableProperty]
    public partial string TitleDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EmailLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EmailPlaceholderDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    [NotifyPropertyChangedFor(nameof(CanRegisterExecute))]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PasswordLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PasswordPlaceholderDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    [NotifyPropertyChangedFor(nameof(CanRegisterExecute))]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ConfirmPasswordLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ConfirmPasswordPlaceholderDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    [NotifyPropertyChangedFor(nameof(CanRegisterExecute))]
    public partial string ConfirmPassword { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string RegisterButtonDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ResetButtonDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CloseTooltipDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    [NotifyPropertyChangedFor(nameof(CanRegisterExecute))]
    [NotifyPropertyChangedFor(nameof(IsNotRegistering))]
    public partial bool IsRegistering { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ResetCommand))]
    [NotifyPropertyChangedFor(nameof(CanResetExecute))]
    [NotifyPropertyChangedFor(nameof(IsNotResetting))]
    public partial bool IsResetting { get; set; }

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasError { get; set; }

    private event Action? CloseRequestedInternal;
    private event Action<Account>? RegisterSucceededInternal;

    public Account? RegisteredAccount { get; private set; }

    public bool IsNotRegistering => !IsRegistering;
    public bool IsNotResetting => !IsResetting;
    public bool CanRegisterExecute => CanRegister();
    public bool CanResetExecute => !IsRegistering && !IsResetting;

    public event Action? CloseRequested
    {
        add => CloseRequestedInternal += value;
        remove => CloseRequestedInternal -= value;
    }

    public event Action<Account>? RegisterSucceeded
    {
        add => RegisterSucceededInternal += value;
        remove => RegisterSucceededInternal -= value;
    }

    public RegisterViewModel(
        ILocalizationService localizationService,
        IDialogService dialogService,
        RegisterUserUseCase registerUseCase,
        INotificationService notificationService)
        : base(localizationService)
    {
        _dialogService = dialogService;
        _registerUseCase = registerUseCase;
        _notificationService = notificationService;

        RefreshLocalizedText();
    }

    protected override void RefreshLocalizedText()
    {
        TitleDisplay = LocalizationService.GetString("RegisterDialogTitleText");
        EmailLabelDisplay = LocalizationService.GetString("RegisterDialogEmailLabelText");
        EmailPlaceholderDisplay = LocalizationService.GetString("RegisterDialogEmailPlaceholderText");
        PasswordLabelDisplay = LocalizationService.GetString("RegisterDialogPasswordLabelText");
        PasswordPlaceholderDisplay = LocalizationService.GetString("RegisterDialogPasswordPlaceHolderText");
        ConfirmPasswordLabelDisplay = LocalizationService.GetString("RegisterDialogConfirmPasswordLabelText");
        ConfirmPasswordPlaceholderDisplay = LocalizationService.GetString("RegisterDialogConfirmPasswordPlaceholderText");
        RegisterButtonDisplay = LocalizationService.GetString("RegisterDialogCreateButtonText");
        ResetButtonDisplay = LocalizationService.GetString("RegisterDialogResetButtonText");
        CloseTooltipDisplay = LocalizationService.GetString("CloseTooltipText");
    }

    [RelayCommand(CanExecute = nameof(CanRegister))]
    private async Task RegisterAsync()
    {
        IsRegistering = true;
        HasError = false;
        ErrorMessage = string.Empty;

        try
        {
            if (!EmailValidation.IsValid(Email))
            {
                HasError = true;
                ErrorMessage = LocalizationService.GetString("RegisterDialogInvalidEmailText");
                return;
            }

            if (Password != ConfirmPassword)
            {
                HasError = true;
                ErrorMessage = LocalizationService.GetString("RegisterDialogPasswordMismatchText");
                return;
            }

            string email = Email.Trim();
            string password = Password;
            string language = LocalizationService.Language;

            CloseRequestedInternal?.Invoke();
            await Task.Yield();
            await _dialogService.ShowDialogAsync(
                "Otp",
                new OtpDialogRequest
                {
                    Mode = OtpDialogMode.VerifyRegistration,
                    PendingEmail = email,
                    OnVerifiedAsync = () => CompleteRegistrationAsync(email, password, language)
                });
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync(ex.Message);
        }
        finally
        {
            IsRegistering = false;
        }
    }

    private async Task CompleteRegistrationAsync(string email, string password, string language)
    {
        var result = await _registerUseCase.ExecuteAsync(email, password, language);

        if (result.Success)
        {
            RegisteredAccount = result.Account;
            RegisterSucceededInternal?.Invoke(result.Account!);

            await _notificationService.SendAsync(
                LocalizationService.GetString("RegisterSuccessTitle"),
                string.Format(LocalizationService.GetString("RegisterSuccessMessage"), result.Account!.Email),
                NotificationType.Success);
            return;
        }

        await _dialogService.ShowErrorAsync(result.Message ?? LocalizationService.GetString("RegisterDialogErrorText"));
    }

    [RelayCommand(CanExecute = nameof(CanResetExecute))]
    private void Reset()
    {
        Email = string.Empty;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        HasError = false;
        ErrorMessage = string.Empty;
    }

    private bool CanRegister() =>
        !IsRegistering &&
        !string.IsNullOrWhiteSpace(Email) &&
        !string.IsNullOrWhiteSpace(Password) &&
        !string.IsNullOrWhiteSpace(ConfirmPassword);

    [RelayCommand]
    private void Close()
    {
        CloseRequestedInternal?.Invoke();
    }
}
