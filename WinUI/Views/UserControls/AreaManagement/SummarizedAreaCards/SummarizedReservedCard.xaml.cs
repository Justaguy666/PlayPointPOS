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
        ApplyHoverVisualState();
    }

    private void HandleCardPointerExited(object sender, PointerRoutedEventArgs e)
    {
        _isPointerOver = false;
        ApplyDefaultVisualState();
    }

    private void HandleCardPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        ApplyPressedVisualState();
    }

    private void HandleCardPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (_isPointerOver)
        {
            ApplyHoverVisualState();
            return;
        }

        ApplyDefaultVisualState();
    }

    private void HandleCardPointerCanceled(object sender, PointerRoutedEventArgs e)
    {
        ApplyDefaultVisualState();
    }

    private void HandleCardPointerCaptureLost(object sender, PointerRoutedEventArgs e)
    {
        if (_isPointerOver)
        {
            ApplyHoverVisualState();
            return;
        }

        ApplyDefaultVisualState();
    }

    private void ApplyDefaultVisualState()
    {
        ApplyVisualState("SummarizedReservedAreaBackgroundBrush", "SummarizedAreaBorderBrush");
    }

    private void ApplyHoverVisualState()
    {
        ApplyVisualState("SummarizedAreaHoverBackgroundBrush", "SummarizedAreaHoverBackgroundBrush");
    }

    private void ApplyPressedVisualState()
    {
        ApplyVisualState("SummarizedAreaPressedBackgroundBrush", "SummarizedAreaPressedBackgroundBrush");
    }

    private void ApplyVisualState(string backgroundBrushKey, string borderBrushKey)
    {
        CardBorder.Background = ResolveBrush(backgroundBrushKey);
        CardBorder.BorderBrush = ResolveBrush(borderBrushKey);
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
