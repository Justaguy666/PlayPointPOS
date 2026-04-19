using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.Dialogs.Management;

namespace WinUI.Views.Dialogs.Management;

public sealed partial class TransactionDetailDialog : ContentDialog
{
    public TransactionDetailDialogViewModel ViewModel { get; }

    public IconState HeaderIconState { get; } = new() { Kind = IconKind.History, Size = 20, AlwaysFilled = true };

    public IconState CloseIconState { get; } = new() { Kind = IconKind.Close, Size = 16 };

    public TransactionDetailDialog(
        TransactionDetailDialogViewModel viewModel,
        TransactionDetailDialogRequest? request)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;
        InitializeComponent();
        ViewModel.Configure(request);

        Closed += HandleClosed;
    }

    private void HandleCloseButtonClick(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void HandleClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        Closed -= HandleClosed;
        ViewModel.Dispose();
    }
}
