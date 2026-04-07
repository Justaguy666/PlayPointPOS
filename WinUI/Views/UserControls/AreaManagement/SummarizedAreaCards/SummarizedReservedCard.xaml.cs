using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace WinUI.Views.UserControls.AreaManagement.SummarizedAreaCards;

public sealed partial class SummarizedReservedCard : UserControl
{
    private bool _isPointerOver;

    public SummarizedReservedCard()
    {
        this.InitializeComponent();
    }

    private void HandleCardPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        _isPointerOver = true;
        ApplyVisualState("SummarizedAreaHoverBackgroundBrush");
    }

    private void HandleCardPointerExited(object sender, PointerRoutedEventArgs e)
    {
        _isPointerOver = false;
        ApplyDefaultVisualState();
    }

    private void HandleCardPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        ApplyVisualState("SummarizedAreaPressedBackgroundBrush");
    }

    private void HandleCardPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        ApplyVisualState(_isPointerOver
            ? "SummarizedAreaHoverBackgroundBrush"
            : "SummarizedReservedAreaBackgroundBrush");
    }

    private void HandleCardPointerCanceled(object sender, PointerRoutedEventArgs e)
    {
        ApplyDefaultVisualState();
    }

    private void HandleCardPointerCaptureLost(object sender, PointerRoutedEventArgs e)
    {
        ApplyVisualState(_isPointerOver
            ? "SummarizedAreaHoverBackgroundBrush"
            : "SummarizedReservedAreaBackgroundBrush");
    }

    private void ApplyDefaultVisualState()
    {
        ApplyVisualState("SummarizedReservedAreaBackgroundBrush");
    }

    private void ApplyVisualState(string backgroundBrushKey)
    {
        CardBorder.Background = ResolveBrush(backgroundBrushKey);
        CardBorder.BorderBrush = ResolveBrush(backgroundBrushKey);
    }

    private static Brush ResolveBrush(string resourceKey)
    {
        if (Microsoft.UI.Xaml.Application.Current?.Resources.TryGetValue(resourceKey, out var resource) == true &&
            resource is Brush brush)
        {
            return brush;
        }

        throw new KeyNotFoundException($"Brush resource '{resourceKey}' was not found.");
    }
}
