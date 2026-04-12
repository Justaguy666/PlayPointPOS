using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using WinUI.Helpers;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.Dialogs.Management;

namespace WinUI.Views.Dialogs.Management;

public sealed partial class ReservationDialog : ContentDialog
{
    private static readonly SolidColorBrush DarkForegroundBrush = new(Windows.UI.Color.FromArgb(255, 31, 31, 31));

    private readonly DatePickerFlyout _datePickerFlyout;
    private readonly TimePickerFlyout _timePickerFlyout;
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
        _datePickerFlyout = CreateDatePickerFlyout();
        _timePickerFlyout = CreateTimePickerFlyout();
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

    private async void ReservationDateFieldButton_Click(object sender, RoutedEventArgs e)
    {
        DateTimeOffset minimumDateTime = ReservationDateTimeHelper.GetMinimumSelectableDateTime();
        DateTimeOffset selectedDate = ViewModel.ReservationDate ?? minimumDateTime;
        _datePickerFlyout.MinYear = ReservationDateTimeHelper.CreateDate(minimumDateTime);
        _datePickerFlyout.MaxYear = ReservationDateTimeHelper.CreateDate(minimumDateTime.AddYears(2));
        _datePickerFlyout.Date = ReservationDateTimeHelper.CoerceDate(selectedDate);
        await _datePickerFlyout.ShowAtAsync(ReservationDateFieldButton);
    }

    private async void ReservationTimeFieldButton_Click(object sender, RoutedEventArgs e)
    {
        DateTimeOffset minimumDateTime = ReservationDateTimeHelper.GetMinimumSelectableDateTime();
        DateTimeOffset selectedDate = ReservationDateTimeHelper.CoerceDate(ViewModel.ReservationDate ?? minimumDateTime);
        TimeSpan selectedTime = ViewModel.ReservationTime ?? ReservationDateTimeHelper.GetMinimumSelectableTime(selectedDate);
        _timePickerFlyout.Time = ReservationDateTimeHelper.CoerceTime(selectedDate, selectedTime);
        await _timePickerFlyout.ShowAtAsync(ReservationTimeFieldButton);
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
        _datePickerFlyout.DatePicked -= HandleDatePicked;
        _datePickerFlyout.Opened -= HandleFlyoutOpened;
        _timePickerFlyout.TimePicked -= HandleTimePicked;
        _timePickerFlyout.Opened -= HandleFlyoutOpened;
        ViewModel.Dispose();
    }

    private DatePickerFlyout CreateDatePickerFlyout()
    {
        DateTimeOffset minimumDateTime = ReservationDateTimeHelper.GetMinimumSelectableDateTime();
        var flyout = new DatePickerFlyout
        {
            Date = ReservationDateTimeHelper.CreateDate(minimumDateTime),
            MinYear = ReservationDateTimeHelper.CreateDate(minimumDateTime),
            MaxYear = ReservationDateTimeHelper.CreateDate(minimumDateTime.AddYears(2))
        };

        flyout.DatePicked += HandleDatePicked;
        flyout.Opened += HandleFlyoutOpened;
        return flyout;
    }

    private TimePickerFlyout CreateTimePickerFlyout()
    {
        DateTimeOffset minimumDateTime = ReservationDateTimeHelper.GetMinimumSelectableDateTime();
        var flyout = new TimePickerFlyout
        {
            Time = minimumDateTime.TimeOfDay,
            ClockIdentifier = "24HourClock",
            MinuteIncrement = ReservationDateTimeHelper.MinuteIncrement
        };

        flyout.TimePicked += HandleTimePicked;
        flyout.Opened += HandleFlyoutOpened;
        return flyout;
    }

    private void HandleFlyoutOpened(object? sender, object e)
    {
        DispatcherQueue.TryEnqueue(FixAcceptDismissButtonForeground);
    }

    private void FixAcceptDismissButtonForeground()
    {
        var popups = VisualTreeHelper.GetOpenPopupsForXamlRoot(XamlRoot);

        foreach (Popup popup in popups)
        {
            if (popup.Child == null)
            {
                continue;
            }

            var acceptButton = VisualTreeSearchHelper.FindDescendant<Button>(popup.Child, "AcceptButton");
            var dismissButton = VisualTreeSearchHelper.FindDescendant<Button>(popup.Child, "DismissButton");

            if (acceptButton is not null)
            {
                acceptButton.Foreground = DarkForegroundBrush;
            }

            if (dismissButton is not null)
            {
                dismissButton.Foreground = DarkForegroundBrush;
            }
        }
    }

    private void HandleDatePicked(DatePickerFlyout sender, DatePickedEventArgs args)
    {
        DateTimeOffset selectedDate = ReservationDateTimeHelper.CoerceDate(args.NewDate);
        ViewModel.ReservationDate = selectedDate;

        if (ViewModel.ReservationTime.HasValue)
        {
            ViewModel.ReservationTime = ReservationDateTimeHelper.CoerceTime(selectedDate, ViewModel.ReservationTime.Value);
        }
    }

    private void HandleTimePicked(TimePickerFlyout sender, TimePickedEventArgs args)
    {
        DateTimeOffset selectedDate = ReservationDateTimeHelper.CoerceDate(
            ViewModel.ReservationDate ?? ReservationDateTimeHelper.GetMinimumSelectableDateTime());

        if (!ViewModel.ReservationDate.HasValue)
        {
            ViewModel.ReservationDate = selectedDate;
        }

        ViewModel.ReservationTime = ReservationDateTimeHelper.CoerceTime(selectedDate, args.NewTime);
    }
}
