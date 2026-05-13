using System;
using System.Threading.Tasks;
using Application.Navigation;
using Application.Navigation.Requests;
using Application.Services;
using Application.UseCases.Auth.Contracts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Entities;
using WinUI.ViewModels;

namespace WinUI.ViewModels.Dialogs;

public partial class LoginViewModel : LocalizedViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IAuthApiService _authApiService;
    private readonly IAuthStateService _authStateService;
    private readonly IConfigurationService _configurationService;
    private readonly INavigationService _navigationService;
    private readonly INotificationService _notificationService;
    private readonly IManagementDataPreloadService _managementDataPreloadService;
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty]
    public partial string TitleText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EmailLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EmailPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    [NotifyPropertyChangedFor(nameof(CanLoginExecute))]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PasswordLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PasswordPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    [NotifyPropertyChangedFor(nameof(CanLoginExecute))]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool RememberMe { get; set; }

    [ObservableProperty]
    public partial string RememberMeLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LoginButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ForgotPasswordButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CloseTooltipText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    [NotifyCanExecuteChangedFor(nameof(ForgotPasswordCommand))]
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
        ILocalizationService localizationService,
        IDialogService dialogService,
        IAuthApiService authApiService,
        IAuthStateService authStateService,
        IConfigurationService configurationService,
        INavigationService navigationService,
        INotificationService notificationService,
        IManagementDataPreloadService managementDataPreloadService,
        MainViewModel mainViewModel)
        : base(localizationService)
    {
        _dialogService = dialogService;
        _authApiService = authApiService;
        _authStateService = authStateService;
        _configurationService = configurationService;
        _navigationService = navigationService;
        _notificationService = notificationService;
        _managementDataPreloadService = managementDataPreloadService;
        _mainViewModel = mainViewModel;

        Email = _configurationService.RememberedEmail;
        RememberMe = _configurationService.RememberMe;
        RefreshLocalizedText();
    }

    protected override void RefreshLocalizedText()
    {
        TitleText = LocalizationService.GetString("LoginDialogTitleText");
        EmailLabelText = LocalizationService.GetString("LoginDialogEmailLabelText");
        EmailPlaceholderText = LocalizationService.GetString("LoginDialogEmailPlaceholderText");
        PasswordLabelText = LocalizationService.GetString("LoginDialogPasswordLabelText");
        PasswordPlaceholderText = LocalizationService.GetString("LoginDialogPasswordPlaceholderText");
        RememberMeLabelText = LocalizationService.GetString("LoginDialogRememberMeLabelText");
        LoginButtonText = LocalizationService.GetString("LoginDialogLoginButtonText");
        ForgotPasswordButtonText = LocalizationService.GetString("LoginDialogForgotPasswordButtonText");
        CloseTooltipText = LocalizationService.GetString("CloseTooltipText");
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        IsLoggingIn = true;
        HasError = false;
        ErrorMessage = string.Empty;

        try
        {
            LoginResult result = await _authApiService.LoginAsync(Email.Trim(), Password);

            if (result.Success)
            {
                await _configurationService.SaveRememberedLoginAsync(Email.Trim(), RememberMe);
                LoggedInAccount = result.Account;
                _managementDataPreloadService.Clear();
                _authStateService.ShopId = result.Account?.Id;
                LoginSucceededInternal?.Invoke(result.Account!);
                CloseRequestedInternal?.Invoke();

                _mainViewModel.IsNavigationVisible = true;
                _navigationService.Navigate(new NavigateToDashboard());
                _ = _managementDataPreloadService.WarmUpAsync();

                await _notificationService.SendAsync(
                    LocalizationService.GetString("LoginSuccessTitle"),
                    string.Format(LocalizationService.GetString("LoginSuccessMessage"), result.Account!.Email),
                    NotificationType.Success);
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Message ?? LocalizationService.GetString("LoginDialogErrorText");
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

    [RelayCommand(CanExecute = nameof(CanOpenForgotPassword))]
    private async Task ForgotPasswordAsync()
    {
        CloseRequestedInternal?.Invoke();
        await Task.Yield();
        await _dialogService.ShowDialogAsync("ForgotPassword");
    }

    private bool CanOpenForgotPassword() => !IsLoggingIn;

    [RelayCommand]
    private void Close()
    {
        CloseRequestedInternal?.Invoke();
    }
}
