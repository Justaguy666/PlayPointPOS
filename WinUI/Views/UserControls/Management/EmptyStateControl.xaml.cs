using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI.UIModels;

namespace WinUI.Views.UserControls.Management;

public sealed partial class EmptyStateControl : UserControl
{
    public EmptyStateControl()
    {
        InitializeComponent();
    }

    public IconState? IconState
    {
        get => (IconState?)GetValue(IconStateProperty);
        set => SetValue(IconStateProperty, value);
    }

    public static readonly DependencyProperty IconStateProperty =
        DependencyProperty.Register(
            nameof(IconState),
            typeof(IconState),
            typeof(EmptyStateControl),
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
            typeof(EmptyStateControl),
            new PropertyMetadata(string.Empty));
}
