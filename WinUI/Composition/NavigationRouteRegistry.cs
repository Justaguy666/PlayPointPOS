using Application.Navigation.Requests;
using Microsoft.Extensions.DependencyInjection;
using WinUI.Services;
using WinUI.Views.Pages;

namespace WinUI.Composition;

internal static class NavigationRouteRegistry
{
    public static void AddNavigationRoutes(this IServiceCollection services)
    {
        services.AddSingleton<INavigationRoute>(sp => new NavigationRoute<NavigateToStarting>(() => sp.GetRequiredService<StartingPage>()));
        services.AddSingleton<INavigationRoute>(sp => new NavigationRoute<NavigateToDashboard>(() => sp.GetRequiredService<DashboardPage>()));
        services.AddSingleton<INavigationRoute>(sp => new NavigationRoute<NavigateToAreaManagement>(() => sp.GetRequiredService<AreaManagementPage>()));
        services.AddSingleton<INavigationRoute>(sp => new NavigationRoute<NavigateToGameManagement>(() => sp.GetRequiredService<GameManagementPage>()));
        services.AddSingleton<INavigationRoute>(sp => new NavigationRoute<NavigateToProductManagement>(() => sp.GetRequiredService<ProductManagementPage>()));
        services.AddSingleton<INavigationRoute>(sp => new NavigationRoute<NavigateToMemberManagement>(() => sp.GetRequiredService<MemberManagementPage>()));
        services.AddSingleton<INavigationRoute>(sp => new NavigationRoute<NavigateToTransactionHistory>(() => sp.GetRequiredService<TransactionManagementPage>()));
        services.AddSingleton<INavigationRoute>(sp => new NavigationRoute<NavigateToSettings>(() => sp.GetRequiredService<SettingsPage>()));
    }
}
