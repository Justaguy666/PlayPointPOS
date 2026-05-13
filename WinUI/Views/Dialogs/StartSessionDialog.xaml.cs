using System;
using System.Diagnostics;
using Microsoft.UI.Xaml.Controls;
using WinUI.Helpers;
using WinUI.UIModels;
using WinUI.UIModels.Management;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.Dialogs;

namespace WinUI.Views.Dialogs;

public sealed partial class StartSessionDialog : ContentDialog
{
    public StartSessionViewModel ViewModel { get; }
    public IconState HeaderIconState { get; } = new() { Kind = IconKind.Table, Size = 24 };
    public IconState CloseIconState { get; } = new() { Kind = IconKind.Close, Size = 16 };

    public StartSessionDialog(StartSessionViewModel viewModel, AreaModel? model)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        ViewModel.Configure(model);
        DataContext = ViewModel;
        try
        {
            InitializeComponent();
        }
        catch (Exception ex)
        {
            SessionFlowDebugLog.Append("StartSessionDialog.InitializeComponent", ex);
            Debug.WriteLine(ex.ToString());
            throw;
        }

        ViewModel.CloseRequested += HandleCloseRequested;
        Closed += HandleClosed;
    }

    private void HandleCloseRequested()
    {
        try
        {
            Hide();
        }
        catch (Exception ex)
        {
            SessionFlowDebugLog.Append("StartSessionDialog.Hide", ex);
            Debug.WriteLine(ex.ToString());
        }
    }

    private void HandleClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        Closed -= HandleClosed;
        ViewModel.CloseRequested -= HandleCloseRequested;
        try
        {
            ViewModel.Dispose();
        }
        catch (Exception ex)
        {
            SessionFlowDebugLog.Append("StartSessionDialog.ViewModel.Dispose", ex);
            Debug.WriteLine(ex.ToString());
        }
    }
}
