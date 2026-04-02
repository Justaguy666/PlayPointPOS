using Application.UseCases.Analytics.Contracts.Enums;
using Microsoft.UI.Xaml.Media;

namespace WinUI.UIModels.Dashboard;

public class ChartTabItemModel
{
    public string Title { get; set; } = string.Empty;
    public ChartMetricType MetricType { get; set; }
    public double Width { get; set; }
    public bool IsHovered { get; set; }
    public bool IsSelected { get; set; }
    public Brush? Background { get; set; }
    public Brush? BorderBrush { get; set; }
    public Brush? Foreground { get; set; }
}
