using System.Reflection;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace WinUI.Helpers;

public static class CursorBehavior
{
    private static readonly PropertyInfo? ProtectedCursorProperty = typeof(UIElement).GetProperty(
        "ProtectedCursor",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    private static readonly InputCursor HandCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
    private static readonly InputCursor ArrowCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);

    public static readonly DependencyProperty UseHandCursorProperty =
        DependencyProperty.RegisterAttached(
            "UseHandCursor",
            typeof(bool),
            typeof(CursorBehavior),
            new PropertyMetadata(false, OnUseHandCursorChanged));

    public static bool GetUseHandCursor(DependencyObject obj)
    {
        return (bool)obj.GetValue(UseHandCursorProperty);
    }

    public static void SetUseHandCursor(DependencyObject obj, bool value)
    {
        obj.SetValue(UseHandCursorProperty, value);
    }

    private static void OnUseHandCursorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
        {
            return;
        }

        element.PointerEntered -= OnPointerEntered;
        element.PointerExited -= OnPointerExited;
        element.PointerCanceled -= OnPointerExited;
        element.PointerCaptureLost -= OnPointerExited;

        if (e.NewValue is true)
        {
            element.PointerEntered += OnPointerEntered;
            element.PointerExited += OnPointerExited;
            element.PointerCanceled += OnPointerExited;
            element.PointerCaptureLost += OnPointerExited;
            return;
        }

        SetProtectedCursor(element, ArrowCursor);
    }

    private static void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is UIElement element)
        {
            SetProtectedCursor(element, HandCursor);
        }
    }

    private static void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is UIElement element)
        {
            SetProtectedCursor(element, ArrowCursor);
        }
    }

    private static void SetProtectedCursor(UIElement element, InputCursor cursor)
    {
        ProtectedCursorProperty?.SetValue(element, cursor);
    }
}
