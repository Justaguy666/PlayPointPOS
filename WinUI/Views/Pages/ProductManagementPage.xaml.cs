using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI.ViewModels.Pages;

namespace WinUI.Views.Pages;

public sealed partial class ProductManagementPage : Page
{
    private const double ProductsLeftPadding = 24;
    private const double ProductsRightPadding = 24;

    public ProductManagementPageViewModel ViewModel { get; }

    public ProductManagementPage(ProductManagementPageViewModel viewModel)
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
        double availableWidth = ProductsScrollViewer.ActualWidth - ProductsLeftPadding - ProductsRightPadding;
        if (availableWidth <= 0)
        {
            return;
        }

        ViewModel.UpdateGridProductsLayout(availableWidth);
    }
}
