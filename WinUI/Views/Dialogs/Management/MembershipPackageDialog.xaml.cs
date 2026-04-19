using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.Dialogs.Management;

namespace WinUI.Views.Dialogs.Management;

public sealed partial class MembershipPackageDialog : ContentDialog
{
    private bool _isTemporarilyHiddenForConfirmation;
    private bool _isCleanedUp;

    public MembershipPackageDialogViewModel ViewModel { get; }

    public IconState HeaderIconState { get; } = new()
    {
        Kind = IconKind.Folder,
        Size = 20,
        AlwaysFilled = true,
    };

    public IconState CloseIconState { get; } = new()
    {
        Kind = IconKind.Close,
        Size = 10,
        AlwaysFilled = false,
    };

    public MembershipPackageDialog(MembershipPackageDialogViewModel viewModel, MembershipPackageDialogRequest request)
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

    private void OnMembershipPackageDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: MembershipPackageItemViewModel item }
            && item.BeginEditCommand.CanExecute(null))
        {
            item.BeginEditCommand.Execute(null);
        }
    }
}
