using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;

namespace WinUI.Behaviors;

public static class InputCommandBehavior
{
    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.RegisterAttached(
            "CommandParameter",
            typeof(object),
            typeof(InputCommandBehavior),
            new PropertyMetadata(null));

    public static readonly DependencyProperty TappedCommandProperty =
        DependencyProperty.RegisterAttached(
            "TappedCommand",
            typeof(ICommand),
            typeof(InputCommandBehavior),
            new PropertyMetadata(null, HandleTappedCommandChanged));

    public static readonly DependencyProperty DoubleTappedCommandProperty =
        DependencyProperty.RegisterAttached(
            "DoubleTappedCommand",
            typeof(ICommand),
            typeof(InputCommandBehavior),
            new PropertyMetadata(null, HandleDoubleTappedCommandChanged));

    public static readonly DependencyProperty RightTappedCommandProperty =
        DependencyProperty.RegisterAttached(
            "RightTappedCommand",
            typeof(ICommand),
            typeof(InputCommandBehavior),
            new PropertyMetadata(null, HandleRightTappedCommandChanged));

    public static readonly DependencyProperty ShowAttachedFlyoutOnRightTappedProperty =
        DependencyProperty.RegisterAttached(
            "ShowAttachedFlyoutOnRightTapped",
            typeof(bool),
            typeof(InputCommandBehavior),
            new PropertyMetadata(false));

    public static readonly DependencyProperty RightTappedFlyoutOwnerProperty =
        DependencyProperty.RegisterAttached(
            "RightTappedFlyoutOwner",
            typeof(FrameworkElement),
            typeof(InputCommandBehavior),
            new PropertyMetadata(null));

    public static readonly DependencyProperty HideAttachedFlyoutOnClickProperty =
        DependencyProperty.RegisterAttached(
            "HideAttachedFlyoutOnClick",
            typeof(bool),
            typeof(InputCommandBehavior),
            new PropertyMetadata(false, HandleHideAttachedFlyoutOnClickChanged));

    public static readonly DependencyProperty ClickFlyoutOwnerProperty =
        DependencyProperty.RegisterAttached(
            "ClickFlyoutOwner",
            typeof(FrameworkElement),
            typeof(InputCommandBehavior),
            new PropertyMetadata(null));

    public static object? GetCommandParameter(DependencyObject element)
        => element.GetValue(CommandParameterProperty);

    public static void SetCommandParameter(DependencyObject element, object? value)
        => element.SetValue(CommandParameterProperty, value);

    public static ICommand? GetTappedCommand(DependencyObject element)
        => (ICommand?)element.GetValue(TappedCommandProperty);

    public static void SetTappedCommand(DependencyObject element, ICommand? value)
        => element.SetValue(TappedCommandProperty, value);

    public static ICommand? GetDoubleTappedCommand(DependencyObject element)
        => (ICommand?)element.GetValue(DoubleTappedCommandProperty);

    public static void SetDoubleTappedCommand(DependencyObject element, ICommand? value)
        => element.SetValue(DoubleTappedCommandProperty, value);

    public static ICommand? GetRightTappedCommand(DependencyObject element)
        => (ICommand?)element.GetValue(RightTappedCommandProperty);

    public static void SetRightTappedCommand(DependencyObject element, ICommand? value)
        => element.SetValue(RightTappedCommandProperty, value);

    public static bool GetShowAttachedFlyoutOnRightTapped(DependencyObject element)
        => (bool)element.GetValue(ShowAttachedFlyoutOnRightTappedProperty);

    public static void SetShowAttachedFlyoutOnRightTapped(DependencyObject element, bool value)
        => element.SetValue(ShowAttachedFlyoutOnRightTappedProperty, value);

    public static FrameworkElement? GetRightTappedFlyoutOwner(DependencyObject element)
        => (FrameworkElement?)element.GetValue(RightTappedFlyoutOwnerProperty);

    public static void SetRightTappedFlyoutOwner(DependencyObject element, FrameworkElement? value)
        => element.SetValue(RightTappedFlyoutOwnerProperty, value);

    public static bool GetHideAttachedFlyoutOnClick(DependencyObject element)
        => (bool)element.GetValue(HideAttachedFlyoutOnClickProperty);

    public static void SetHideAttachedFlyoutOnClick(DependencyObject element, bool value)
        => element.SetValue(HideAttachedFlyoutOnClickProperty, value);

    public static FrameworkElement? GetClickFlyoutOwner(DependencyObject element)
        => (FrameworkElement?)element.GetValue(ClickFlyoutOwnerProperty);

    public static void SetClickFlyoutOwner(DependencyObject element, FrameworkElement? value)
        => element.SetValue(ClickFlyoutOwnerProperty, value);

    private static void HandleTappedCommandChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is UIElement element)
        {
            if (e.OldValue is not null)
            {
                element.Tapped -= HandleTapped;
            }

            if (e.NewValue is not null)
            {
                element.Tapped += HandleTapped;
            }
        }
    }

    private static void HandleDoubleTappedCommandChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is UIElement element)
        {
            if (e.OldValue is not null)
            {
                element.DoubleTapped -= HandleDoubleTapped;
            }

            if (e.NewValue is not null)
            {
                element.DoubleTapped += HandleDoubleTapped;
            }
        }
    }

    private static void HandleRightTappedCommandChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is UIElement element)
        {
            if (e.OldValue is not null)
            {
                element.RightTapped -= HandleRightTapped;
            }

            if (e.NewValue is not null)
            {
                element.RightTapped += HandleRightTapped;
            }
        }
    }

    private static void HandleHideAttachedFlyoutOnClickChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is ButtonBase button)
        {
            if (e.OldValue is true)
            {
                button.Click -= HandleClick;
            }

            if (e.NewValue is true)
            {
                button.Click += HandleClick;
            }
        }
    }

    private static void HandleTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is DependencyObject element && ExecuteCommand(GetTappedCommand(element), ResolveParameter(element)))
        {
            e.Handled = true;
        }
    }

    private static void HandleDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is DependencyObject element && ExecuteCommand(GetDoubleTappedCommand(element), ResolveParameter(element)))
        {
            e.Handled = true;
        }
    }

    private static void HandleRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        if (!ExecuteCommand(GetRightTappedCommand(element), ResolveParameter(element)))
        {
            return;
        }

        if (GetShowAttachedFlyoutOnRightTapped(element))
        {
            FrameworkElement flyoutOwner = GetRightTappedFlyoutOwner(element) ?? element;
            FlyoutBase? flyout = FlyoutBase.GetAttachedFlyout(flyoutOwner);
            flyout?.ShowAt(
                flyoutOwner,
                new FlyoutShowOptions
                {
                    Position = e.GetPosition(flyoutOwner),
                });
        }

        e.Handled = true;
    }

    private static void HandleClick(object sender, RoutedEventArgs e)
    {
        if (sender is not DependencyObject element)
        {
            return;
        }

        FrameworkElement? flyoutOwner = GetClickFlyoutOwner(element) ?? element as FrameworkElement;
        if (flyoutOwner is null)
        {
            return;
        }

        FlyoutBase.GetAttachedFlyout(flyoutOwner)?.Hide();
    }

    private static object? ResolveParameter(DependencyObject element)
    {
        object? parameter = GetCommandParameter(element);
        return parameter ?? (element as FrameworkElement)?.DataContext;
    }

    private static bool ExecuteCommand(ICommand? command, object? parameter)
    {
        if (command is null || !command.CanExecute(parameter))
        {
            return false;
        }

        command.Execute(parameter);
        return true;
    }
}
