using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI.Helpers;
using WinUI.Services.Layout;
using WinUI.ViewModels.Pages;

namespace WinUI.Views.Pages;

public sealed partial class TransactionManagementPage : Page
{
    public TransactionManagementPageViewModel ViewModel { get; }

    public TransactionManagementPage()
    {
        ViewModel = App.Host!.Services.GetRequiredService<TransactionManagementPageViewModel>();
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

    private void HandleTransactionsScrollViewerLoaded(object sender, RoutedEventArgs e)
    {
        UpdateTransactionsLayout();
    }

    private void HandleTransactionsScrollViewerSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateTransactionsLayout();
    }

    private void UpdateTransactionsLayout()
    {
        double horizontalPadding = ResponsiveBreakpoints.IsCompact(ActualWidth)
            ? ResponsiveBreakpoints.CompactPageHorizontalPadding * 2
            : ResponsiveBreakpoints.PageHorizontalPadding * 2;
        double availableWidth = TransactionsScrollViewer.ActualWidth - horizontalPadding;
        if (availableWidth <= 0)
        {
            return;
        }

        ViewModel.UpdateTransactionCardsLayout(availableWidth);
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
        ContentGrid.Padding = new Thickness(horizontalPadding, 20, horizontalPadding, 24);

        FilterButtonText.Visibility = isToolbarCompact ? Visibility.Collapsed : Visibility.Visible;
        FilterButton.MinWidth = isToolbarCompact ? 48 : 108;
        FilterButton.Padding = isToolbarCompact ? new Thickness(0) : new Thickness(14, 0, 14, 0);
        SearchBox.Margin = isCompact
            ? new Thickness(0, 0, 12, 0)
            : new Thickness(0, 0, 20, 0);

        PageInfoBorder.HorizontalAlignment = isCompact
            ? HorizontalAlignment.Stretch
            : HorizontalAlignment.Right;
    }
}
