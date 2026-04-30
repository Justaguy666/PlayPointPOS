using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI.Helpers;
using WinUI.Services.Layout;
using WinUI.ViewModels.Pages;

namespace WinUI.Views.Pages;

public sealed partial class TransactionManagementPage : Page
{
    public TransactionManagementPageViewModel ViewModel { get; }

    public TransactionManagementPage(TransactionManagementPageViewModel viewModel)
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
        double horizontalPadding = isCompact
            ? ResponsiveBreakpoints.CompactPageHorizontalPadding
            : ResponsiveBreakpoints.PageHorizontalPadding;

        ContentPanel.Padding = new Thickness(horizontalPadding, 20, horizontalPadding, 24);

        PageInfoChip.HorizontalAlignment = isCompact
            ? HorizontalAlignment.Stretch
            : HorizontalAlignment.Right;
    }
}
