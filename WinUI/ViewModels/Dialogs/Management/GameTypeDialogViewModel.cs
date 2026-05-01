using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Application.Services;
using Application.Services.Games;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Application.Games;
using WinUI.Services.Dialogs;

namespace WinUI.ViewModels.Dialogs.Management;

public partial class GameTypeDialogViewModel : LocalizedViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IGameTypeManagementService _gameTypeManagementService;
    private IList<GameType> _gameTypes = new List<GameType>();
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

    public GameTypeDialogViewModel(
        ILocalizationService localizationService,
        IDialogService dialogService,
        IGameTypeManagementService gameTypeManagementService)
        : base(localizationService)
    {
        _dialogService = dialogService;
        _gameTypeManagementService = gameTypeManagementService ?? throw new ArgumentNullException(nameof(gameTypeManagementService));
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
        _gameTypes = request.GameTypes;

        RefreshItems();
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

        GameType newType = _gameTypeManagementService.Add(_gameTypes, NewGameTypeName);
        
        if (_onGameTypeAddedAsync != null)
        {
            await _onGameTypeAddedAsync(newType);
        }
        
        RefreshItems();
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

            _gameTypeManagementService.Delete(_gameTypes, item.GameType);
            RefreshItems();
        }
    }

    public async Task UpdateGameTypeAsync(GameTypeItemViewModel item, string newName)
    {
        if (!_gameTypeManagementService.Update(item.GameType, newName))
        {
            return;
        }

        item.Name = item.GameType.Name;

        if (_onGameTypeUpdatedAsync != null)
        {
            await _onGameTypeUpdatedAsync(item.GameType);
        }

        RefreshItems();
    }

    private void RefreshItems()
    {
        GameTypes.Clear();
        foreach (GameType type in _gameTypes.OrderBy(type => type.Name, StringComparer.CurrentCultureIgnoreCase))
        {
            GameTypes.Add(new GameTypeItemViewModel(type, this));
        }
    }
}
