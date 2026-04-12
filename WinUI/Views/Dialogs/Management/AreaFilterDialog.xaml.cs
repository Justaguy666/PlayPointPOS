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

public sealed partial class AreaFilterDialog : ContentDialog
{
    private static readonly SolidColorBrush DarkForegroundBrush = new(Windows.UI.Color.FromArgb(255, 31, 31, 31));

    private readonly TimePickerFlyout _startTimeFromFlyout;
    private readonly TimePickerFlyout _startTimeToFlyout;

    public AreaFilterViewModel ViewModel { get; }

    public IconState HeaderIconState => ViewModel.Icon;

    public IconState CloseIconState { get; } = new() { Kind = IconKind.Close, Size = 16 };

    public IconState ClockIconState { get; } = new() { Kind = IconKind.Clock, Size = 20, AlwaysFilled = true };

    public AreaFilterDialog(AreaFilterViewModel viewModel, AreaFilterDialogRequest? request)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        ViewModel.Configure(request);
        _startTimeFromFlyout = CreateTimePickerFlyout();
        _startTimeToFlyout = CreateTimePickerFlyout();
        DataContext = ViewModel;
        InitializeComponent();

        ViewModel.CloseRequested += HandleCloseRequested;
        Closed += HandleClosed;
    }

    private void HandleCloseRequested()
    {
        Hide();
    }

    private async void StartTimeFromFieldButton_Click(object sender, RoutedEventArgs e)
    {
        _startTimeFromFlyout.Time = CoerceFilterTime(ViewModel.StartTimeFrom ?? TimeSpan.Zero);
        await _startTimeFromFlyout.ShowAtAsync(StartTimeFromFieldButton);
    }

    private async void StartTimeToFieldButton_Click(object sender, RoutedEventArgs e)
    {
        _startTimeToFlyout.Time = CoerceFilterTime(ViewModel.StartTimeTo ?? new TimeSpan(23, 55, 0));
        await _startTimeToFlyout.ShowAtAsync(StartTimeToFieldButton);
    }

    private void HandleClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        Closed -= HandleClosed;
        _startTimeFromFlyout.TimePicked -= HandleTimePicked;
        _startTimeFromFlyout.Opened -= HandleFlyoutOpened;
        _startTimeToFlyout.TimePicked -= HandleTimePicked;
        _startTimeToFlyout.Opened -= HandleFlyoutOpened;
        ViewModel.CloseRequested -= HandleCloseRequested;
        ViewModel.Dispose();
    }

    private TimePickerFlyout CreateTimePickerFlyout()
    {
        var flyout = new TimePickerFlyout
        {
            Time = TimeSpan.Zero,
            ClockIdentifier = "24HourClock",
            MinuteIncrement = ReservationDateTimeHelper.MinuteIncrement,
        };

        flyout.Opened += HandleFlyoutOpened;
        flyout.TimePicked += HandleTimePicked;
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

    private void HandleTimePicked(TimePickerFlyout sender, TimePickedEventArgs args)
    {
        if (ReferenceEquals(sender, _startTimeFromFlyout))
        {
            HandleStartTimeFromPicked(sender, args);
            return;
        }

        HandleStartTimeToPicked(sender, args);
    }

    private void HandleStartTimeFromPicked(TimePickerFlyout sender, TimePickedEventArgs args)
    {
        ViewModel.StartTimeFrom = CoerceFilterTime(args.NewTime);
    }

    private void HandleStartTimeToPicked(TimePickerFlyout sender, TimePickedEventArgs args)
    {
        ViewModel.StartTimeTo = CoerceFilterTime(args.NewTime);
    }

    private static TimeSpan CoerceFilterTime(TimeSpan selectedTime)
    {
        int minuteIncrement = ReservationDateTimeHelper.MinuteIncrement;
        int totalMinutes = (int)Math.Round(selectedTime.TotalMinutes / minuteIncrement, MidpointRounding.AwayFromZero) * minuteIncrement;
        totalMinutes = Math.Clamp(totalMinutes, 0, (24 * 60) - minuteIncrement);
        return TimeSpan.FromMinutes(totalMinutes);
    }
}
