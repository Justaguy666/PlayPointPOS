using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.Dialogs.Management;

namespace WinUI.Views.Dialogs.Management;

public sealed partial class MembershipPackageEditDialog : ContentDialog
{
    public MembershipPackageEditDialogViewModel ViewModel { get; }

    public IconState HeaderIconState { get; } = new()
    {
        Kind = IconKind.Update,
        Size = 20,
        AlwaysFilled = true,
    };

    public MembershipPackageEditDialog(MembershipPackageEditDialogViewModel viewModel, MembershipPackageEditDialogRequest request)
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
}
