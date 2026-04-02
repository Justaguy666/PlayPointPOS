using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace WinUI.UIModels.Dashboard;

public class PopularItemModel
{
    public int Position { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Times { get; set; }
    public string Revenue { get; set; } = string.Empty;
    public Brush? PositionBackground { get; set; }
}
