using System;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI.UIModels;
using WinUI.UIModels.Enums;

namespace WinUI.Views.UserControls.Dialogs;

public sealed partial class DialogShell : UserControl
{
    public DialogShell()
    {
        InitializeComponent();
        Loaded += HandleLoaded;
        SizeChanged += HandleSizeChanged;
        BodyScrollViewer.SizeChanged += HandleBodyScrollViewerSizeChanged;
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(DialogShell),
            new PropertyMetadata(string.Empty));

    public IconState? IconState
    {
        get => (IconState?)GetValue(IconStateProperty);
        set => SetValue(IconStateProperty, value);
    }

    public static readonly DependencyProperty IconStateProperty =
        DependencyProperty.Register(
            nameof(IconState),
            typeof(IconState),
            typeof(DialogShell),
            new PropertyMetadata(null, HandleChromePropertyChanged));

    public IconState CloseIconState
    {
        get => (IconState)GetValue(CloseIconStateProperty);
        set => SetValue(CloseIconStateProperty, value);
    }

    public static readonly DependencyProperty CloseIconStateProperty =
        DependencyProperty.Register(
            nameof(CloseIconState),
            typeof(IconState),
            typeof(DialogShell),
            new PropertyMetadata(new IconState { Kind = IconKind.Close, Size = 16 }));

    public ICommand? CloseCommand
    {
        get => (ICommand?)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    public static readonly DependencyProperty CloseCommandProperty =
        DependencyProperty.Register(
            nameof(CloseCommand),
            typeof(ICommand),
            typeof(DialogShell),
            new PropertyMetadata(null));

    public string CloseTooltipText
    {
        get => (string)GetValue(CloseTooltipTextProperty);
        set => SetValue(CloseTooltipTextProperty, value);
    }

    public static readonly DependencyProperty CloseTooltipTextProperty =
        DependencyProperty.Register(
            nameof(CloseTooltipText),
            typeof(string),
            typeof(DialogShell),
            new PropertyMetadata(string.Empty));

    public bool ShowCloseButton
    {
        get => (bool)GetValue(ShowCloseButtonProperty);
        set => SetValue(ShowCloseButtonProperty, value);
    }

    public static readonly DependencyProperty ShowCloseButtonProperty =
        DependencyProperty.Register(
            nameof(ShowCloseButton),
            typeof(bool),
            typeof(DialogShell),
            new PropertyMetadata(true, HandleChromePropertyChanged));

    public object? BodyContent
    {
        get => GetValue(BodyContentProperty);
        set => SetValue(BodyContentProperty, value);
    }

    public static readonly DependencyProperty BodyContentProperty =
        DependencyProperty.Register(
            nameof(BodyContent),
            typeof(object),
            typeof(DialogShell),
            new PropertyMetadata(null));

    public object? FooterContent
    {
        get => GetValue(FooterContentProperty);
        set => SetValue(FooterContentProperty, value);
    }

    public static readonly DependencyProperty FooterContentProperty =
        DependencyProperty.Register(
            nameof(FooterContent),
            typeof(object),
            typeof(DialogShell),
            new PropertyMetadata(null, HandleChromePropertyChanged));

    public DialogShellSize Size
    {
        get => (DialogShellSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(
            nameof(Size),
            typeof(DialogShellSize),
            typeof(DialogShell),
            new PropertyMetadata(DialogShellSize.Standard, HandleChromePropertyChanged));

    public double MaxBodyHeight
    {
        get => (double)GetValue(MaxBodyHeightProperty);
        set => SetValue(MaxBodyHeightProperty, value);
    }

    public static readonly DependencyProperty MaxBodyHeightProperty =
        DependencyProperty.Register(
            nameof(MaxBodyHeight),
            typeof(double),
            typeof(DialogShell),
            new PropertyMetadata(double.PositiveInfinity, HandleChromePropertyChanged));

    private static void HandleChromePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is DialogShell shell && shell.IsLoaded)
        {
            shell.ApplyChrome();
        }
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        ApplyChrome();
        UpdateBodyContentWidth();
    }

    private void HandleSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplyChrome();
        UpdateBodyContentWidth();
    }

    private void HandleBodyScrollViewerSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateBodyContentWidth();
    }

    private void ApplyChrome()
    {
        (double minWidth, double maxWidth) = Size switch
        {
            DialogShellSize.Compact => (360, 600),
            DialogShellSize.Wide => (560, 820),
            _ => (440, 720),
        };

        double availableWidth = ResolveAvailableWidth();
        if (availableWidth > 0)
        {
            maxWidth = Math.Min(maxWidth, availableWidth);
            minWidth = Math.Min(minWidth, maxWidth);
        }

        RootGrid.MinWidth = minWidth;
        RootGrid.MaxWidth = maxWidth;
        BodyScrollViewer.MaxHeight = MaxBodyHeight;
        BodyScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
        BodyScrollViewer.VerticalScrollMode = ScrollMode.Enabled;

        HeaderIcon.Visibility = IconState is null ? Visibility.Collapsed : Visibility.Visible;
        CloseButton.Visibility = ShowCloseButton ? Visibility.Visible : Visibility.Collapsed;
        FooterDivider.Visibility = FooterContent is null ? Visibility.Collapsed : Visibility.Visible;
        FooterPresenter.Visibility = FooterContent is null ? Visibility.Collapsed : Visibility.Visible;
    }

    private double ResolveAvailableWidth()
    {
        const double horizontalMargin = 80;

        if (!double.IsNaN(ActualWidth) && ActualWidth > 0)
        {
            return Math.Max(0, ActualWidth);
        }

        double rootWidth = XamlRoot?.Size.Width ?? 0;
        if (double.IsNaN(rootWidth) || rootWidth <= 0)
        {
            return 0;
        }

        return Math.Max(0, rootWidth - horizontalMargin);
    }

    private void UpdateBodyContentWidth()
    {
        if (BodyContainer is null)
        {
            return;
        }

        double width = BodyScrollViewer.ViewportWidth;
        if (double.IsNaN(width) || width <= 0)
        {
            width = BodyScrollViewer.ActualWidth;
        }

        if (double.IsNaN(width) || width <= 0)
        {
            return;
        }

        double padding = BodyScrollViewer.Padding.Left + BodyScrollViewer.Padding.Right;
        BodyContainer.Width = Math.Max(0, width - padding);
    }
}
