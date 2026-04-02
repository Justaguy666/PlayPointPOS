using System;
using Microsoft.UI.Xaml.Controls;
using Application.Navigation;
using Infrastructure.Services.Navigation;
using WinUI.ViewModels;
using WinUI.ViewModels.UserControls;
using WinUI.Views.Pages;

namespace WinUI.Services;

/// <summary>
/// WinUI implementation of navigation service.
/// Depends on Microsoft.UI.Xaml.Controls.Frame (platform-specific).
/// </summary>
public class WinUINavigationService : INavigationService
{
    private Frame? _frame;
    private MainViewModel? _mainViewModel;
    private NavbarControlViewModel? _navbarViewModel;
    private readonly IServiceProvider _serviceProvider;

    public WinUINavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public void SetFrame(object frame)
    {
        if (frame is Frame f)
        {
            _frame = f;
        }
    }

    public void SetShellViewModels(MainViewModel mainViewModel, NavbarControlViewModel navbarViewModel)
    {
        _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
        _navbarViewModel = navbarViewModel ?? throw new ArgumentNullException(nameof(navbarViewModel));
    }

    public void Navigate(INavigationRequest request)
    {
        var requestType = request.GetType();

        if (!NavigationMap.Map.TryGetValue(requestType, out var pageType))
        {
            throw new InvalidOperationException($"No mapping found for navigation request of type {requestType.Name}");
        }

        // Create page instance using DI container to support constructor injection
        var pageInstance = (Page?)_serviceProvider.GetService(pageType)
            ?? throw new InvalidOperationException($"Failed to create instance of page type {pageType.Name}");

        // Set Frame.Content directly (avoids XAML instantiation issues with DI constructors)
        if (_frame != null)
        {
            _frame.Content = pageInstance;

            if (_mainViewModel != null)
            {
                _mainViewModel.IsNavigationVisible = pageInstance is not StartingPage;
            }

            _navbarViewModel?.SelectNavigationItem(requestType);
        }
    }
}
