using System.Collections.ObjectModel;
using System.Linq;
using Application.Navigation;
using Application.Navigation.Requests;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinUI.UIModels;

namespace WinUI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    public partial bool IsNavigationVisible { get; set; } = true;

    // Header Properties (Dummy data for now)
    [ObservableProperty]
    public partial string TodayRevenue { get; set; } = "1.005.000 đ";

    [ObservableProperty]
    public partial string ActiveTables { get; set; } = "4/9";

    [ObservableProperty]
    public partial string ReservedTables { get; set; } = "2";

    // Header Localization Properties
    [ObservableProperty]
    public partial string TodayRevenueLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ActiveTablesLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ReservedTablesLabelDisplay { get; set; } = string.Empty;

    // Navigation Localization Properties
    [ObservableProperty]
    public partial string NavDashboardText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NavTableText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NavGameText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NavFoodText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NavMemberText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NavHistoryText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NavSettingsText { get; set; } = string.Empty;

    // Navbar Properties
    public ObservableCollection<NavigationItemModel> NavigationItems { get; } = new();

    public MainViewModel(INavigationService navigationService, ILocalizationService localizationService)
    {
        _navigationService = navigationService;
        _localizationService = localizationService;
        _localizationService.LanguageChanged += UpdateHeaderTexts;
        UpdateHeaderTexts();
        InitializeNavigationItems();
    }

    private void UpdateHeaderTexts()
    {
        TodayRevenueLabelDisplay = _localizationService.GetString("HeaderTodayRevenueLabel");
        ActiveTablesLabelDisplay = _localizationService.GetString("HeaderActiveTablesLabel");
        ReservedTablesLabelDisplay = _localizationService.GetString("HeaderReservedTablesLabel");

        NavDashboardText = _localizationService.GetString("NavDashboardText");
        NavTableText = _localizationService.GetString("NavTableText");
        NavGameText = _localizationService.GetString("NavGameText");
        NavFoodText = _localizationService.GetString("NavFoodText");
        NavMemberText = _localizationService.GetString("NavMemberText");
        NavHistoryText = _localizationService.GetString("NavHistoryText");
        NavSettingsText = _localizationService.GetString("NavSettingsText");

        // Refresh navigation items with new localized labels
        RefreshNavigationLabels();
    }

    private void RefreshNavigationLabels()
    {
        if (NavigationItems.Count == 0) return;

        NavigationItems[0].Label = NavDashboardText;
        NavigationItems[1].Label = NavTableText;
        NavigationItems[2].Label = NavGameText;
        NavigationItems[3].Label = NavFoodText;
        NavigationItems[4].Label = NavMemberText;
        NavigationItems[5].Label = NavHistoryText;
        NavigationItems[6].Label = NavSettingsText;
    }

    private void InitializeNavigationItems()
    {
        NavigationItems.Add(new NavigationItemModel { Label = NavDashboardText, IconGlyph = "\uE80F", RequestType = typeof(NavigateToDashboard) }); // Home icon
        NavigationItems.Add(new NavigationItemModel { Label = NavTableText, IconGlyph = "\uE87A", RequestType = typeof(NavigateToTableManagement) }); // Timeline icon
        NavigationItems.Add(new NavigationItemModel { Label = NavGameText, IconGlyph = "\uE71FB", RequestType = typeof(NavigateToGameManagement) }); // Game icon
        NavigationItems.Add(new NavigationItemModel { Label = NavFoodText, IconGlyph = "\uE7FC", RequestType = typeof(NavigateToFoodManagement) }); // Shop icon
        NavigationItems.Add(new NavigationItemModel { Label = NavMemberText, IconGlyph = "\uE716", RequestType = typeof(NavigateToMemberManagement) }); // Contact icon
        NavigationItems.Add(new NavigationItemModel { Label = NavHistoryText, IconGlyph = "\uE81C", RequestType = typeof(NavigateToTransactionHistory) }); // History icon
        NavigationItems.Add(new NavigationItemModel { Label = NavSettingsText, IconGlyph = "\uE713", RequestType = typeof(NavigateToSettings) }); // Settings icon
    }

    [RelayCommand]
    private void Navigate(NavigationItemModel item)
    {
        if (item == null) return;

        foreach (var navItem in NavigationItems)
        {
            navItem.IsSelected = (navItem == item);
        }

        var request = (INavigationRequest)System.Activator.CreateInstance(item.RequestType)!;
        _navigationService.Navigate(request);
    }
}
