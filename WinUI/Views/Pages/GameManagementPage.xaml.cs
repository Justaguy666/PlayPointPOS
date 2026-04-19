using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.Pages;

namespace WinUI.Views.Pages;

public sealed partial class GameManagementPage : Page
{
    private const double GamesLeftPadding = 24;
    private const double GamesRightPadding = 24;

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

        Unloaded += HandleUnloaded;
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        Unloaded -= HandleUnloaded;
        ViewModel.Dispose();
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
        var availableWidth = GamesScrollViewer.ActualWidth - GamesLeftPadding - GamesRightPadding;
        if (availableWidth <= 0)
        {
            return;
        }

        ViewModel.UpdateGridGamesLayout(availableWidth);
    }
}
