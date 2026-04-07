using System;
using System.Threading.Tasks;
using Application.Navigation;
using Application.Navigation.Requests;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinUI.ViewModels;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.UserControls.Settings;

namespace WinUI.ViewModels.Pages;

public partial class SettingsPageViewModel : LocalizedViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private readonly IDisposable[] _ownedViewModels;
    private bool _isDisposed;

    [ObservableProperty]
    public partial string LogoutButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ChangePasswordButtonText { get; set; } = string.Empty;

    public IconState LogoutIconState { get; } = new() { Kind = IconKind.Logout, Size = 24 };

    public SettingsPageViewModel(
        ILocalizationService localizationService,
        IDialogService dialogService,
        INavigationService navigationService,
        ShopInformationCardControlViewModel shopInformationCardViewModel,
        GeneralSettingsCardControlViewModel generalSettingsCardViewModel)
        : base(localizationService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        ShopInformationCardViewModel = shopInformationCardViewModel ?? throw new ArgumentNullException(nameof(shopInformationCardViewModel));
        GeneralSettingsCardViewModel = generalSettingsCardViewModel ?? throw new ArgumentNullException(nameof(generalSettingsCardViewModel));

        _ownedViewModels =
        [
            ShopInformationCardViewModel,
            GeneralSettingsCardViewModel,
        ];

        LogoutCommand = new AsyncRelayCommand(OnLogout);
        ChangePasswordCommand = new AsyncRelayCommand(OnChangePassword);

        RefreshLocalizedText();
    }

    public ShopInformationCardControlViewModel ShopInformationCardViewModel { get; }

    public GeneralSettingsCardControlViewModel GeneralSettingsCardViewModel { get; }

    public IAsyncRelayCommand LogoutCommand { get; }

    public IAsyncRelayCommand ChangePasswordCommand { get; }

    protected override void RefreshLocalizedText()
    {
        LogoutButtonText = LocalizationService.GetString("SettingsPageLogoutButton");
        ChangePasswordButtonText = LocalizationService.GetString("SettingsPageChangePasswordButton");
    }

    public new void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        foreach (var viewModel in _ownedViewModels)
        {
            viewModel.Dispose();
        }

        base.Dispose();
    }

    private Task OnLogout()
    {
        _navigationService.Navigate(new NavigateToStarting());
        return Task.CompletedTask;
    }

    private Task OnChangePassword() => _dialogService.ShowDialogAsync("Otp");
}
