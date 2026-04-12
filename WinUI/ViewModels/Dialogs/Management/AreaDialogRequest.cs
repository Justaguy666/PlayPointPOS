using System;
using System.Threading.Tasks;
using WinUI.UIModels.AreaManagement;
using WinUI.UIModels.Enums;

namespace WinUI.ViewModels.Dialogs.Management;

public sealed class AreaDialogRequest
{
    public UpsertDialogMode Mode { get; init; } = UpsertDialogMode.Add;

    public AreaModel? Model { get; init; }

    public Func<AreaModel, Task>? OnSubmittedAsync { get; init; }
}
