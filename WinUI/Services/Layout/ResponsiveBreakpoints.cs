namespace WinUI.Services.Layout;

public static class ResponsiveBreakpoints
{
    public const double CompactMaxWidth = 719;
    public const double MediumMinWidth = 720;
    public const double WideMinWidth = 1100;

    public const double PageHorizontalPadding = 24;
    public const double CompactPageHorizontalPadding = 16;

    public static ResponsiveBreakpoint FromWidth(double width)
    {
        if (width >= WideMinWidth)
        {
            return ResponsiveBreakpoint.Wide;
        }

        return width >= MediumMinWidth
            ? ResponsiveBreakpoint.Medium
            : ResponsiveBreakpoint.Compact;
    }

    public static bool IsCompact(double width) => FromWidth(width) == ResponsiveBreakpoint.Compact;
}
