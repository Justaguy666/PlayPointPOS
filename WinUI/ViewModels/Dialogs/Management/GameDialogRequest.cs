using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;
using WinUI.UIModels.Enums;
using WinUI.UIModels.Management;

namespace WinUI.ViewModels.Dialogs.Management;

public sealed class GameDialogRequest
{
    public UpsertDialogMode Mode { get; init; } = UpsertDialogMode.Add;

    public GameModel? Model { get; init; }

    public IReadOnlyList<GameType>? AvailableGameTypes { get; init; }

    public Func<GameModel, Task>? OnSubmittedAsync { get; init; }
}
