using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.Dialogs;

namespace WinUI.Views.Dialogs;

public sealed partial class OtpDialog : ContentDialog
{
    public OtpViewModel ViewModel { get; }
    public IconState HeaderIconState { get; } = new() { Kind = IconKind.Password, Size = 24 };
    public IconState CloseIconState { get; } = new() { Kind = IconKind.Close, Size = 16 };

    public OtpDialog(OtpViewModel viewModel)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;
        InitializeComponent();

        ViewModel.CloseRequested += HandleCloseRequested;
        Closed += HandleClosed;
    }

    private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.NewPassword = NewPasswordBox.Password;
    }

    private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.ConfirmPassword = ConfirmPasswordBox.Password;
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
