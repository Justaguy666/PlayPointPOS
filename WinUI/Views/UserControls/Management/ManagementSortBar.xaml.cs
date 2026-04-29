using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI.Services.Layout;

namespace WinUI.Views.UserControls.Management;

public sealed partial class ManagementSortBar : UserControl
{
    public ManagementSortBar()
    {
        InitializeComponent();
        Loaded += HandleLoaded;
        SizeChanged += HandleSizeChanged;
    }

    public object? SortFieldItemsSource
    {
        get => GetValue(SortFieldItemsSourceProperty);
        set => SetValue(SortFieldItemsSourceProperty, value);
    }

    public static readonly DependencyProperty SortFieldItemsSourceProperty =
        DependencyProperty.Register(
            nameof(SortFieldItemsSource),
            typeof(object),
            typeof(ManagementSortBar),
            new PropertyMetadata(null));

    public string SelectedSortField
    {
        get => (string)GetValue(SelectedSortFieldProperty);
        set => SetValue(SelectedSortFieldProperty, value);
    }

    public static readonly DependencyProperty SelectedSortFieldProperty =
        DependencyProperty.Register(
            nameof(SelectedSortField),
            typeof(string),
            typeof(ManagementSortBar),
            new PropertyMetadata(string.Empty));

    public object? SortDirectionItemsSource
    {
        get => GetValue(SortDirectionItemsSourceProperty);
        set => SetValue(SortDirectionItemsSourceProperty, value);
    }

    public static readonly DependencyProperty SortDirectionItemsSourceProperty =
        DependencyProperty.Register(
            nameof(SortDirectionItemsSource),
            typeof(object),
            typeof(ManagementSortBar),
            new PropertyMetadata(null));

    public string SelectedSortDirection
    {
        get => (string)GetValue(SelectedSortDirectionProperty);
        set => SetValue(SelectedSortDirectionProperty, value);
    }

    public static readonly DependencyProperty SelectedSortDirectionProperty =
        DependencyProperty.Register(
            nameof(SelectedSortDirection),
            typeof(string),
            typeof(ManagementSortBar),
            new PropertyMetadata(string.Empty));

    public string SortFieldPlaceholderText
    {
        get => (string)GetValue(SortFieldPlaceholderTextProperty);
        set => SetValue(SortFieldPlaceholderTextProperty, value);
    }

    public static readonly DependencyProperty SortFieldPlaceholderTextProperty =
        DependencyProperty.Register(
            nameof(SortFieldPlaceholderText),
            typeof(string),
            typeof(ManagementSortBar),
            new PropertyMetadata(string.Empty));

    public string SortDirectionPlaceholderText
    {
        get => (string)GetValue(SortDirectionPlaceholderTextProperty);
        set => SetValue(SortDirectionPlaceholderTextProperty, value);
    }

    public static readonly DependencyProperty SortDirectionPlaceholderTextProperty =
        DependencyProperty.Register(
            nameof(SortDirectionPlaceholderText),
            typeof(string),
            typeof(ManagementSortBar),
            new PropertyMetadata(string.Empty));

    public string PageInfoText
    {
        get => (string)GetValue(PageInfoTextProperty);
        set => SetValue(PageInfoTextProperty, value);
    }

    public static readonly DependencyProperty PageInfoTextProperty =
        DependencyProperty.Register(
            nameof(PageInfoText),
            typeof(string),
            typeof(ManagementSortBar),
            new PropertyMetadata(string.Empty));

    public bool ShowSortControls
    {
        get => (bool)GetValue(ShowSortControlsProperty);
        set => SetValue(ShowSortControlsProperty, value);
    }

    public static readonly DependencyProperty ShowSortControlsProperty =
        DependencyProperty.Register(
            nameof(ShowSortControls),
            typeof(bool),
            typeof(ManagementSortBar),
            new PropertyMetadata(true));

    public double ComboBoxWidth
    {
        get => (double)GetValue(ComboBoxWidthProperty);
        set => SetValue(ComboBoxWidthProperty, value);
    }

    public static readonly DependencyProperty ComboBoxWidthProperty =
        DependencyProperty.Register(
            nameof(ComboBoxWidth),
            typeof(double),
            typeof(ManagementSortBar),
            new PropertyMetadata(152d));

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        ApplyResponsiveLayout();
    }

    private void HandleSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplyResponsiveLayout();
    }

    private void ApplyResponsiveLayout()
    {
        bool isCompact = ActualWidth > 0 && ActualWidth < ResponsiveBreakpoints.WideMinWidth;

        Grid.SetRow(SortControlsPanel, 0);
        Grid.SetColumn(SortControlsPanel, 0);
        Grid.SetColumnSpan(SortControlsPanel, isCompact ? 2 : 1);
        SortControlsPanel.Orientation = isCompact ? Orientation.Vertical : Orientation.Horizontal;
        SortControlsPanel.HorizontalAlignment = isCompact ? HorizontalAlignment.Stretch : HorizontalAlignment.Left;

        double comboWidth = isCompact ? double.NaN : ComboBoxWidth;
        SortFieldComboBox.Width = comboWidth;
        SortDirectionComboBox.Width = comboWidth;
        SortFieldComboBox.HorizontalAlignment = isCompact ? HorizontalAlignment.Stretch : HorizontalAlignment.Left;
        SortDirectionComboBox.HorizontalAlignment = isCompact ? HorizontalAlignment.Stretch : HorizontalAlignment.Left;

        Grid.SetRow(PageInfo, isCompact ? 1 : 0);
        Grid.SetColumn(PageInfo, isCompact ? 0 : 1);
        Grid.SetColumnSpan(PageInfo, isCompact ? 2 : 1);
        PageInfo.HorizontalAlignment = isCompact ? HorizontalAlignment.Stretch : HorizontalAlignment.Right;
        PageInfo.MaxWidth = isCompact ? Math.Max(180, ActualWidth) : double.PositiveInfinity;
    }
}
