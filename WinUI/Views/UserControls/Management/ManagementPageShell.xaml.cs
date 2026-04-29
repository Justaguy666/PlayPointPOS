using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WinUI.Views.UserControls.Management;

public sealed partial class ManagementPageShell : UserControl
{
    public ManagementPageShell()
    {
        InitializeComponent();
    }

    public object? ToolbarContent
    {
        get => GetValue(ToolbarContentProperty);
        set => SetValue(ToolbarContentProperty, value);
    }

    public static readonly DependencyProperty ToolbarContentProperty =
        DependencyProperty.Register(
            nameof(ToolbarContent),
            typeof(object),
            typeof(ManagementPageShell),
            new PropertyMetadata(null));

    public object? BodyContent
    {
        get => GetValue(BodyContentProperty);
        set => SetValue(BodyContentProperty, value);
    }

    public static readonly DependencyProperty BodyContentProperty =
        DependencyProperty.Register(
            nameof(BodyContent),
            typeof(object),
            typeof(ManagementPageShell),
            new PropertyMetadata(null));
}
