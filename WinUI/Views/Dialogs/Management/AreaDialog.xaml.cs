using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WinUI.Converters;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.Dialogs.Management;

namespace WinUI.Views.Dialogs.Management;

public sealed partial class AreaDialog : ContentDialog
{
    private static readonly IconConverter IconConverter = new();
    private bool _isTemporarilyHiddenForConfirmation;
    private bool _isCleanedUp;

    public AreaDialogViewModel ViewModel { get; }

    public IconState HeaderIconState => ViewModel.Icon;

    public IconState CloseIconState { get; } = new() { Kind = IconKind.Close, Size = 16 };

    public AreaDialog(AreaDialogViewModel viewModel, AreaDialogRequest? request)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        ViewModel.Configure(request);
        DataContext = ViewModel;
        InitializeComponent();
        HeaderIcon.Data = ConvertIcon(HeaderIconState);
        CloseIcon.Data = ConvertIcon(CloseIconState);

        ViewModel.CloseRequested += HandleCloseRequested;
        ViewModel.DialogHideRequested += HandleDialogHideRequested;
        ViewModel.DialogShowRequested += HandleDialogShowRequested;
        Closed += HandleClosed;
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

    private void HandleCloseRequested()
    {
        if (_isTemporarilyHiddenForConfirmation)
        {
            Cleanup();
            return;
        }

        Hide();
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

    private static Geometry? ConvertIcon(IconState iconState)
    {
        return IconConverter.Convert(iconState, typeof(Geometry), null, string.Empty) as Geometry;
    }
}
