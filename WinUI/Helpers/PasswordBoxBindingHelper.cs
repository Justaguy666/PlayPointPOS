using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WinUI.Helpers;

public static class PasswordBoxBindingHelper
{
    public static readonly DependencyProperty BoundPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BoundPassword",
            typeof(string),
            typeof(PasswordBoxBindingHelper),
            new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

    public static readonly DependencyProperty BindPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BindPassword",
            typeof(bool),
            typeof(PasswordBoxBindingHelper),
            new PropertyMetadata(false, OnBindPasswordChanged));

    private static readonly DependencyProperty IsUpdatingPasswordProperty =
        DependencyProperty.RegisterAttached(
            "IsUpdatingPassword",
            typeof(bool),
            typeof(PasswordBoxBindingHelper),
            new PropertyMetadata(false));

    public static string GetBoundPassword(DependencyObject dependencyObject)
    {
        return (string)dependencyObject.GetValue(BoundPasswordProperty);
    }

    public static void SetBoundPassword(DependencyObject dependencyObject, string value)
    {
        dependencyObject.SetValue(BoundPasswordProperty, value);
    }

    public static bool GetBindPassword(DependencyObject dependencyObject)
    {
        return (bool)dependencyObject.GetValue(BindPasswordProperty);
    }

    public static void SetBindPassword(DependencyObject dependencyObject, bool value)
    {
        dependencyObject.SetValue(BindPasswordProperty, value);
    }

    private static bool GetIsUpdatingPassword(DependencyObject dependencyObject)
    {
        return (bool)dependencyObject.GetValue(IsUpdatingPasswordProperty);
    }

    private static void SetIsUpdatingPassword(DependencyObject dependencyObject, bool value)
    {
        dependencyObject.SetValue(IsUpdatingPasswordProperty, value);
    }

    private static void OnBindPasswordChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is not PasswordBox passwordBox)
        {
            return;
        }

        passwordBox.PasswordChanged -= HandlePasswordChanged;

        if ((bool)e.NewValue)
        {
            passwordBox.PasswordChanged += HandlePasswordChanged;
        }
    }

    private static void OnBoundPasswordChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is not PasswordBox passwordBox || GetIsUpdatingPassword(passwordBox))
        {
            return;
        }

        passwordBox.Password = e.NewValue as string ?? string.Empty;
    }

    private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not PasswordBox passwordBox)
        {
            return;
        }

        SetIsUpdatingPassword(passwordBox, true);
        SetBoundPassword(passwordBox, passwordBox.Password);
        SetIsUpdatingPassword(passwordBox, false);
    }
}
