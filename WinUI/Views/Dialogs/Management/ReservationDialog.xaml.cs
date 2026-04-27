using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.Dialogs.Management;
using WinUI.Helpers;

namespace WinUI.Views.Dialogs.Management;

public sealed partial class ReservationDialog : ContentDialog
{
    private bool _isTemporarilyHiddenForConfirmation;
    private bool _isCleanedUp;

    public ReservationViewModel ViewModel { get; }

    public IconState HeaderIconState => ViewModel.Icon;

    public IconState CloseIconState { get; } = new() { Kind = IconKind.Close, Size = 16 };

    public IconState CalendarIconState { get; } = new() { Kind = IconKind.Calendar, Size = 20, AlwaysFilled = true };

    public IconState ClockIconState { get; } = new() { Kind = IconKind.Clock, Size = 20, AlwaysFilled = true };

    public ReservationDialog(ReservationViewModel viewModel, ReservationDialogRequest? request)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        ViewModel.Configure(request);
        DataContext = ViewModel;
        InitializeComponent();

        ViewModel.CloseRequested += HandleCloseRequested;
        ViewModel.DialogHideRequested += HandleDialogHideRequested;
        ViewModel.DialogShowRequested += HandleDialogShowRequested;
        Closed += HandleClosed;
    }

    private void HandleCloseRequested()
    {
        if (_isTemporarilyHiddenForConfirmation)
        {
            Cleanup();
            return;
        }

        Hide();
    }

    private void HandleDialogHideRequested()
    {
        _isTemporarilyHiddenForConfirmation = true;
        Hide();
    }

    private async void HandleDialogShowRequested()
    {
        _isTemporarilyHiddenForConfirmation = false;
        await ShowAsync();
    }

    private void HandleClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        if (_isTemporarilyHiddenForConfirmation)
        {
            return;
        }

        Cleanup();
    }

    private void Cleanup()
    {
        if (_isCleanedUp)
        {
            return;
        }

        _isCleanedUp = true;
        _isTemporarilyHiddenForConfirmation = false;
        Closed -= HandleClosed;
        ViewModel.CloseRequested -= HandleCloseRequested;
        ViewModel.DialogHideRequested -= HandleDialogHideRequested;
        ViewModel.DialogShowRequested -= HandleDialogShowRequested;
        ViewModel.Dispose();
    }

    private void HandleFlyoutOpened(object? sender, object e)
    {
        DispatcherQueue.TryEnqueue(() => DialogFlyoutButtonHelper.ApplyDarkAcceptDismissForeground(XamlRoot));
    }

    private void HandleDatePicked(DatePickerFlyout sender, DatePickedEventArgs args)
    {
        ViewModel.ApplyReservationDateSelection(args.NewDate);
    }

    private void HandleTimePicked(TimePickerFlyout sender, TimePickedEventArgs args)
    {
        ViewModel.ApplyReservationTimeSelection(args.NewTime);
    }
}
