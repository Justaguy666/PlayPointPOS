using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Domain.Entities;

namespace WinUI.ViewModels.Dialogs.Management;

public class GameTypeDialogRequest
{
    public required ObservableCollection<GameType> GameTypes { get; init; }

    public Func<GameType, Task>? OnGameTypeAddedAsync { get; init; }

    public Func<GameType, Task>? OnGameTypeDeletedAsync { get; init; }

    public Func<GameType, Task>? OnGameTypeUpdatedAsync { get; init; }
}
