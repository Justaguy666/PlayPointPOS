using System;
using Microsoft.UI.Xaml.Controls;
using WinUI.UIModels;
using WinUI.ViewModels.Dialogs.Dashboard;

namespace WinUI.Views.Dialogs.Dashboard;

public sealed partial class GoalKpiDialog : ContentDialog
{
    public GoalKpiDialogViewModel ViewModel { get; }

    public IconState HeaderIconState => ViewModel.Icon;

    public GoalKpiDialog(GoalKpiDialogViewModel viewModel, GoalKpiDialogRequest? request)
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
}
