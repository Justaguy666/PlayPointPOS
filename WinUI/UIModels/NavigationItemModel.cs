using System;
using Application.Navigation;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WinUI.UIModels;

public partial class NavigationItemModel : ObservableObject
{
    public string Label { get; set; } = string.Empty;

    public string IconGlyph { get; set; } = string.Empty;

    public Type RequestType { get; set; } = null!;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
