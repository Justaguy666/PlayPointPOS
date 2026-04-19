using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.Pages;

namespace WinUI.Views.Pages;

public sealed partial class MemberManagementPage : Page
{
    public MemberManagementPageViewModel ViewModel { get; }

    public IconState ManagePackagesIconState { get; } = new()
    {
        Kind = IconKind.Folder,
        Size = 20,
        AlwaysFilled = true,
    };

    public MemberManagementPage(MemberManagementPageViewModel viewModel)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;
        InitializeComponent();

        Unloaded += HandleUnloaded;
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        Unloaded -= HandleUnloaded;
        ViewModel.Dispose();
    }
}
