using System;
using System.Collections.Generic;
using System.Text;

using WinUI.UIModels.Enums;

namespace WinUI.UIModels;

public class IconState
{
    public IconKind Kind { get; set; }
    public int Size { get; set; } = 24; // Default size
    public bool IsSelected { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool IsHovered { get; set; }
    public bool AlwaysFilled { get; set; } = false;
}
