using System;

namespace WinUI.Services.Layout;

public sealed class ResponsiveLayoutService : IResponsiveLayoutService
{
    private const double LayoutPrecisionEpsilon = 0.5;

    public ResponsiveBreakpoint GetBreakpoint(double width)
    {
        return ResponsiveBreakpoints.FromWidth(width);
    }

    public CardGridLayout CalculateCardGrid(
        double availableWidth,
        double preferredItemWidth,
        double itemHeight,
        double columnSpacing,
        int maxColumns,
        int wideRows,
        int compactPageSize)
    {
        if (availableWidth <= 0)
        {
            return new CardGridLayout(1, preferredItemWidth, itemHeight, compactPageSize);
        }

        int possibleColumns = availableWidth < preferredItemWidth
            ? 1
            : (int)((availableWidth + columnSpacing) / (preferredItemWidth + columnSpacing));
        int columns = Math.Max(1, Math.Min(maxColumns, possibleColumns));
        double totalSpacing = columnSpacing * (columns - 1);
        double itemWidth = Math.Max(1, Math.Floor((availableWidth - totalSpacing + LayoutPrecisionEpsilon) / columns));
        int pageSize = columns >= 3
            ? columns * Math.Max(1, wideRows)
            : Math.Max(1, compactPageSize);

        return new CardGridLayout(columns, itemWidth, itemHeight, pageSize);
    }
}
