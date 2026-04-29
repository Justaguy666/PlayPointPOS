using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using WinUI.Helpers;
using WinUI.Services.Layout;
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

        SizeChanged += HandleSizeChanged;
        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
        ApplyResponsiveLayout();
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        SizeChanged -= HandleSizeChanged;
        Loaded -= HandleLoaded;
        Unloaded -= HandleUnloaded;
        ViewModel.Dispose();
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        ToolTipHelper.ApplyMissingToolTips(this);
    }

    private void HandleSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplyResponsiveLayout();
    }

    private void ApplyResponsiveLayout()
    {
        bool isCompact = ResponsiveBreakpoints.IsCompact(ActualWidth);
        ContentStack.Padding = isCompact
            ? new Thickness(ResponsiveBreakpoints.CompactPageHorizontalPadding, 16, ResponsiveBreakpoints.CompactPageHorizontalPadding, 20)
            : new Thickness(ResponsiveBreakpoints.PageHorizontalPadding, 20, ResponsiveBreakpoints.PageHorizontalPadding, 24);
    }
}
