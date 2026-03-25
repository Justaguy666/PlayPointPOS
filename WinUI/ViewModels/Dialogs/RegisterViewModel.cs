using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Navigation;
using Application.Navigation.Requests;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Entities;

namespace WinUI.ViewModels.Dialogs;

public partial class RegisterViewModel : ObservableObject
{
    private readonly ILocalizationService _loc;
    private readonly IDialogService _dialogService;
    private readonly IRepository<Account> _accountRepo;
    private readonly INavigationService _navigationService;
    private readonly INotificationService _notificationService;
    private readonly MainViewModel _mainViewModel;

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
    public partial string OTPLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string OTPPlaceholderDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    [NotifyPropertyChangedFor(nameof(CanRegisterExecute))]
    public partial string OTP { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string RegisterButtonDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ResetButtonDisplay { get; set; } = string.Empty;

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
        ILocalizationService loc,
        IDialogService dialogService,
        IRepository<Account> accountRepo,
        INavigationService navigationService,
        INotificationService notificationService,
        MainViewModel mainViewModel)
    {
        _loc = loc;
        _dialogService = dialogService;
        _accountRepo = accountRepo;
        _navigationService = navigationService;
        _notificationService = notificationService;
        _mainViewModel = mainViewModel;

        _loc.LanguageChanged += UpdateTexts;
        UpdateTexts();
    }

    private void UpdateTexts()
    {
        TitleDisplay = _loc.GetString("RegisterDialogTitleText");
        EmailLabelDisplay = _loc.GetString("RegisterDialogEmailLabelText");
        EmailPlaceholderDisplay = _loc.GetString("RegisterDialogEmailPlaceholderText");
        PasswordLabelDisplay = _loc.GetString("RegisterDialogPasswordLabelText");
        PasswordPlaceholderDisplay = _loc.GetString("RegisterDialogPasswordPlaceHolderText");
        ConfirmPasswordLabelDisplay = _loc.GetString("RegisterDialogConfirmPasswordLabelText");
        ConfirmPasswordPlaceholderDisplay = _loc.GetString("RegisterDialogConfirmPasswordPlaceholderText");
        OTPLabelDisplay = _loc.GetString("RegisterDialogOTPLabelText");
        OTPPlaceholderDisplay = _loc.GetString("RegisterDialogOTPPlaceHolderText");
        RegisterButtonDisplay = _loc.GetString("RegisterDialogCreateButtonText");
        ResetButtonDisplay = _loc.GetString("RegisterDialogResetButtonText");
    }

    [RelayCommand(CanExecute = nameof(CanRegister))]
    private async Task RegisterAsync()
    {
        IsRegistering = true;
        HasError = false;
        ErrorMessage = string.Empty;

        try
        {
            if (Password != ConfirmPassword)
            {
                HasError = true;
                ErrorMessage = _loc.GetString("RegisterDialogPasswordMismatchText");
                return;
            }

            var accounts = await _accountRepo.GetAllAsync();
            if (accounts.Any(a => a.Email.Equals(Email, StringComparison.OrdinalIgnoreCase)))
            {
                HasError = true;
                ErrorMessage = _loc.GetString("RegisterDialogErrorText");
                return;
            }

            var newAccount = new Account
            {
                Email = Email,
                PasswordHash = Password,
                Language = "en-US"
            };

            await _accountRepo.AddAsync(newAccount);

            RegisteredAccount = newAccount;
            RegisterSucceededInternal?.Invoke(newAccount);
            CloseRequestedInternal?.Invoke();

            // Toast notification
            await _notificationService.SendAsync(
                _loc.GetString("RegisterSuccessTitle"),
                string.Format(_loc.GetString("RegisterSuccessMessage"), newAccount.Email),
                NotificationType.Success);
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

    [RelayCommand(CanExecute = nameof(CanResetExecute))]
    private void Reset()
    {
        Email = string.Empty;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        OTP = string.Empty;
        HasError = false;
        ErrorMessage = string.Empty;
    }

    private bool CanRegister() =>
        !IsRegistering &&
        !string.IsNullOrWhiteSpace(Email) &&
        !string.IsNullOrWhiteSpace(Password) &&
        !string.IsNullOrWhiteSpace(ConfirmPassword) &&
        !string.IsNullOrWhiteSpace(OTP);
}
