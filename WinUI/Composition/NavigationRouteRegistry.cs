using Application.Navigation.Requests;
using Infrastructure.Services.Navigation;
using WinUI.Views.Pages;

namespace WinUI.Composition;

internal static class NavigationRouteRegistry
{
    public static void RegisterRoutes()
    {
        NavigationMap.Register<NavigateToStarting, StartingPage>();
        NavigationMap.Register<NavigateToDashboard, DashboardPage>();
        NavigationMap.Register<NavigateToAreaManagement, AreaManagementPage>();
        NavigationMap.Register<NavigateToGameManagement, GameManagementPage>();
        NavigationMap.Register<NavigateToProductManagement, ProductManagementPage>();
        NavigationMap.Register<NavigateToMemberManagement, MemberManagementPage>();
        NavigationMap.Register<NavigateToTransactionHistory, TransactionManagementPage>();
        NavigationMap.Register<NavigateToSettings, SettingsPage>();
    }
}
