using Microsoft.UI.Xaml.Controls;
using Application.Navigation;

namespace Infrastructure.Services.Navigation;

public class NavigationService : INavigationService
{
    private Frame? _frame;

    public void SetFrame(object frame)
    {
        if (frame is Frame f)
        {
            _frame = f;
        }
    }

    public void Navigate(INavigationRequest request)
    {
        var requestType = request.GetType();

        if (!NavigationMap.Map.TryGetValue(requestType, out var pageType))
        {
            throw new InvalidOperationException($"No mapping found for navigation request of type {requestType.Name}");
        }

        _frame?.Navigate(pageType);
    }
}
