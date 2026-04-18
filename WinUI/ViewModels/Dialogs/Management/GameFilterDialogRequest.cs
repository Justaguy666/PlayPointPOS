using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Games;
using Domain.Entities;

namespace WinUI.ViewModels.Dialogs.Management;

public sealed class GameFilterDialogRequest
{
    public IReadOnlyList<GameType>? AvailableGameTypes { get; init; }

    public BoardGameFilter? InitialCriteria { get; init; }

    public Func<BoardGameFilter, Task>? OnSubmittedAsync { get; init; }
}
