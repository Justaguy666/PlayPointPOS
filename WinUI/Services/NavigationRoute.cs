using System;
using Application.Navigation;
using Microsoft.UI.Xaml.Controls;

namespace WinUI.Services;

public interface INavigationRoute
{
    Type RequestType { get; }

    Page CreatePage();
}

public sealed class NavigationRoute<TRequest> : INavigationRoute
    where TRequest : INavigationRequest
{
    private readonly Func<Page> _pageFactory;

    public NavigationRoute(Func<Page> pageFactory)
    {
        _pageFactory = pageFactory ?? throw new ArgumentNullException(nameof(pageFactory));
    }

    public Type RequestType => typeof(TRequest);

    public Page CreatePage() => _pageFactory();
}
