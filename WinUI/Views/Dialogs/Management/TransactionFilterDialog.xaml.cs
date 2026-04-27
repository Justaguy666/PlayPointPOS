using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.Dialogs.Management;
using WinUI.Helpers;

namespace WinUI.Views.Dialogs.Management;

public sealed partial class TransactionFilterDialog : ContentDialog
{
    public TransactionFilterDialogViewModel ViewModel { get; }

    public IconState HeaderIconState { get; } = new() { Kind = IconKind.Filter, Size = 20, AlwaysFilled = true };

    public IconState CloseIconState { get; } = new() { Kind = IconKind.Close, Size = 16 };

    public IconState CalendarIconState { get; } = new() { Kind = IconKind.Calendar, Size = 20, AlwaysFilled = true };

    public TransactionFilterDialog(
        TransactionFilterDialogViewModel viewModel,
        TransactionFilterDialogRequest? request)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        ViewModel.Configure(request);
        DataContext = ViewModel;
        InitializeComponent();

        ViewModel.CloseRequested += HandleCloseRequested;
        Closed += HandleClosed;
    }

    private void HandleCloseRequested()
    {
        Hide();
    }

    private void HandleClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        ViewModel.CloseRequested -= HandleCloseRequested;
        Closed -= HandleClosed;
        ViewModel.Dispose();
    }

    private void HandleFlyoutOpened(object? sender, object e)
    {
        DispatcherQueue.TryEnqueue(() => DialogFlyoutButtonHelper.ApplyDarkAcceptDismissForeground(XamlRoot));
    }

    private void HandleDateFromPicked(DatePickerFlyout sender, DatePickedEventArgs args)
    {
        ViewModel.ApplyDateFromSelection(args.NewDate);
    }

    private void HandleDateToPicked(DatePickerFlyout sender, DatePickedEventArgs args)
    {
        ViewModel.ApplyDateToSelection(args.NewDate);
    }
}
