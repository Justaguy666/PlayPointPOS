namespace WinUI.Services.Layout;

public interface IResponsiveLayoutService
{
    ResponsiveBreakpoint GetBreakpoint(double width);

    CardGridLayout CalculateCardGrid(
        double availableWidth,
        double preferredItemWidth,
        double itemHeight,
        double columnSpacing,
        int maxColumns,
        int wideRows,
        int compactPageSize);
}
