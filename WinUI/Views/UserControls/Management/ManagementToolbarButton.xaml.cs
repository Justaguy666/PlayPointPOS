using System;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WinUI.UIModels;

namespace WinUI.Views.UserControls.Management;

public sealed partial class ManagementToolbarButton : UserControl
{
    public ManagementToolbarButton()
    {
        InitializeComponent();
        Loaded += HandleLoaded;
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(ManagementToolbarButton),
            new PropertyMetadata(null));

    public IconState? IconState
    {
        get => (IconState?)GetValue(IconStateProperty);
        set => SetValue(IconStateProperty, value);
    }

    public static readonly DependencyProperty IconStateProperty =
        DependencyProperty.Register(
            nameof(IconState),
            typeof(IconState),
            typeof(ManagementToolbarButton),
            new PropertyMetadata(null));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(ManagementToolbarButton),
            new PropertyMetadata(string.Empty));

    public bool IsPrimary
    {
        get => (bool)GetValue(IsPrimaryProperty);
        set => SetValue(IsPrimaryProperty, value);
    }

    public static readonly DependencyProperty IsPrimaryProperty =
        DependencyProperty.Register(
            nameof(IsPrimary),
            typeof(bool),
            typeof(ManagementToolbarButton),
            new PropertyMetadata(false, HandleVisualPropertyChanged));

    public bool IsCompact
    {
        get => (bool)GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }

    public static readonly DependencyProperty IsCompactProperty =
        DependencyProperty.Register(
            nameof(IsCompact),
            typeof(bool),
            typeof(ManagementToolbarButton),
            new PropertyMetadata(false, HandleVisualPropertyChanged));

    public double WideMinWidth
    {
        get => (double)GetValue(WideMinWidthProperty);
        set => SetValue(WideMinWidthProperty, value);
    }

    public static readonly DependencyProperty WideMinWidthProperty =
        DependencyProperty.Register(
            nameof(WideMinWidth),
            typeof(double),
            typeof(ManagementToolbarButton),
            new PropertyMetadata(0d, HandleVisualPropertyChanged));

    public double CompactMinWidth
    {
        get => (double)GetValue(CompactMinWidthProperty);
        set => SetValue(CompactMinWidthProperty, value);
    }

    public static readonly DependencyProperty CompactMinWidthProperty =
        DependencyProperty.Register(
            nameof(CompactMinWidth),
            typeof(double),
            typeof(ManagementToolbarButton),
            new PropertyMetadata(48d, HandleVisualPropertyChanged));

    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public static readonly DependencyProperty IconSizeProperty =
        DependencyProperty.Register(
            nameof(IconSize),
            typeof(double),
            typeof(ManagementToolbarButton),
            new PropertyMetadata(18d, HandleVisualPropertyChanged));

    private static void HandleVisualPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is ManagementToolbarButton button && button.IsLoaded)
        {
            button.ApplyVisualState();
        }
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        ApplyVisualState();
    }

    private void ApplyVisualState()
    {
        ActionButton.Style = (Style)Microsoft.UI.Xaml.Application.Current.Resources[
            IsPrimary ? "PrimaryActionButtonStyle" : "SecondaryActionButtonStyle"];

        ActionButton.Resources.MergedDictionaries.Clear();
        ActionButton.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri(IsPrimary
                ? "ms-appx:///Resources/Styles/StateResources/PrimaryActionButtonStateResources.xaml"
                : "ms-appx:///Resources/Styles/StateResources/SecondaryActionButtonStateResources.xaml")
        });

        ActionButton.MinWidth = IsCompact ? CompactMinWidth : WideMinWidth;
        ActionButton.Padding = IsCompact ? new Thickness(0) : new Thickness(14, 0, 14, 0);
        LabelText.Visibility = IsCompact ? Visibility.Collapsed : Visibility.Visible;
        LabelText.FontWeight = IsPrimary ? Microsoft.UI.Text.FontWeights.SemiBold : Microsoft.UI.Text.FontWeights.Normal;
        Icon.Width = IconSize;
        Icon.Height = IconSize;

        if (!IsPrimary)
        {
            ActionButton.Background = (Brush)Microsoft.UI.Xaml.Application.Current.Resources["WhiteBrush"];
            ActionButton.BorderBrush = (Brush)Microsoft.UI.Xaml.Application.Current.Resources["LightGrayBrush"];
            ActionButton.BorderThickness = new Thickness(1);
            ActionButton.Foreground = (Brush)Microsoft.UI.Xaml.Application.Current.Resources["BlackBrush"];
        }
    }
}
