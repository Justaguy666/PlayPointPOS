using Microsoft.UI.Xaml.Controls;

namespace WinUI.Views.UserControls;

public sealed partial class SearchControl : UserControl
{
    public SearchControl()
    {
        InitializeComponent();
    }

    private void SearchTextBox_GotFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (this.Content is Grid rootGrid)
        {
            rootGrid.Background = (Microsoft.UI.Xaml.Media.Brush)Microsoft.UI.Xaml.Application.Current.Resources["VeryLightGrayBrush"];
        }
    }

    private void SearchTextBox_LostFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (this.Content is Grid rootGrid)
        {
            rootGrid.Background = (Microsoft.UI.Xaml.Media.Brush)Microsoft.UI.Xaml.Application.Current.Resources["WhiteBrush"];
        }
    }

    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public static readonly Microsoft.UI.Xaml.DependencyProperty SearchTextProperty =
        Microsoft.UI.Xaml.DependencyProperty.Register(
            nameof(SearchText),
            typeof(string),
            typeof(SearchControl),
            new Microsoft.UI.Xaml.PropertyMetadata(string.Empty)
        );

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public static readonly Microsoft.UI.Xaml.DependencyProperty PlaceholderTextProperty =
        Microsoft.UI.Xaml.DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(SearchControl),
            new Microsoft.UI.Xaml.PropertyMetadata("Search...")
        );

    public WinUI.UIModels.Enums.IconKind IconKind
    {
        get => (WinUI.UIModels.Enums.IconKind)GetValue(IconKindProperty);
        set => SetValue(IconKindProperty, value);
    }

    public static readonly Microsoft.UI.Xaml.DependencyProperty IconKindProperty =
        Microsoft.UI.Xaml.DependencyProperty.Register(
            nameof(IconKind),
            typeof(WinUI.UIModels.Enums.IconKind),
            typeof(SearchControl),
            new Microsoft.UI.Xaml.PropertyMetadata(WinUI.UIModels.Enums.IconKind.Search, OnIconKindChanged)
        );

    public WinUI.UIModels.IconState IconPath
    {
        get => (WinUI.UIModels.IconState)GetValue(IconPathProperty);
        set => SetValue(IconPathProperty, value);
    }

    public static readonly Microsoft.UI.Xaml.DependencyProperty IconPathProperty =
        Microsoft.UI.Xaml.DependencyProperty.Register(
            nameof(IconPath),
            typeof(WinUI.UIModels.IconState),
            typeof(SearchControl),
            new Microsoft.UI.Xaml.PropertyMetadata(null)
        );

    public System.Windows.Input.ICommand? SearchCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(SearchCommandProperty);
        set => SetValue(SearchCommandProperty, value);
    }

    public static readonly Microsoft.UI.Xaml.DependencyProperty SearchCommandProperty =
        Microsoft.UI.Xaml.DependencyProperty.Register(
            nameof(SearchCommand),
            typeof(System.Windows.Input.ICommand),
            typeof(SearchControl),
            new Microsoft.UI.Xaml.PropertyMetadata(null)
        );

    public System.Windows.Input.ICommand? ClearCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(ClearCommandProperty);
        set => SetValue(ClearCommandProperty, value);
    }

    public static readonly Microsoft.UI.Xaml.DependencyProperty ClearCommandProperty =
        Microsoft.UI.Xaml.DependencyProperty.Register(
            nameof(ClearCommand),
            typeof(System.Windows.Input.ICommand),
            typeof(SearchControl),
            new Microsoft.UI.Xaml.PropertyMetadata(null)
        );

    private static void OnIconKindChanged(Microsoft.UI.Xaml.DependencyObject d, Microsoft.UI.Xaml.DependencyPropertyChangedEventArgs e)
    {
        if (d is SearchControl control && e.NewValue is WinUI.UIModels.Enums.IconKind newKind)
        {
            control.IconPath = new WinUI.UIModels.IconState { Kind = newKind, Size = 24, IsEnabled = true };
        }
    }
}
