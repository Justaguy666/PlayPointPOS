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
        double horizontalPadding = isCompact
            ? ResponsiveBreakpoints.CompactPageHorizontalPadding
            : ResponsiveBreakpoints.PageHorizontalPadding;

        ContentPanel.Padding = new Thickness(horizontalPadding, 20, horizontalPadding, 24);
    }
}
