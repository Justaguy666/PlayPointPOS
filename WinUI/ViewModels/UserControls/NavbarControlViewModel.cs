using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Application.Navigation;
using Application.Navigation.Requests;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinUI.UIModels;
using WinUI.UIModels.Enums;

namespace WinUI.ViewModels.UserControls;

public partial class NavbarControlViewModel : LocalizedViewModelBase
{
    private const double WideEnoughThreshold = 120.0;
    private const double ExpandedSidebarWidthValue = 248.0;
    private const double CollapsedSidebarWidthValue = 76.0;

    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    public ObservableCollection<NavbarItemModel> NavigationItems { get; } = [];

    [ObservableProperty]
    public partial double MinItemWidth { get; set; }

    [ObservableProperty]
    public partial bool IsWideEnough { get; set; } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SidebarWidth))]
    public partial bool IsSidebarExpanded { get; set; } = true;

    [ObservableProperty]
    public partial string LogoutButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ChangePasswordButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ToggleSidebarToolTipText { get; set; } = string.Empty;

    public double SidebarWidth => IsSidebarExpanded ? ExpandedSidebarWidthValue : CollapsedSidebarWidthValue;

    public IconState ToggleSidebarIconState { get; } = new() { Kind = IconKind.List, Size = 20 };

    public IconState ChangePasswordIconState { get; } = new() { Kind = IconKind.Password, Size = 20 };

    public IconState LogoutIconState { get; } = new() { Kind = IconKind.Logout, Size = 20 };

    public IRelayCommand<NavbarItemModel> NavigateCommand { get; }

    public IRelayCommand ToggleSidebarCommand { get; }

    public IAsyncRelayCommand LogoutCommand { get; }

    public IAsyncRelayCommand ChangePasswordCommand { get; }

    public NavbarControlViewModel(
        INavigationService navigationService,
        IDialogService dialogService,
        ILocalizationService localizationService)
        : base(localizationService)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        NavigateCommand = new RelayCommand<NavbarItemModel>(Navigate);
        ToggleSidebarCommand = new RelayCommand(ToggleSidebar);
        LogoutCommand = new AsyncRelayCommand(OnLogout);
        ChangePasswordCommand = new AsyncRelayCommand(OnChangePassword);

        InitializeNavigationItems();
        RefreshLocalizedText();
    }

    protected override void RefreshLocalizedText()
    {
        foreach (var navItem in NavigationItems)
        {
            navItem.Label = LocalizationService.GetString(navItem.LabelResourceKey);
        }

        LogoutButtonText = LocalizationService.GetString("SettingsPageLogoutButton");
        ChangePasswordButtonText = LocalizationService.GetString("SettingsPageChangePasswordButton");
        ToggleSidebarToolTipText = LocalizationService.GetString("NavigationToggleSidebarToolTip");
    }

    public void SelectNavigationItem(Type? requestType)
    {
        foreach (var navItem in NavigationItems)
        {
            navItem.IsSelected = requestType != null && navItem.RequestType == requestType;
        }
    }

    public void UpdateLayout(double availableWidth)
    {
        if (NavigationItems.Count == 0 || availableWidth <= 0)
        {
            return;
        }

        double itemWidth = Math.Floor(availableWidth / NavigationItems.Count);
        MinItemWidth = itemWidth;
        IsWideEnough = itemWidth >= WideEnoughThreshold;
    }

    private void InitializeNavigationItems()
    {
        NavigationItems.Add(CreateItem(IconKind.Dashboard, typeof(NavigateToDashboard), "NavDashboardLabel"));
        NavigationItems.Add(CreateItem(IconKind.Area, typeof(NavigateToAreaManagement), "NavAreaLabel"));
        NavigationItems.Add(CreateItem(IconKind.Game, typeof(NavigateToGameManagement), "NavGameLabel"));
        NavigationItems.Add(CreateItem(IconKind.Product, typeof(NavigateToProductManagement), "NavProductLabel"));
        NavigationItems.Add(CreateItem(IconKind.Member, typeof(NavigateToMemberManagement), "NavMemberLabel"));
        NavigationItems.Add(CreateItem(IconKind.History, typeof(NavigateToTransactionHistory), "NavHistoryLabel"));
        NavigationItems.Add(CreateItem(IconKind.Settings, typeof(NavigateToSettings), "NavSettingsLabel"));
    }

    private NavbarItemModel CreateItem(IconKind icon, Type requestType, string labelResourceKey)
    {
        return new NavbarItemModel
        {
            Icon = icon,
            RequestType = requestType,
            LabelResourceKey = labelResourceKey,
            Label = string.Empty,
        };
    }

    private void Navigate(NavbarItemModel? item)
    {
        if (item == null)
            return;

        SelectNavigationItem(item.RequestType);
        var request = (INavigationRequest)Activator.CreateInstance(item.RequestType)!;
        _navigationService.Navigate(request);
    }

    private void ToggleSidebar()
    {
        IsSidebarExpanded = !IsSidebarExpanded;
    }

    private async Task OnLogout()
    {
        bool isConfirmed = await _dialogService.ShowConfirmationAsync(
            titleKey: "ConfirmLogoutTitle",
            messageKey: "ConfirmLogoutMessage",
            confirmButtonTextKey: "ConfirmLogoutButton",
            cancelButtonTextKey: "CancelButtonText"
        );

        if (isConfirmed)
        {
            _navigationService.Navigate(new NavigateToStarting());
        }
    }

    private async Task OnChangePassword()
    {
        await _dialogService.ShowDialogAsync("Otp");
    }
}
