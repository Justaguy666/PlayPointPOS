using System;
using CommunityToolkit.Mvvm.ComponentModel;
using WinUI.UIModels.Enums;

namespace WinUI.UIModels;

public partial class NavbarItemModel : ObservableObject
{
    [ObservableProperty]
    public partial string Label { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IconKind Icon { get; set; }

    [ObservableProperty]
    public partial IconState IconState { get; set; } = new() { Size = 20 };

    public required Type RequestType { get; init; }

    public required string LabelResourceKey { get; init; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    partial void OnIconChanged(IconKind value)
    {
        UpdateIconState();
    }

    partial void OnIsSelectedChanged(bool value)
    {
        UpdateIconState();
    }

    private void UpdateIconState()
    {
        int size = IconState?.Size > 0 ? IconState.Size : 20;
        IconState = new IconState
        {
            Kind = Icon,
            Size = size,
            IsSelected = IsSelected,
        };
    }
}
