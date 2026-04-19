using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Entities;

namespace WinUI.ViewModels.Dialogs.Management;

public partial class GameTypeItemViewModel : ObservableObject
{
    private readonly GameTypeDialogViewModel _parent;

    public GameType GameType { get; }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial bool IsEditing { get; set; }

    [ObservableProperty]
    public partial string EditName { get; set; } = string.Empty;

    public GameTypeItemViewModel(GameType gameType, GameTypeDialogViewModel parent)
    {
        GameType = gameType ?? throw new ArgumentNullException(nameof(gameType));
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        Name = gameType.Name ?? string.Empty;
    }

    [RelayCommand]
    private void BeginEdit()
    {
        EditName = Name;
        IsEditing = true;
    }

    [RelayCommand]
    private async Task CommitEditAsync()
    {
        if (IsEditing)
        {
            IsEditing = false;
            await _parent.UpdateGameTypeAsync(this, EditName);
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
    }

    [RelayCommand]
    private Task DeleteAsync()
    {
        return _parent.DeleteGameTypeAsync(this);
    }
}
