using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI.Helpers;
using WinUI.Services.Layout;
using WinUI.ViewModels.Pages;

namespace WinUI.Views.Pages;

public sealed partial class ProductManagementPage : Page
{
    public ProductManagementPageViewModel ViewModel { get; }

    public ProductManagementPage(ProductManagementPageViewModel viewModel)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;
        InitializeComponent();

        SizeChanged += HandlePageSizeChanged;
        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
        ApplyResponsiveLayout();
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        SizeChanged -= HandlePageSizeChanged;
        Loaded -= HandleLoaded;
        Unloaded -= HandleUnloaded;
        ViewModel.Dispose();
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        ToolTipHelper.ApplyMissingToolTips(this);
    }

    private void HandlePageSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplyResponsiveLayout();
    }

    private void HandleProductsScrollViewerLoaded(object sender, RoutedEventArgs e)
    {
        UpdateProductsLayout();
    }

    private void HandleProductsScrollViewerSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateProductsLayout();
    }

    private void UpdateProductsLayout()
    {
        double horizontalPadding = ResponsiveBreakpoints.IsCompact(ActualWidth)
            ? ResponsiveBreakpoints.CompactPageHorizontalPadding * 2
            : ResponsiveBreakpoints.PageHorizontalPadding * 2;
        double availableWidth = ProductsScrollViewer.ActualWidth - horizontalPadding;
        if (availableWidth <= 0)
        {
            return;
        }

        ViewModel.UpdateGridProductsLayout(availableWidth);
    }

    private void ApplyResponsiveLayout()
    {
        bool isCompact = ResponsiveBreakpoints.IsCompact(ActualWidth);
        bool isToolbarCompact = ActualWidth < ResponsiveBreakpoints.WideMinWidth;
        double horizontalPadding = isCompact
            ? ResponsiveBreakpoints.CompactPageHorizontalPadding
            : ResponsiveBreakpoints.PageHorizontalPadding;

        ToolbarGrid.Padding = isCompact
            ? new Thickness(horizontalPadding, 14, horizontalPadding, 14)
            : new Thickness(horizontalPadding, 20, horizontalPadding, 20);
        ContentPanel.Padding = new Thickness(horizontalPadding, 20, horizontalPadding, 24);

        FilterButtonText.Visibility = isToolbarCompact ? Visibility.Collapsed : Visibility.Visible;
        AddButtonText.Visibility = isToolbarCompact ? Visibility.Collapsed : Visibility.Visible;

        FilterButton.MinWidth = isToolbarCompact ? 48 : 108;
        AddButton.MinWidth = isToolbarCompact ? 48 : 0;
        FilterButton.Padding = isToolbarCompact ? new Thickness(0) : new Thickness(14, 0, 14, 0);
        AddButton.Padding = isToolbarCompact ? new Thickness(0) : new Thickness(14, 0, 14, 0);

        Grid.SetRow(GridViewButton, 0);
        Grid.SetRow(ListViewButton, 0);
        Grid.SetRow(SearchBox, 0);

        if (isToolbarCompact)
        {
            SearchBox.Margin = new Thickness(12, 0, 0, 0);
            Grid.SetColumn(SearchBox, 2);
            Grid.SetColumnSpan(SearchBox, 3);

            Grid.SetRow(FilterButton, 1);
            Grid.SetColumn(FilterButton, 0);
            Grid.SetColumnSpan(FilterButton, 1);
            FilterButton.Margin = new Thickness(0, 12, 8, 0);

            Grid.SetRow(AddButton, 1);
            Grid.SetColumn(AddButton, 1);
            Grid.SetColumnSpan(AddButton, 1);
            AddButton.Margin = new Thickness(0, 12, 0, 0);

            Grid.SetRow(SortControlsPanel, 0);
            Grid.SetColumn(SortControlsPanel, 0);
            Grid.SetColumnSpan(SortControlsPanel, 2);

            Grid.SetRow(PageInfoBorder, 1);
            Grid.SetColumn(PageInfoBorder, 0);
            Grid.SetColumnSpan(PageInfoBorder, 2);
            PageInfoBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
            PageInfoBorder.Margin = new Thickness(0);
            return;
        }

        SearchBox.Margin = new Thickness(20, 0, 20, 0);
        Grid.SetColumn(SearchBox, 2);
        Grid.SetColumnSpan(SearchBox, 1);

        Grid.SetRow(FilterButton, 0);
        Grid.SetColumn(FilterButton, 3);
        Grid.SetColumnSpan(FilterButton, 1);
        FilterButton.Margin = new Thickness(0, 0, 10, 0);

        Grid.SetRow(AddButton, 0);
        Grid.SetColumn(AddButton, 4);
        Grid.SetColumnSpan(AddButton, 1);
        AddButton.Margin = new Thickness(0);

        Grid.SetRow(SortControlsPanel, 0);
        Grid.SetColumn(SortControlsPanel, 0);
        Grid.SetColumnSpan(SortControlsPanel, 1);

        Grid.SetRow(PageInfoBorder, 0);
        Grid.SetColumn(PageInfoBorder, 1);
        Grid.SetColumnSpan(PageInfoBorder, 1);
        PageInfoBorder.HorizontalAlignment = HorizontalAlignment.Right;
        PageInfoBorder.Margin = new Thickness(0);
    }
}
