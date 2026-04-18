using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Entities;

namespace WinUI.ViewModels.Dialogs.Management;

public partial class GameTypeDialogViewModel : LocalizedViewModelBase
{
    private readonly IDialogService _dialogService;
    private Func<GameType, Task>? _onGameTypeAddedAsync;
    private Func<GameType, Task>? _onGameTypeDeletedAsync;
    private Func<GameType, Task>? _onGameTypeUpdatedAsync;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAddGameType))]
    [NotifyCanExecuteChangedFor(nameof(AddGameTypeCommand))]
    public partial string NewGameTypeName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CloseTooltipText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AddButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NamePlaceholderText { get; set; } = string.Empty;

    public ObservableCollection<GameTypeItemViewModel> GameTypes { get; } = [];

    public event Action? CloseRequested;
    public event Action? DialogHideRequested;
    public event Action? DialogShowRequested;

    public bool CanAddGameType => !string.IsNullOrWhiteSpace(NewGameTypeName);

    public GameTypeDialogViewModel(ILocalizationService localizationService, IDialogService dialogService)
        : base(localizationService)
    {
        _dialogService = dialogService;
        RefreshLocalizedText();
    }

    protected override void RefreshLocalizedText()
    {
        Title = LocalizationService.GetString("GameTypeDialogTitleText") ?? "Quản lý danh mục";
        CloseTooltipText = LocalizationService.GetString("CloseTooltipText");
        AddButtonText = LocalizationService.GetString("GameTypeDialogAddButtonText") ?? "Thêm";
        NamePlaceholderText = LocalizationService.GetString("GameTypeDialogNamePlaceholderText") ?? "Tên danh mục...";
    }

    public void Configure(GameTypeDialogRequest request)
    {
        _onGameTypeAddedAsync = request.OnGameTypeAddedAsync;
        _onGameTypeDeletedAsync = request.OnGameTypeDeletedAsync;
        _onGameTypeUpdatedAsync = request.OnGameTypeUpdatedAsync;

        GameTypes.Clear();
        foreach (var type in request.GameTypes)
        {
            var item = new GameTypeItemViewModel(type, this);
            GameTypes.Add(item);
        }
    }

    [RelayCommand]
    private void Close()
    {
        CloseRequested?.Invoke();
    }

    [RelayCommand(CanExecute = nameof(CanAddGameType))]
    private async Task AddGameTypeAsync()
    {
        if (string.IsNullOrWhiteSpace(NewGameTypeName))
        {
            return;
        }

        var newType = new GameType { Name = NewGameTypeName };
        
        if (_onGameTypeAddedAsync != null)
        {
            await _onGameTypeAddedAsync(newType);
        }
        
        GameTypes.Insert(0, new GameTypeItemViewModel(newType, this));
        NewGameTypeName = string.Empty;
    }

    public async Task DeleteGameTypeAsync(GameTypeItemViewModel item)
    {
        DialogHideRequested?.Invoke();

        bool isConfirmed;
        try
        {
            isConfirmed = await _dialogService.ShowConfirmationAsync(
                titleKey: "ConfirmDeleteGameTypeTitle",
                messageKey: "ConfirmDeleteGameTypeMessage",
                confirmButtonTextKey: "ConfirmDeleteGameTypeButton",
                cancelButtonTextKey: "CancelButtonText");
        }
        finally
        {
            DialogShowRequested?.Invoke();
        }

        if (isConfirmed)
        {
            if (_onGameTypeDeletedAsync != null)
            {
                await _onGameTypeDeletedAsync(item.GameType);
            }
            GameTypes.Remove(item);
        }
    }

    public async Task UpdateGameTypeAsync(GameTypeItemViewModel item, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName) || item.GameType.Name == newName)
            return;

        item.GameType.Name = newName;
        item.Name = newName;

        if (_onGameTypeUpdatedAsync != null)
        {
            await _onGameTypeUpdatedAsync(item.GameType);
        }
    }
}

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
        GameType = gameType;
        _parent = parent;
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
