using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;

namespace WinUI.UIModels.Dashboard;

public class ChartBarItemModel : ObservableObject
{
    private bool _isHovered;
    private Brush? _fill;

    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
    public string DisplayValue { get; set; } = string.Empty;
    public string PopupValue { get; set; } = string.Empty;
    public double NormalizedValue { get; set; }
    public bool IsHovered
    {
        get => _isHovered;
        set => SetProperty(ref _isHovered, value);
    }

    public bool IsMax { get; set; }
    public Brush? Fill
    {
        get => _fill;
        set => SetProperty(ref _fill, value);
    }

    public Brush? LabelForeground { get; set; }
    public Brush? CalloutBackground { get; set; }
    public Brush? CalloutForeground { get; set; }
    public Brush? DefaultFill { get; set; }
    public Brush? HoverFill { get; set; }
}
