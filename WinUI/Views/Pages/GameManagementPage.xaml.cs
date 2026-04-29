using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI.Helpers;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.Services.Layout;
using WinUI.ViewModels.Pages;

namespace WinUI.Views.Pages;

public sealed partial class GameManagementPage : Page
{
    public GameManagementPageViewModel ViewModel { get; }

    public IconState ManageTypesIconState { get; } = new()
    {
        Kind = IconKind.Folder,
        Size = 20,
        AlwaysFilled = true,
    };

    public GameManagementPage(GameManagementPageViewModel viewModel)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;
        InitializeComponent();

        SizeChanged += HandlePageSizeChanged;
        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
        ApplyResponsiveLayout();
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        SizeChanged -= HandlePageSizeChanged;
        Loaded -= HandleLoaded;
        Unloaded -= HandleUnloaded;
        ViewModel.Dispose();
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        ToolTipHelper.ApplyMissingToolTips(this);
    }

    private void HandlePageSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplyResponsiveLayout();
    }

    private void HandleGamesScrollViewerLoaded(object sender, RoutedEventArgs e)
    {
        UpdateGamesLayout();
    }

    private void HandleGamesScrollViewerSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateGamesLayout();
    }

    private void UpdateGamesLayout()
    {
        double horizontalPadding = ResponsiveBreakpoints.IsCompact(ActualWidth)
            ? ResponsiveBreakpoints.CompactPageHorizontalPadding * 2
            : ResponsiveBreakpoints.PageHorizontalPadding * 2;
        var availableWidth = GamesScrollViewer.ActualWidth - horizontalPadding;
        if (availableWidth <= 0)
        {
            return;
        }

        ViewModel.UpdateGridGamesLayout(availableWidth);
    }

    private void ApplyResponsiveLayout()
    {
        bool isCompact = ResponsiveBreakpoints.IsCompact(ActualWidth);
        double horizontalPadding = isCompact
            ? ResponsiveBreakpoints.CompactPageHorizontalPadding
            : ResponsiveBreakpoints.PageHorizontalPadding;

        ContentPanel.Padding = new Thickness(horizontalPadding, 20, horizontalPadding, 24);
    }
}
