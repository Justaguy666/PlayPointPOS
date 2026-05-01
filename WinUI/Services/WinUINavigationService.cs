using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Application.Navigation;
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
    private readonly IReadOnlyDictionary<Type, INavigationRoute> _routesByRequestType;

    public WinUINavigationService(IEnumerable<INavigationRoute> routes)
    {
        ArgumentNullException.ThrowIfNull(routes);
        _routesByRequestType = routes.ToDictionary(route => route.RequestType);
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

        if (!_routesByRequestType.TryGetValue(requestType, out INavigationRoute? route))
        {
            throw new InvalidOperationException($"No mapping found for navigation request of type {requestType.Name}");
        }

        Page pageInstance = route.CreatePage();

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
