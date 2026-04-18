using System;
using Microsoft.UI.Xaml.Controls;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.Dialogs;

namespace WinUI.Views.Dialogs;

public sealed partial class RegisterDialog : ContentDialog
{
    public RegisterViewModel ViewModel { get; }
    public IconState HeaderIconState { get; } = new() { Kind = IconKind.Register, Size = 24 };
    public IconState CloseIconState { get; } = new() { Kind = IconKind.Close, Size = 16 };

    public RegisterDialog(RegisterViewModel viewModel)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
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
}
