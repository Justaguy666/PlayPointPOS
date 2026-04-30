using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.Dialogs.Management;

namespace WinUI.Views.Dialogs.Management;

public sealed partial class TransactionDetailDialog : ContentDialog
{
    public TransactionDetailDialogViewModel ViewModel { get; }

    public IconState HeaderIconState { get; } = new() { Kind = IconKind.History, Size = 20, AlwaysFilled = true };

    public ICommand CloseCommand { get; }

    public TransactionDetailDialog(
        TransactionDetailDialogViewModel viewModel,
        TransactionDetailDialogRequest? request)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        CloseCommand = new RelayCommand(Hide);
        DataContext = ViewModel;
        InitializeComponent();
        ViewModel.Configure(request);

        Closed += HandleClosed;
    }

    private void HandleClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        Closed -= HandleClosed;
        ViewModel.Dispose();
    }
}
