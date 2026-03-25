using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WinUI.UIModels;

public partial class MenuItemModel : ObservableObject
{
    public string ResourceKey { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public string DialogKey { get; set; } = string.Empty;

    public ICommand? OnMenuItemSelectedCommand { get; set; }

    public bool IsExit { get; set; }

    public bool RequiresConfig { get; set; }

    public bool HideWhenConfigured { get; set; }

    [ObservableProperty]
    public partial string DisplayText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsVisible { get; set; } = true;
}
