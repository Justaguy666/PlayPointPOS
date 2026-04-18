using System;
using Microsoft.UI.Xaml.Controls;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.Dialogs;

namespace WinUI.Views.Dialogs;

public sealed partial class LoginDialog : ContentDialog
{
    public LoginViewModel ViewModel { get; }
    public IconState HeaderIconState { get; } = new() { Kind = IconKind.Login, Size = 24 };
    public IconState CloseIconState { get; } = new() { Kind = IconKind.Close, Size = 16 };

    public LoginDialog(LoginViewModel viewModel)
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
