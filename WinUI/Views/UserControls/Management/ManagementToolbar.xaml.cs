using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WinUI.Services.Layout;
using WinUI.UIModels.Enums;

namespace WinUI.Views.UserControls.Management;

public sealed partial class ManagementToolbar : UserControl
{
    public ManagementToolbar()
    {
        InitializeComponent();
        Loaded += HandleLoaded;
        SizeChanged += HandleSizeChanged;
    }

    public object? LeadingContent
    {
        get => GetValue(LeadingContentProperty);
        set => SetValue(LeadingContentProperty, value);
    }

    public static readonly DependencyProperty LeadingContentProperty =
        DependencyProperty.Register(
            nameof(LeadingContent),
            typeof(object),
            typeof(ManagementToolbar),
            new PropertyMetadata(null, HandleLayoutPropertyChanged));

    public object? ActionsContent
    {
        get => GetValue(ActionsContentProperty);
        set => SetValue(ActionsContentProperty, value);
    }

    public static readonly DependencyProperty ActionsContentProperty =
        DependencyProperty.Register(
            nameof(ActionsContent),
            typeof(object),
            typeof(ManagementToolbar),
            new PropertyMetadata(null, HandleLayoutPropertyChanged));

    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public static readonly DependencyProperty SearchTextProperty =
        DependencyProperty.Register(
            nameof(SearchText),
            typeof(string),
            typeof(ManagementToolbar),
            new PropertyMetadata(string.Empty));

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(ManagementToolbar),
            new PropertyMetadata(string.Empty));

    public IconKind IconKind
    {
        get => (IconKind)GetValue(IconKindProperty);
        set => SetValue(IconKindProperty, value);
    }

    public static readonly DependencyProperty IconKindProperty =
        DependencyProperty.Register(
            nameof(IconKind),
            typeof(IconKind),
            typeof(ManagementToolbar),
            new PropertyMetadata(IconKind.Search));

    public ICommand? ClearCommand
    {
        get => (ICommand?)GetValue(ClearCommandProperty);
        set => SetValue(ClearCommandProperty, value);
    }

    public static readonly DependencyProperty ClearCommandProperty =
        DependencyProperty.Register(
            nameof(ClearCommand),
            typeof(ICommand),
            typeof(ManagementToolbar),
            new PropertyMetadata(null));

    private static void HandleLayoutPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is ManagementToolbar toolbar && toolbar.IsLoaded)
        {
            toolbar.ApplyResponsiveLayout();
        }
    }

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
        bool hasLeading = LeadingContent is not null;
        bool hasActions = ActionsContent is not null;
        bool isCompact = ActualWidth > 0 && ActualWidth < ResponsiveBreakpoints.WideMinWidth;
        bool isPageCompact = ResponsiveBreakpoints.IsCompact(ActualWidth);
        double horizontalPadding = isPageCompact
            ? ResponsiveBreakpoints.CompactPageHorizontalPadding
            : ResponsiveBreakpoints.PageHorizontalPadding;

        RootGrid.Padding = isPageCompact
            ? new Thickness(horizontalPadding, 14, horizontalPadding, 14)
            : new Thickness(horizontalPadding, 20, horizontalPadding, 20);

        LeadingPresenter.Visibility = hasLeading ? Visibility.Visible : Visibility.Collapsed;
        ActionsPresenter.Visibility = hasActions ? Visibility.Visible : Visibility.Collapsed;

        Grid.SetRow(LeadingPresenter, 0);
        Grid.SetColumn(LeadingPresenter, 0);
        Grid.SetColumnSpan(LeadingPresenter, 1);

        if (isCompact)
        {
            Grid.SetRow(SearchBox, 0);
            Grid.SetColumn(SearchBox, hasLeading ? 1 : 0);
            Grid.SetColumnSpan(SearchBox, hasLeading ? 2 : 3);

            Grid.SetRow(ActionsPresenter, 1);
            Grid.SetColumn(ActionsPresenter, 0);
            Grid.SetColumnSpan(ActionsPresenter, 3);
            ActionsPresenter.HorizontalAlignment = HorizontalAlignment.Left;
            ApplyToolbarButtonState(isCompact);
            return;
        }

        Grid.SetRow(SearchBox, 0);
        Grid.SetColumn(SearchBox, hasLeading ? 1 : 0);
        Grid.SetColumnSpan(SearchBox, hasLeading ? 1 : 2);

        Grid.SetRow(ActionsPresenter, 0);
        Grid.SetColumn(ActionsPresenter, 2);
        Grid.SetColumnSpan(ActionsPresenter, 1);
        ActionsPresenter.HorizontalAlignment = HorizontalAlignment.Right;
        ApplyToolbarButtonState(isCompact);
    }

    private void ApplyToolbarButtonState(bool isCompact)
    {
        ApplyToolbarButtonState(LeadingPresenter, isCompact);
        ApplyToolbarButtonState(ActionsPresenter, isCompact);
    }

    private static void ApplyToolbarButtonState(DependencyObject root, bool isCompact)
    {
        if (root is ManagementToolbarButton toolbarButton)
        {
            toolbarButton.IsCompact = isCompact;
            return;
        }

        int childCount = VisualTreeHelper.GetChildrenCount(root);
        for (int index = 0; index < childCount; index++)
        {
            ApplyToolbarButtonState(VisualTreeHelper.GetChild(root, index), isCompact);
        }
    }
}
