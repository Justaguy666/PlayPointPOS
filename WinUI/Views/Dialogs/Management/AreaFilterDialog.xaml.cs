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

    public AreaFilterViewModel ViewModel { get; }

    public IconState HeaderIconState => ViewModel.Icon;

    public IconState CloseIconState { get; } = new() { Kind = IconKind.Close, Size = 16 };

    public IconState ClockIconState { get; } = new() { Kind = IconKind.Clock, Size = 20, AlwaysFilled = true };

    public AreaFilterDialog(AreaFilterViewModel viewModel, AreaFilterDialogRequest? request)
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
        Closed -= HandleClosed;
        ViewModel.CloseRequested -= HandleCloseRequested;
        ViewModel.Dispose();
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

    private void HandleStartTimeFromPicked(TimePickerFlyout sender, TimePickedEventArgs args)
    {
        ViewModel.ApplyStartTimeFromSelection(args.NewTime);
    }

    private void HandleStartTimeToPicked(TimePickerFlyout sender, TimePickedEventArgs args)
    {
        ViewModel.ApplyStartTimeToSelection(args.NewTime);
    }
}
