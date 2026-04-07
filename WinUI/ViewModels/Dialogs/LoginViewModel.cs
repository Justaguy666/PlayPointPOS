using System;
using System.Threading.Tasks;
using Application.Navigation;
using Application.Navigation.Requests;
using Application.Services;
using Application.UseCases.Auth;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Entities;
using WinUI.ViewModels;

namespace WinUI.ViewModels.Dialogs;

public partial class LoginViewModel : LocalizedViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly LoginUserUseCase _loginUseCase;
    private readonly INavigationService _navigationService;
    private readonly INotificationService _notificationService;
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
        LoginUserUseCase loginUseCase,
        INavigationService navigationService,
        INotificationService notificationService,
        MainViewModel mainViewModel)
        : base(localizationService)
    {
        _dialogService = dialogService;
        _loginUseCase = loginUseCase;
        _navigationService = navigationService;
        _notificationService = notificationService;
        _mainViewModel = mainViewModel;

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
            var result = await _loginUseCase.ExecuteAsync(Email, Password);

            if (result.Success)
            {
                LoggedInAccount = result.Account;
                LoginSucceededInternal?.Invoke(result.Account!);
                CloseRequestedInternal?.Invoke();

                // Navigate to Dashboard
                _mainViewModel.IsNavigationVisible = true;
                _navigationService.Navigate(new NavigateToDashboard());

                // Toast notification
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
