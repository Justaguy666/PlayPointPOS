using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI.ViewModels.Pages;

namespace WinUI.Views.Pages;

public sealed partial class TransactionManagementPage : Page
{
    private const double TransactionsLeftPadding = 24;
    private const double TransactionsRightPadding = 24;

    public TransactionManagementPageViewModel ViewModel { get; }

    public TransactionManagementPage()
    {
        ViewModel = App.Host!.Services.GetRequiredService<TransactionManagementPageViewModel>();
        DataContext = ViewModel;
        InitializeComponent();

        Unloaded += HandleUnloaded;
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        Unloaded -= HandleUnloaded;
        ViewModel.Dispose();
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
        double availableWidth = TransactionsScrollViewer.ActualWidth - TransactionsLeftPadding - TransactionsRightPadding;
        if (availableWidth <= 0)
        {
            return;
        }

        ViewModel.UpdateTransactionCardsLayout(availableWidth);
    }
}
