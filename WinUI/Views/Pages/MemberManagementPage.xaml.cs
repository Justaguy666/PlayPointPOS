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
        bool isToolbarCompact = ActualWidth < ResponsiveBreakpoints.WideMinWidth;

        ToolbarGrid.Padding = isCompact
            ? new Thickness(ResponsiveBreakpoints.CompactPageHorizontalPadding, 16, ResponsiveBreakpoints.CompactPageHorizontalPadding, 16)
            : new Thickness(ResponsiveBreakpoints.PageHorizontalPadding, 20, ResponsiveBreakpoints.PageHorizontalPadding, 20);
        ContentStack.Padding = isCompact
            ? new Thickness(ResponsiveBreakpoints.CompactPageHorizontalPadding, 16, ResponsiveBreakpoints.CompactPageHorizontalPadding, 20)
            : new Thickness(ResponsiveBreakpoints.PageHorizontalPadding, 20, ResponsiveBreakpoints.PageHorizontalPadding, 24);

        ApplyToolbarLayout(isToolbarCompact);
        ApplySortLayout(isToolbarCompact);
    }

    private void ApplyToolbarLayout(bool isCompact)
    {
        Grid.SetRow(SearchBox, 0);
        Grid.SetColumn(SearchBox, 0);
        Grid.SetColumnSpan(SearchBox, isCompact ? 4 : 1);
        SearchBox.Margin = isCompact ? new Thickness(0, 0, 0, 12) : new Thickness(0, 0, 20, 0);

        Grid.SetRow(ManagePackagesButton, isCompact ? 1 : 0);
        Grid.SetColumn(ManagePackagesButton, isCompact ? 0 : 1);
        Grid.SetRow(FilterButton, isCompact ? 1 : 0);
        Grid.SetColumn(FilterButton, isCompact ? 1 : 2);
        Grid.SetRow(AddButton, isCompact ? 1 : 0);
        Grid.SetColumn(AddButton, isCompact ? 2 : 3);

        ManagePackagesButtonText.Visibility = isCompact ? Visibility.Collapsed : Visibility.Visible;
        FilterButtonText.Visibility = isCompact ? Visibility.Collapsed : Visibility.Visible;
        AddButtonText.Visibility = isCompact ? Visibility.Collapsed : Visibility.Visible;

        Thickness padding = isCompact ? new Thickness(0) : new Thickness(14, 0, 14, 0);
        ManagePackagesButton.MinWidth = isCompact ? 48 : 148;
        FilterButton.MinWidth = isCompact ? 48 : 108;
        AddButton.MinWidth = isCompact ? 48 : 0;
        ManagePackagesButton.Padding = padding;
        FilterButton.Padding = padding;
        AddButton.Padding = padding;
    }

    private void ApplySortLayout(bool isCompact)
    {
        Grid.SetRow(SortControlsPanel, 0);
        Grid.SetColumn(SortControlsPanel, 0);
        Grid.SetColumnSpan(SortControlsPanel, isCompact ? 2 : 1);
        SortControlsPanel.Orientation = isCompact ? Orientation.Vertical : Orientation.Horizontal;
        SortControlsPanel.HorizontalAlignment = isCompact ? HorizontalAlignment.Stretch : HorizontalAlignment.Left;

        double comboWidth = isCompact ? double.NaN : 174;
        SortFieldComboBox.Width = comboWidth;
        SortDirectionComboBox.Width = comboWidth;
        SortFieldComboBox.HorizontalAlignment = isCompact ? HorizontalAlignment.Stretch : HorizontalAlignment.Left;
        SortDirectionComboBox.HorizontalAlignment = isCompact ? HorizontalAlignment.Stretch : HorizontalAlignment.Left;

        Grid.SetRow(PageInfoBorder, isCompact ? 1 : 0);
        Grid.SetColumn(PageInfoBorder, isCompact ? 0 : 1);
        Grid.SetColumnSpan(PageInfoBorder, isCompact ? 2 : 1);
        PageInfoBorder.HorizontalAlignment = isCompact ? HorizontalAlignment.Left : HorizontalAlignment.Right;
        PageInfoText.MaxWidth = isCompact ? Math.Max(180, ActualWidth - 64) : double.PositiveInfinity;
    }
}
