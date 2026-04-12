using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using WinUI.ViewModels.AreaManagement.SummarizedAreaCards;
using WinUI.ViewModels.Pages;

namespace WinUI.Views.Pages;

public sealed partial class AreaManagementPage : Page
{
    private const int PreferredAreaCardsPerRow = 3;
    private const double MinimumSingleColumnWidth = 240;
    private const double MinimumTwoColumnWidth = 420;
    private const double AreaCardOuterHeight = 168;
    private const double AreaCardsLeftPadding = 10;
    private const double AreaCardsRightPadding = 28;
    private const double AreaCardsColumnSpacing = 8;
    private const double LayoutPrecisionEpsilon = 0.01;
    private ISummarizedAreaCardViewModel? _contextMenuAreaCardViewModel;

    public AreaManagementPageViewModel ViewModel { get; }

    public AreaManagementPage(AreaManagementPageViewModel viewModel)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;
        InitializeComponent();

        Unloaded += HandleUnloaded;
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        Unloaded -= HandleUnloaded;
        ViewModel.Dispose();
    }

    private void HandleAreaCardsScrollViewerLoaded(object sender, RoutedEventArgs e)
    {
        UpdateAreaCardsLayout();
    }

    private void HandleAreaCardsScrollViewerSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateAreaCardsLayout();
    }

    private void HandleAreaCardTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement element &&
            element.DataContext is ISummarizedAreaCardViewModel clickedAreaCardViewModel)
        {
            ViewModel.SelectSummarizedAreaCardCommand.Execute(clickedAreaCardViewModel);
        }
    }

    private void HandleAreaCardRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        if (sender is not FrameworkElement areaCardElement ||
            areaCardElement.DataContext is not ISummarizedAreaCardViewModel areaCardViewModel)
        {
            return;
        }

        if (areaCardViewModel.Status != Domain.Enums.PlayAreaStatus.Available)
        {
            return;
        }

        _contextMenuAreaCardViewModel = areaCardViewModel;
        ViewModel.SelectSummarizedAreaCardCommand.Execute(areaCardViewModel);

        AreaCardActionsFlyout.ShowAt(
            areaCardElement,
            new FlyoutShowOptions
            {
                Position = e.GetPosition(areaCardElement),
            });

        e.Handled = true;
    }

    private void HandleAreaFilterChipPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Button { Tag: bool isSelected, Content: Grid contentGrid } || isSelected)
        {
            return;
        }

        if (TryGetAreaFilterHoverOverlay(contentGrid) is Border hoverOverlay)
        {
            hoverOverlay.Opacity = 1;
        }
    }

    private void HandleAreaFilterChipPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Button { Content: Grid contentGrid })
        {
            return;
        }

        if (TryGetAreaFilterHoverOverlay(contentGrid) is Border hoverOverlay)
        {
            hoverOverlay.Opacity = 0;
        }
    }

    private void HandleAreaFilterChipPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Button { Content: Grid contentGrid })
        {
            return;
        }

        if (TryGetAreaFilterHoverOverlay(contentGrid) is Border hoverOverlay)
        {
            hoverOverlay.Opacity = 0;
        }
    }

    private void HandleEditAreaContextButtonClick(object sender, RoutedEventArgs e)
    {
        ViewModel.EditAreaCommand.Execute(_contextMenuAreaCardViewModel);
        AreaCardActionsFlyout.Hide();
    }

    private void HandleDeleteAreaContextButtonClick(object sender, RoutedEventArgs e)
    {
        ViewModel.DeleteAreaCommand.Execute(_contextMenuAreaCardViewModel);
        AreaCardActionsFlyout.Hide();
    }

    private void HandleEditAreaKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (ViewModel.SelectedSummarizedAreaCardViewModel is null)
        {
            return;
        }

        ViewModel.EditAreaCommand.Execute(ViewModel.SelectedSummarizedAreaCardViewModel);
        args.Handled = true;
    }

    private void HandleDeleteAreaKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (ViewModel.SelectedSummarizedAreaCardViewModel is null)
        {
            return;
        }

        ViewModel.DeleteAreaCommand.Execute(ViewModel.SelectedSummarizedAreaCardViewModel);
        args.Handled = true;
    }

    private void UpdateAreaCardsLayout()
    {
        var availableWidth = AreaCardsScrollViewer.ActualWidth - AreaCardsLeftPadding - AreaCardsRightPadding;
        if (availableWidth <= 0)
        {
            return;
        }

        var columns = CalculateColumnCount(availableWidth);
        var totalSpacing = AreaCardsColumnSpacing * (columns - 1);
        var itemWidth = Math.Floor((availableWidth - totalSpacing + LayoutPrecisionEpsilon) / columns);

        AreaCardsGridLayout.MaximumRowsOrColumns = columns;
        AreaCardsGridLayout.MinItemWidth = itemWidth;
        AreaCardsGridLayout.MinItemHeight = AreaCardOuterHeight;
    }

    private static int CalculateColumnCount(double availableWidth)
    {
        if (availableWidth < MinimumSingleColumnWidth)
        {
            return 1;
        }

        if (availableWidth < MinimumTwoColumnWidth)
        {
            return 2;
        }

        return PreferredAreaCardsPerRow;
    }

    private static Border? TryGetAreaFilterHoverOverlay(Grid contentGrid)
    {
        return contentGrid.Children.Count > 1
            ? contentGrid.Children[1] as Border
            : null;
    }
}
