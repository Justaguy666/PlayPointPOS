using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using WinUI.Helpers;
using WinUI.Services.Layout;
using WinUI.ViewModels.AreaManagement.SummarizedAreaCards;
using WinUI.ViewModels.Pages;

namespace WinUI.Views.Pages;

public sealed partial class AreaManagementPage : Page
{
    public AreaManagementPageViewModel ViewModel { get; }

    public AreaManagementPage(AreaManagementPageViewModel viewModel)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;
        InitializeComponent();

        SizeChanged += HandleSizeChanged;
        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
        ApplyResponsiveLayout();
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        SizeChanged -= HandleSizeChanged;
        Loaded -= HandleLoaded;
        Unloaded -= HandleUnloaded;
        ViewModel.Dispose();
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        ToolTipHelper.ApplyMissingToolTips(this);
    }

    private void HandleSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplyResponsiveLayout();
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

    private void EditSelectedArea_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (ViewModel.EditSelectedAreaCommand.CanExecute(null))
        {
            ViewModel.EditSelectedAreaCommand.Execute(null);
            args.Handled = true;
        }
    }

    private void DeleteSelectedArea_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (ViewModel.DeleteSelectedAreaCommand.CanExecute(null))
        {
            ViewModel.DeleteSelectedAreaCommand.Execute(null);
            args.Handled = true;
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

    private void HandleAreaContextActionClick(object sender, RoutedEventArgs e)
    {
        AreaCardActionsFlyout.Hide();
    }

    private void UpdateAreaCardsLayout()
    {
        Thickness padding = AreaCardsScrollViewer.Padding;
        var availableWidth = AreaCardsScrollViewer.ActualWidth - padding.Left - padding.Right;
        if (availableWidth <= 0)
        {
            return;
        }

        ViewModel.UpdateAreaCardsLayout(availableWidth);
    }

    private void ApplyResponsiveLayout()
    {
        bool isCompact = ResponsiveBreakpoints.IsCompact(ActualWidth);
        bool isHeaderCompact = ActualWidth < ResponsiveBreakpoints.WideMinWidth;

        RootGrid.ColumnDefinitions[0].Width = isCompact
            ? new GridLength(1, GridUnitType.Star)
            : new GridLength(4.3, GridUnitType.Star);
        RootGrid.ColumnDefinitions[1].Width = isCompact
            ? new GridLength(0)
            : new GridLength(5.7, GridUnitType.Star);
        RootGrid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
        RootGrid.RowDefinitions[1].Height = isCompact
            ? new GridLength(1, GridUnitType.Star)
            : new GridLength(0);

        Grid.SetRow(ListPane, 0);
        Grid.SetColumn(ListPane, 0);
        Grid.SetRow(EmptyDetailPane, isCompact ? 1 : 0);
        Grid.SetColumn(EmptyDetailPane, isCompact ? 0 : 1);
        Grid.SetRow(DetailPane, isCompact ? 1 : 0);
        Grid.SetColumn(DetailPane, isCompact ? 0 : 1);

        HeaderPane.Padding = isCompact ? new Thickness(16) : new Thickness(20);
        AreaCardsScrollViewer.Padding = isCompact
            ? new Thickness(ResponsiveBreakpoints.CompactPageHorizontalPadding, 16, ResponsiveBreakpoints.CompactPageHorizontalPadding, 16)
            : new Thickness(10, 20, 28, 20);

        ApplyHeaderLayout(isHeaderCompact);
        UpdateAreaCardsLayout();
    }

    private void ApplyHeaderLayout(bool isCompact)
    {
        Grid.SetRow(TypeFilterHost, 0);
        Grid.SetColumn(TypeFilterHost, 0);
        Grid.SetColumnSpan(TypeFilterHost, isCompact ? 3 : 1);
        TypeFilterHost.Margin = isCompact ? new Thickness(0, 0, 0, 8) : new Thickness(0);

        Grid.SetRow(AdvancedFilterButton, isCompact ? 1 : 0);
        Grid.SetColumn(AdvancedFilterButton, isCompact ? 0 : 2);
        AdvancedFilterButton.HorizontalAlignment = isCompact ? HorizontalAlignment.Left : HorizontalAlignment.Right;
        AdvancedFilterButton.Padding = isCompact ? new Thickness(10) : new Thickness(10);
        AdvancedFilterButton.MinWidth = isCompact ? 44 : 0;

        AddButtonText.Visibility = isCompact ? Visibility.Collapsed : Visibility.Visible;
        AdvancedFilterButtonText.Visibility = isCompact ? Visibility.Collapsed : Visibility.Visible;
        TableFilterTextBlock.Visibility = isCompact ? Visibility.Collapsed : Visibility.Visible;
        RoomFilterTextBlock.Visibility = isCompact ? Visibility.Collapsed : Visibility.Visible;
        AllAreasFilterTextBlock.MaxWidth = isCompact ? 96 : double.PositiveInfinity;
    }

    private static Border? TryGetAreaFilterHoverOverlay(Grid contentGrid)
    {
        return contentGrid.Children.Count > 1
            ? contentGrid.Children[1] as Border
            : null;
    }
}
