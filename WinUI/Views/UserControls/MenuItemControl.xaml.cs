using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Windows.Input;
using WinUI;

namespace WinUI.Views.UserControls;

public sealed partial class MenuItemControl : UserControl
{
    private Brush? _normalForeground;
    private Brush? _hoverForeground;

    public MenuItemControl()
    {
        InitializeComponent();
        Loaded += MenuItemControl_Loaded;
    }

    private void MenuItemControl_Loaded(object _sender, RoutedEventArgs _e)
    {
        // Get the current application's resources
        if (Microsoft.UI.Xaml.Application.Current?.Resources is var resources && resources != null)
        {
            _normalForeground = (Brush)resources["PrimaryOrangeBrush"];
            _hoverForeground = (Brush)resources["OrangePeachLightBrush"];
        }

        if (IconState == null)
        {
            IconState = new WinUI.UIModels.IconState { Kind = IconKind, Size = 24, IsHovered = false, IsSelected = false };
        }
        else
        {
            // Force property change to re-trigger binding
            var state = IconState;
            IconState = null!;
            IconState = state;
        }
    }

    private void RootButton_PointerEntered(object _sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs _e)
    {
        // Swap colors on hover
        if (_hoverForeground != null)
        {
            TextElement.Foreground = _hoverForeground;
            if (IconElement != null) IconElement.Foreground = _hoverForeground;
        }
        if (IconState != null)
        {
            var newState = new WinUI.UIModels.IconState { Kind = IconState.Kind, Size = IconState.Size, IsSelected = IconState.IsSelected, IsEnabled = IconState.IsEnabled, IsHovered = true };
            IconState = newState;
        }
    }

    private void RootButton_PointerExited(object _sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs _e)
    {
        // Restore normal colors on exit
        if (_normalForeground != null)
        {
            TextElement.Foreground = _normalForeground;
            if (IconElement != null) IconElement.Foreground = _normalForeground;
        }
        if (IconState != null)
        {
            var newState = new WinUI.UIModels.IconState { Kind = IconState.Kind, Size = IconState.Size, IsSelected = IconState.IsSelected, IsEnabled = IconState.IsEnabled, IsHovered = false };
            IconState = newState;
        }
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
            typeof(MenuItemControl),
            new PropertyMetadata(null)
        );

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(
            nameof(CommandParameter),
            typeof(object),
            typeof(MenuItemControl),
            new PropertyMetadata(null)
        );

    public WinUI.UIModels.Enums.IconKind IconKind
    {
        get => (WinUI.UIModels.Enums.IconKind)GetValue(IconKindProperty);
        set => SetValue(IconKindProperty, value);
    }

    public static readonly DependencyProperty IconKindProperty =
        DependencyProperty.Register(
            nameof(IconKind),
            typeof(WinUI.UIModels.Enums.IconKind),
            typeof(MenuItemControl),
            new PropertyMetadata(WinUI.UIModels.Enums.IconKind.Dashboard, OnIconKindChanged)
        );

    public WinUI.UIModels.IconState IconState
    {
        get => (WinUI.UIModels.IconState)GetValue(IconStateProperty);
        set => SetValue(IconStateProperty, value);
    }

    public static readonly DependencyProperty IconStateProperty =
        DependencyProperty.Register(
            nameof(IconState),
            typeof(WinUI.UIModels.IconState),
            typeof(MenuItemControl),
            new PropertyMetadata(null)
        );

    private static void OnIconKindChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MenuItemControl control && e.NewValue is WinUI.UIModels.Enums.IconKind newKind)
        {
            control.IconState = new WinUI.UIModels.IconState { Kind = newKind, Size = 24, IsHovered = false, IsSelected = false };
        }
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(MenuItemControl),
            new PropertyMetadata(string.Empty)
        );

    public new bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    public static readonly new DependencyProperty IsEnabledProperty =
        DependencyProperty.Register(
            nameof(IsEnabled),
            typeof(bool),
            typeof(MenuItemControl),
            new PropertyMetadata(true)
        );
}
