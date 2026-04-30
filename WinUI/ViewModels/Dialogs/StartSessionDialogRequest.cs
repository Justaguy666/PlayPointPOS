using WinUI.UIModels.Management;

namespace WinUI.ViewModels.Dialogs;

public sealed class StartSessionDialogRequest
{
    public required AreaModel Model { get; init; }
}
