using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI.ViewModels.AreaManagement.SummarizedAreaCards;
using WinUI.ViewModels.Pages;

namespace WinUI.Views.Pages;

public sealed partial class AreaManagementPage : Page
{
    private const double MinimumAreaCardOuterWidth = 188;
    private const double AreaCardOuterHeight = 168;
    private const double AreaCardsHorizontalPadding = 10;
    private const double AreaCardsVerticalPadding = 20;
    private const double ScrollBarOverlayCompensation = 20;

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

    private void HandleAreaCardsGridViewLoaded(object sender, RoutedEventArgs e)
    {
        UpdateAreaCardsLayout();
    }

    private void HandleAreaCardsGridViewSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateAreaCardsLayout();
    }

    private void HandleAreaCardsGridViewItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is ISummarizedAreaCardViewModel clickedAreaCardViewModel)
        {
            ViewModel.SelectSummarizedAreaCardCommand.Execute(clickedAreaCardViewModel);
        }
    }

    private void UpdateAreaCardsLayout()
    {
        if (AreaCardsGridView.ItemsPanelRoot is not ItemsWrapGrid itemsWrapGrid)
        {
            return;
        }

        var itemCount = AreaCardsGridView.Items.Count;
        var viewportHeight = AreaCardsGridView.ActualHeight - (AreaCardsVerticalPadding * 2);
        var availableWidth = AreaCardsGridView.ActualWidth - (AreaCardsHorizontalPadding * 2);

        if (viewportHeight <= 0 || availableWidth <= 0)
        {
            return;
        }

        var columns = CalculateColumnCount(availableWidth);
        var requiresVerticalScrollBar = RequiresVerticalScrollBar(itemCount, columns, viewportHeight);
        var rightPadding = AreaCardsHorizontalPadding + (requiresVerticalScrollBar ? ScrollBarOverlayCompensation : 0);
        var desiredPadding = new Thickness(
            AreaCardsHorizontalPadding,
            AreaCardsVerticalPadding,
            rightPadding,
            AreaCardsVerticalPadding);

        if (!AreaCardsGridView.Padding.Equals(desiredPadding))
        {
            AreaCardsGridView.Padding = desiredPadding;
        }

        availableWidth = AreaCardsGridView.ActualWidth - AreaCardsHorizontalPadding - rightPadding;
        if (availableWidth <= 0)
        {
            return;
        }

        columns = CalculateColumnCount(availableWidth);
        var itemWidth = Math.Floor(availableWidth / columns);

        itemsWrapGrid.ItemWidth = itemWidth;
        itemsWrapGrid.ItemHeight = AreaCardOuterHeight;
    }

    private static int CalculateColumnCount(double availableWidth)
    {
        return Math.Max(1, (int)Math.Floor(availableWidth / MinimumAreaCardOuterWidth));
    }

    private static bool RequiresVerticalScrollBar(int itemCount, int columns, double viewportHeight)
    {
        if (itemCount <= 0 || columns <= 0)
        {
            return false;
        }

        var rows = Math.Ceiling((double)itemCount / columns);
        return (rows * AreaCardOuterHeight) > viewportHeight;
    }
}
