using Microsoft.UI.Xaml.Media;

namespace WinUI.UIModels;

public sealed class PaginationPageButtonModel
{
    public int PageNumber { get; init; }

    public string Label { get; init; } = string.Empty;

    public bool IsSelected { get; init; }

    public Brush? Background { get; init; }

    public Brush? BorderBrush { get; init; }

    public Brush? Foreground { get; init; }
}
