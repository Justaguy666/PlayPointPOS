using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace WinUI.Views.Panels;

public sealed class ResponsiveFormPanel : Panel
{
    public static readonly DependencyProperty ColumnSpacingProperty =
        DependencyProperty.Register(
            nameof(ColumnSpacing),
            typeof(double),
            typeof(ResponsiveFormPanel),
            new PropertyMetadata(16d, InvalidatePanelMeasure));

    public static readonly DependencyProperty RowSpacingProperty =
        DependencyProperty.Register(
            nameof(RowSpacing),
            typeof(double),
            typeof(ResponsiveFormPanel),
            new PropertyMetadata(22d, InvalidatePanelMeasure));

    public static readonly DependencyProperty MinColumnWidthProperty =
        DependencyProperty.Register(
            nameof(MinColumnWidth),
            typeof(double),
            typeof(ResponsiveFormPanel),
            new PropertyMetadata(260d, InvalidatePanelMeasure));

    public static readonly DependencyProperty IsFullWidthProperty =
        DependencyProperty.RegisterAttached(
            "IsFullWidth",
            typeof(bool),
            typeof(ResponsiveFormPanel),
            new PropertyMetadata(false, InvalidatePanelMeasure));

    public double ColumnSpacing
    {
        get => (double)GetValue(ColumnSpacingProperty);
        set => SetValue(ColumnSpacingProperty, value);
    }

    public double RowSpacing
    {
        get => (double)GetValue(RowSpacingProperty);
        set => SetValue(RowSpacingProperty, value);
    }

    public double MinColumnWidth
    {
        get => (double)GetValue(MinColumnWidthProperty);
        set => SetValue(MinColumnWidthProperty, value);
    }

    public static bool GetIsFullWidth(UIElement element)
    {
        return (bool)element.GetValue(IsFullWidthProperty);
    }

    public static void SetIsFullWidth(UIElement element, bool value)
    {
        element.SetValue(IsFullWidthProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double layoutWidth = ResolveLayoutWidth(availableSize.Width);
        int columns = GetColumnCount(layoutWidth);
        double columnWidth = GetColumnWidth(layoutWidth, columns);
        double totalHeight = 0;
        double rowHeight = 0;
        int rowCount = 0;
        int column = 0;
        bool hasOpenRow = false;

        foreach (UIElement child in Children)
        {
            if (child.Visibility == Visibility.Collapsed)
            {
                child.Measure(new Size(0, 0));
                continue;
            }

            bool isFullWidth = columns == 1 || GetIsFullWidth(child);
            if (isFullWidth)
            {
                FlushOpenMeasureRow(ref totalHeight, ref rowHeight, ref rowCount, ref column, ref hasOpenRow);

                child.Measure(new Size(layoutWidth, double.PositiveInfinity));
                AddMeasuredRow(child.DesiredSize.Height, ref totalHeight, ref rowCount);
                continue;
            }

            child.Measure(new Size(columnWidth, double.PositiveInfinity));
            rowHeight = Math.Max(rowHeight, child.DesiredSize.Height);
            column++;
            hasOpenRow = true;

            if (column == columns)
            {
                FlushOpenMeasureRow(ref totalHeight, ref rowHeight, ref rowCount, ref column, ref hasOpenRow);
            }
        }

        FlushOpenMeasureRow(ref totalHeight, ref rowHeight, ref rowCount, ref column, ref hasOpenRow);
        return new Size(layoutWidth, totalHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double layoutWidth = ResolveLayoutWidth(finalSize.Width);
        int columns = GetColumnCount(layoutWidth);
        double columnWidth = GetColumnWidth(layoutWidth, columns);
        double y = 0;
        double rowHeight = 0;
        int rowCount = 0;
        int column = 0;
        bool hasOpenRow = false;

        foreach (UIElement child in Children)
        {
            if (child.Visibility == Visibility.Collapsed)
            {
                child.Arrange(new Rect(0, y, 0, 0));
                continue;
            }

            bool isFullWidth = columns == 1 || GetIsFullWidth(child);
            if (isFullWidth)
            {
                FlushOpenArrangeRow(ref y, ref rowHeight, ref column, ref hasOpenRow);
                StartArrangeRow(ref y, ref rowCount);

                double childHeight = child.DesiredSize.Height;
                child.Arrange(new Rect(0, y, layoutWidth, childHeight));
                y += childHeight;
                continue;
            }

            if (!hasOpenRow)
            {
                StartArrangeRow(ref y, ref rowCount);
                hasOpenRow = true;
            }

            double x = column * (columnWidth + ColumnSpacing);
            child.Arrange(new Rect(x, y, columnWidth, child.DesiredSize.Height));
            rowHeight = Math.Max(rowHeight, child.DesiredSize.Height);
            column++;

            if (column == columns)
            {
                FlushOpenArrangeRow(ref y, ref rowHeight, ref column, ref hasOpenRow);
            }
        }

        FlushOpenArrangeRow(ref y, ref rowHeight, ref column, ref hasOpenRow);
        return new Size(layoutWidth, y);
    }

    private static void InvalidatePanelMeasure(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
        if (dependencyObject is ResponsiveFormPanel panel)
        {
            panel.InvalidateMeasure();
        }
    }

    private double ResolveLayoutWidth(double width)
    {
        if (!double.IsInfinity(width) && !double.IsNaN(width) && width > 0)
        {
            return width;
        }

        return (MinColumnWidth * 2) + ColumnSpacing;
    }

    private int GetColumnCount(double width)
    {
        return width >= ((MinColumnWidth * 2) + ColumnSpacing) ? 2 : 1;
    }

    private double GetColumnWidth(double width, int columns)
    {
        if (columns == 1)
        {
            return width;
        }

        return Math.Max(1, (width - ColumnSpacing) / columns);
    }

    private void AddMeasuredRow(double height, ref double totalHeight, ref int rowCount)
    {
        if (rowCount > 0)
        {
            totalHeight += RowSpacing;
        }

        totalHeight += height;
        rowCount++;
    }

    private void FlushOpenMeasureRow(
        ref double totalHeight,
        ref double rowHeight,
        ref int rowCount,
        ref int column,
        ref bool hasOpenRow)
    {
        if (!hasOpenRow)
        {
            return;
        }

        AddMeasuredRow(rowHeight, ref totalHeight, ref rowCount);
        rowHeight = 0;
        column = 0;
        hasOpenRow = false;
    }

    private void StartArrangeRow(ref double y, ref int rowCount)
    {
        if (rowCount > 0)
        {
            y += RowSpacing;
        }

        rowCount++;
    }

    private static void FlushOpenArrangeRow(
        ref double y,
        ref double rowHeight,
        ref int column,
        ref bool hasOpenRow)
    {
        if (!hasOpenRow)
        {
            return;
        }

        y += rowHeight;
        rowHeight = 0;
        column = 0;
        hasOpenRow = false;
    }
}
