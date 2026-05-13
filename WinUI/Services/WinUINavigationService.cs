using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Application.Navigation;
using Application.Navigation.Requests;
using Application.Services;
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
    private readonly IDialogService? _dialogService;

    public WinUINavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _dialogService = _serviceProvider.GetService(typeof(IDialogService)) as IDialogService;
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
        try
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
                    if (_mainViewModel.IsNavigationVisible)
                    {
                        _ = _mainViewModel.RefreshHeaderMetricsAsync();
                    }
                }

                _navbarViewModel?.SelectNavigationItem(requestType);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Navigation failed for {request.GetType().Name}: {ex}");
            _ = ShowNavigationErrorAsync(request, ex);
        }
    }

    private async Task ShowNavigationErrorAsync(INavigationRequest request, Exception exception)
    {
        if (_dialogService is null)
        {
            return;
        }

        string message = BuildFriendlyNavigationErrorMessage(request, exception);
        await _dialogService.ShowErrorAsync(message);
    }

    private static string BuildFriendlyNavigationErrorMessage(INavigationRequest request, Exception exception)
    {
        string pageName = GetFriendlyPageName(request);
        string rawMessage = exception.GetBaseException().Message;
        string normalized = rawMessage.Trim();

        string summary = normalized switch
        {
            _ when normalized.Contains("No authenticated shop is available", StringComparison.OrdinalIgnoreCase)
                => "Phien dang nhap hien tai khong hop le. Vui long dang nhap lai.",
            _ when normalized.Contains("relation", StringComparison.OrdinalIgnoreCase)
                && normalized.Contains("does not exist", StringComparison.OrdinalIgnoreCase)
                => "Du lieu cho chuc nang nay chua duoc tao day du trong database.",
            _ when normalized.Contains("returned no data", StringComparison.OrdinalIgnoreCase)
                => "May chu da phan hoi nhung khong tra ve du lieu hop le.",
            _ when normalized.Contains("request failed with status", StringComparison.OrdinalIgnoreCase)
                => "Khong the ket noi hoac xu ly yeu cau tu may chu.",
            _ when normalized.Contains("Failed to create instance of page type", StringComparison.OrdinalIgnoreCase)
                => "Khong the khoi tao man hinh nay do loi cau hinh ung dung.",
            _ when normalized.Contains("No mapping found for navigation request", StringComparison.OrdinalIgnoreCase)
                => "Khong tim thay man hinh tuong ung cho thao tac dieu huong.",
            _ => "Da xay ra loi khi mo man hinh nay.",
        };

        return
            $"Khong the mo tab {pageName}.{Environment.NewLine}{Environment.NewLine}" +
            $"{summary}{Environment.NewLine}{Environment.NewLine}" +
            $"Chi tiet ky thuat:{Environment.NewLine}{normalized}";
    }

    private static string GetFriendlyPageName(INavigationRequest request)
    {
        return request switch
        {
            NavigateToDashboard => "Tong quan",
            NavigateToAreaManagement => "Khu vuc",
            NavigateToGameManagement => "Tro choi",
            NavigateToProductManagement => "San pham",
            NavigateToMemberManagement => "Thanh vien",
            NavigateToTransactionHistory => "Lich su giao dich",
            NavigateToSettings => "Cai dat",
            NavigateToStarting => "Bat dau",
            _ => request.GetType().Name,
        };
    }
}
