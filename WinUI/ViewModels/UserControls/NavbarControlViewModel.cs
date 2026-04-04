using System;
using System.Collections.ObjectModel;
using Application.Navigation;
using Application.Navigation.Requests;
using Application.Services;
using CommunityToolkit.Mvvm.Input;
using WinUI.UIModels;
using WinUI.UIModels.Enums;

namespace WinUI.ViewModels.UserControls;

public partial class NavbarControlViewModel : LocalizedViewModelBase
{
    private readonly INavigationService _navigationService;

    public ObservableCollection<NavbarItemModel> NavigationItems { get; } = [];

    public IRelayCommand<NavbarItemModel> NavigateCommand { get; }

    public NavbarControlViewModel(INavigationService navigationService, ILocalizationService localizationService)
        : base(localizationService)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        NavigateCommand = new RelayCommand<NavbarItemModel>(Navigate);

        InitializeNavigationItems();
        RefreshLocalizedText();
    }

    protected override void RefreshLocalizedText()
    {
        foreach (var navItem in NavigationItems)
        {
            navItem.Label = LocalizationService.GetString(navItem.ResourceKey);
        }
    }

    public void SelectNavigationItem(Type? requestType)
    {
        foreach (var navItem in NavigationItems)
        {
            navItem.IsSelected = requestType != null && navItem.RequestType == requestType;
        }
    }

    private void InitializeNavigationItems()
    {
        NavigationItems.Add(CreateItem(IconKind.Dashboard, typeof(NavigateToDashboard), "NavDashboardText"));
        NavigationItems.Add(CreateItem(IconKind.Area, typeof(NavigateToAreaManagement), "NavAreaText"));
        NavigationItems.Add(CreateItem(IconKind.Game, typeof(NavigateToGameManagement), "NavGameText"));
        NavigationItems.Add(CreateItem(IconKind.Product, typeof(NavigateToProductManagement), "NavFoodText"));
        NavigationItems.Add(CreateItem(IconKind.Member, typeof(NavigateToMemberManagement), "NavMemberText"));
        NavigationItems.Add(CreateItem(IconKind.History, typeof(NavigateToTransactionHistory), "NavHistoryText"));
        NavigationItems.Add(CreateItem(IconKind.Settings, typeof(NavigateToSettings), "NavSettingsText"));
    }

    private NavbarItemModel CreateItem(IconKind icon, Type requestType, string resourceKey)
    {
        return new NavbarItemModel
        {
            Icon = icon,
            RequestType = requestType,
            ResourceKey = resourceKey,
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
}
