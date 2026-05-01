using System.Windows.Input;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using WinUI.UIModels.Enums;
using WinUI.Services.Dialogs;

namespace WinUI.UIModels;

public partial class MenuItemModel : ObservableObject
{
    public string LabelResourceKey { get; set; } = string.Empty;

    public IconKind Icon { get; set; }

    public DialogKey? DialogKey { get; set; }

    public ICommand? OnMenuItemSelectedCommand { get; set; }

    public bool IsExit { get; set; }

    public bool RequiresConfig { get; set; }

    public bool HideWhenConfigured { get; set; }

    [ObservableProperty]
    public partial string Label { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsVisible { get; set; } = true;
}
