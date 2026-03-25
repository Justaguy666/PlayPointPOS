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

public partial class LoginViewModel : ObservableObject
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
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    [NotifyPropertyChangedFor(nameof(CanLoginExecute))]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PasswordLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PasswordPlaceholderDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    [NotifyPropertyChangedFor(nameof(CanLoginExecute))]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool RememberMe { get; set; }

    [ObservableProperty]
    public partial string RememberMeLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LoginButtonDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    [NotifyPropertyChangedFor(nameof(CanLoginExecute))]
    [NotifyPropertyChangedFor(nameof(IsNotLoggingIn))]
    public partial bool IsLoggingIn { get; set; }

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasError { get; set; }

    private event Action? CloseRequestedInternal;
    private event Action<Account>? LoginSucceededInternal;

    public Account? LoggedInAccount { get; private set; }

    public bool IsNotLoggingIn => !IsLoggingIn;
    public bool CanLoginExecute => CanLogin();

    public event Action? CloseRequested
    {
        add => CloseRequestedInternal += value;
        remove => CloseRequestedInternal -= value;
    }

    public event Action<Account>? LoginSucceeded
    {
        add => LoginSucceededInternal += value;
        remove => LoginSucceededInternal -= value;
    }

    public LoginViewModel(
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
        TitleDisplay = _loc.GetString("LoginDialogTitleText");
        EmailLabelDisplay = _loc.GetString("LoginDialogEmailLabelText");
        EmailPlaceholderDisplay = _loc.GetString("LoginDialogEmailPlaceholderText");
        PasswordLabelDisplay = _loc.GetString("LoginDialogPasswordLabelText");
        PasswordPlaceholderDisplay = _loc.GetString("LoginDialogPasswordPlaceholderText");
        RememberMeLabelDisplay = _loc.GetString("LoginDialogRememberMeLabelText");
        LoginButtonDisplay = _loc.GetString("LoginDialogLoginButtonText");
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        IsLoggingIn = true;
        HasError = false;
        ErrorMessage = string.Empty;

        try
        {
            var accounts = await _accountRepo.GetAllAsync();
            var account = accounts.FirstOrDefault(a =>
                a.Email.Equals(Email, StringComparison.OrdinalIgnoreCase) &&
                a.PasswordHash == Password);

            if (account != null)
            {
                LoggedInAccount = account;
                LoginSucceededInternal?.Invoke(account);
                CloseRequestedInternal?.Invoke();

                // Navigate to Dashboard
                _mainViewModel.IsNavigationVisible = true;
                _navigationService.Navigate(new NavigateToDashboard());

                // Toast notification
                await _notificationService.SendAsync(
                    _loc.GetString("LoginSuccessTitle"),
                    string.Format(_loc.GetString("LoginSuccessMessage"), account.Email),
                    NotificationType.Success);
            }
            else
            {
                HasError = true;
                ErrorMessage = _loc.GetString("LoginDialogErrorText");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync(ex.Message);
        }
        finally
        {
            IsLoggingIn = false;
        }
    }

    private bool CanLogin() =>
        !IsLoggingIn &&
        !string.IsNullOrWhiteSpace(Email) &&
        !string.IsNullOrWhiteSpace(Password);
}
