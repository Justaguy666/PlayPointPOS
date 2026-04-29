using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI.Services.Layout;
using WinUI.ViewModels.Pages;

namespace WinUI.Views.Pages;

public sealed partial class DashboardPage : Page
{
    public DashboardPageViewModel ViewModel { get; }

    public DashboardPage(DashboardPageViewModel viewModel)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;
        InitializeComponent();
        SizeChanged += HandleSizeChanged;
        Unloaded += HandleUnloaded;
        ApplyResponsiveLayout();
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        SizeChanged -= HandleSizeChanged;
        Unloaded -= HandleUnloaded;
        ViewModel.Dispose();
    }

    private void HandleSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplyResponsiveLayout();
    }

    private void ApplyResponsiveLayout()
    {
        double width = ActualWidth;
        ResponsiveBreakpoint breakpoint = ResponsiveBreakpoints.FromWidth(width);
        bool isCompact = breakpoint == ResponsiveBreakpoint.Compact;

        RootGrid.Padding = isCompact
            ? new Thickness(ResponsiveBreakpoints.CompactPageHorizontalPadding, 24, ResponsiveBreakpoints.CompactPageHorizontalPadding, 24)
            : new Thickness(ResponsiveBreakpoints.PageHorizontalPadding, 36, ResponsiveBreakpoints.PageHorizontalPadding, 36);

        ApplyDashboardGridLayout(isCompact);
        ApplyStatsLayout(isCompact);
        ApplyPopularLayout(isCompact);
    }

    private void ApplyDashboardGridLayout(bool isCompact)
    {
        RootGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
        RootGrid.ColumnDefinitions[1].Width = isCompact
            ? new GridLength(0)
            : new GridLength(1, GridUnitType.Star);

        Grid.SetRow(StatsGrid, 0);
        Grid.SetColumn(StatsGrid, 0);
        Grid.SetColumnSpan(StatsGrid, 1);

        Grid.SetRow(RevenueChart, 1);
        Grid.SetColumn(RevenueChart, 0);

        Grid.SetRow(SidePanel, isCompact ? 2 : 0);
        Grid.SetColumn(SidePanel, isCompact ? 0 : 1);
        Grid.SetRowSpan(SidePanel, isCompact ? 1 : 2);

        Grid.SetRow(PopularGrid, isCompact ? 3 : 2);
        Grid.SetColumn(PopularGrid, 0);
        Grid.SetColumnSpan(PopularGrid, isCompact ? 1 : 2);
    }

    private void ApplyStatsLayout(bool isCompact)
    {
        StatsGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
        StatsGrid.ColumnDefinitions[1].Width = isCompact
            ? new GridLength(0)
            : new GridLength(1, GridUnitType.Star);

        PositionStatCard(RevenueStatCard, 0, 0);
        PositionStatCard(GameSessionStatCard, isCompact ? 1 : 1, 0);
        PositionStatCard(CustomerStatCard, isCompact ? 2 : 0, isCompact ? 0 : 1);
        PositionStatCard(ProductStatCard, isCompact ? 3 : 1, isCompact ? 0 : 1);
    }

    private static void PositionStatCard(FrameworkElement card, int row, int column)
    {
        Grid.SetRow(card, row);
        Grid.SetColumn(card, column);
    }

    private void ApplyPopularLayout(bool isCompact)
    {
        PopularGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
        PopularGrid.ColumnDefinitions[1].Width = isCompact
            ? new GridLength(0)
            : new GridLength(1, GridUnitType.Star);
        PopularGrid.ColumnDefinitions[2].Width = isCompact
            ? new GridLength(0)
            : new GridLength(1, GridUnitType.Star);

        PositionPopularCard(TopGamesCard, 0, 0);
        PositionPopularCard(TopFoodsCard, isCompact ? 1 : 0, isCompact ? 0 : 1);
        PositionPopularCard(TopDrinksCard, isCompact ? 2 : 0, isCompact ? 0 : 2);
    }

    private static void PositionPopularCard(FrameworkElement card, int row, int column)
    {
        Grid.SetRow(card, row);
        Grid.SetColumn(card, column);
    }
}
