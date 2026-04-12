using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using WinUI.UIModels.Dashboard;

namespace WinUI.Views.UserControls.Dashboard;

public sealed partial class RevenueChartControl : UserControl
{
    public RevenueChartControl()
    {
        InitializeComponent();
    }

    private void ChartBarPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: ChartBarItemModel item })
        {
            item.IsHovered = true;
            item.Fill = item.HoverFill ?? item.Fill;
        }
    }

    private void ChartBarPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: ChartBarItemModel item })
        {
            item.IsHovered = false;
            item.Fill = item.DefaultFill ?? item.Fill;
        }
    }

    private void ExportButtonPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        ExportButtonChrome.Background = ResolveBrush("VeryLightGrayBrush");
    }

    private void ExportButtonPointerExited(object sender, PointerRoutedEventArgs e)
    {
        ExportButtonChrome.Background = ResolveBrush("WhiteBrush");
        ExportButtonChrome.BorderBrush = ResolveBrush("LightGrayBrush");
        ExportButtonIcon.Fill = ResolveBrush("BlackBrush");
    }

    private void MetricButtonPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Button { Content: Border border, DataContext: ChartTabItemModel item })
            return;

        if (item.IsSelected)
        {
            return;
        }

        ApplyInteractiveButtonHover(border);
    }

    private void MetricButtonPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Button { Content: Border border, DataContext: ChartTabItemModel item })
            return;

        RestoreInteractiveButtonState(border, item.Background, item.BorderBrush, item.Foreground);
    }

    private void TimeRangeButtonPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Button { Content: Border border, DataContext: TimeRangeItemModel item })
            return;

        if (item.IsSelected)
        {
            return;
        }

        ApplyInteractiveButtonHover(border);
    }

    private void TimeRangeButtonPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Button { Content: Border border, DataContext: TimeRangeItemModel item })
            return;

        RestoreInteractiveButtonState(border, item.Background, item.BorderBrush, item.Foreground);
    }

    private void ApplyInteractiveButtonHover(Border border)
    {
        border.Background = ResolveBrush("VeryLightGrayBrush");
    }

    private static void RestoreInteractiveButtonState(
        Border border,
        Brush? background,
        Brush? borderBrush,
        Brush? foreground)
    {
        border.Background = background;
        border.BorderBrush = borderBrush;

        if (border.Child is TextBlock textBlock)
        {
            textBlock.Foreground = foreground;
        }
    }

    private static Brush ResolveBrush(string resourceKey)
    {
        if (Microsoft.UI.Xaml.Application.Current?.Resources.TryGetValue(resourceKey, out var resource) == true &&
            resource is Brush brush)
        {
            return brush;
        }

        throw new InvalidOperationException($"Brush resource '{resourceKey}' was not found.");
    }
}
