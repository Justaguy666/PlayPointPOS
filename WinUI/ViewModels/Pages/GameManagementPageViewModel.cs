using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Application.Games;
using Application.Services;
using Application.Services.Games;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Entities;
using Domain.Enums;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using WinUI.Resources;
using WinUI.Services.Factories;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.UIModels.Management;
using WinUI.ViewModels.Dialogs.Management;
using WinUI.ViewModels.UserControls;
using WinUI.ViewModels.UserControls.Games;

namespace WinUI.ViewModels.Pages;

public partial class GameManagementPageViewModel : LocalizedViewModelBase
{
    private const int GridPageSize = 9;
    private const int ListPageSize = 6;
    private const string SortFieldName = "name";
    private const string SortFieldPrice = "price";
    private const string SortFieldDifficulty = "difficulty";
    private const string SortFieldStock = "stock";
    private const string SortFieldPlayers = "players";
    private const string SortAscending = "asc";
    private const string SortDescending = "desc";
    private const double GameCardOuterWidth = 300;
    private const double GameCardOuterHeight = 460;
    private const double GameCardsColumnSpacing = 16;
    private const int PreferredGridGamesPerRow = 5;
    private const double LayoutPrecisionEpsilon = 0.5;

    private readonly IDialogService _dialogService;
    private readonly INotificationService _notificationService;
    private readonly IGameFilterService _gameFilterService;
    private readonly GameModelFactory _gameModelFactory;
    private readonly GameCardControlViewModelFactory _gameCardControlViewModelFactory;
    private readonly Brush _selectedBackgroundBrush;
    private readonly Brush _selectedForegroundBrush;
    private readonly Brush _unselectedBackgroundBrush;
    private readonly Brush _unselectedForegroundBrush;
    private readonly List<GameModel> _allGames;
    private readonly List<GameType> _allGameTypes;
    private readonly Dictionary<GameModel, GridGameCardControlViewModel> _gridCardViewModelsByGame;
    private readonly Dictionary<GameModel, ListGameCardControlViewModel> _listCardViewModelsByGame;
    private readonly PaginationModel _pagination;
    private IReadOnlyList<GameModel> _filteredGames = [];
    private GameType? _activeGameTypeFilter;
    private GameDifficulty? _activeDifficultyFilter;
    private int? _activePlayerCountFilter;
    private decimal? _activeHourlyPriceMinFilter;
    private decimal? _activeHourlyPriceMaxFilter;
    private bool _isUpdatingSelectionOptions;
    private bool _isInitialized;
    private bool _isDisposed;

    [ObservableProperty]
    public partial bool IsGridView { get; set; } = true;

    private int _gridGamesMaximumRowsOrColumns = 5;
    public int GridGamesMaximumRowsOrColumns
    {
        get => _gridGamesMaximumRowsOrColumns;
        set => SetProperty(ref _gridGamesMaximumRowsOrColumns, value);
    }

    private double _gridGamesMinItemWidth = 300;
    public double GridGamesMinItemWidth
    {
        get => _gridGamesMinItemWidth;
        set => SetProperty(ref _gridGamesMinItemWidth, value);
    }

    private double _gridGamesMinItemHeight = 460;
    public double GridGamesMinItemHeight
    {
        get => _gridGamesMinItemHeight;
        set => SetProperty(ref _gridGamesMinItemHeight, value);
    }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SearchPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SortFieldPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SortDirectionPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AddGameButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ManageGameTypesButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FilterButtonText { get; set; } = string.Empty;


    [ObservableProperty]
    public partial string PageInfoText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NoGamesText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? SelectedSortField { get; set; } = SortFieldName;

    [ObservableProperty]
    public partial string? SelectedSortDirection { get; set; } = SortAscending;

    [ObservableProperty]
    public partial IconState GridIconState { get; set; }

    [ObservableProperty]
    public partial IconState ListIconState { get; set; }

    [ObservableProperty]
    public partial IconState FilterIconState { get; set; } = new() { Kind = IconKind.Filter, Size = 20, AlwaysFilled = false };

    [ObservableProperty]
    public partial IconState AddIconState { get; set; } = new() { Kind = IconKind.Add, Size = 20, AlwaysFilled = true };

    [ObservableProperty]
    public partial Brush GridButtonBackgroundBrush { get; set; } = new SolidColorBrush(AppColors.White);

    [ObservableProperty]
    public partial Brush GridButtonForegroundBrush { get; set; } = new SolidColorBrush(AppColors.Black);

    [ObservableProperty]
    public partial Brush ListButtonBackgroundBrush { get; set; } = new SolidColorBrush(AppColors.White);

    [ObservableProperty]
    public partial Brush ListButtonForegroundBrush { get; set; } = new SolidColorBrush(AppColors.Black);

    public IconState NoGamesIconState { get; } = new()
    {
        Kind = IconKind.Dice,
        Size = 52,
        AlwaysFilled = true,
    };

    private Color _gridButtonBackgroundHoverColor = AppColors.OrangeFocus;
    public Color GridButtonBackgroundHoverColor
    {
        get => _gridButtonBackgroundHoverColor;
        set => SetProperty(ref _gridButtonBackgroundHoverColor, value);
    }

    private Color _gridButtonForegroundHoverColor = AppColors.White;
    public Color GridButtonForegroundHoverColor
    {
        get => _gridButtonForegroundHoverColor;
        set => SetProperty(ref _gridButtonForegroundHoverColor, value);
    }

    private Color _listButtonBackgroundHoverColor = AppColors.VeryLightGray;
    public Color ListButtonBackgroundHoverColor
    {
        get => _listButtonBackgroundHoverColor;
        set => SetProperty(ref _listButtonBackgroundHoverColor, value);
    }

    private Color _listButtonForegroundHoverColor = AppColors.Black;
    public Color ListButtonForegroundHoverColor
    {
        get => _listButtonForegroundHoverColor;
        set => SetProperty(ref _listButtonForegroundHoverColor, value);
    }

    public ObservableCollection<GridGameCardControlViewModel> PagedGridGameCards { get; } = [];

    public ObservableCollection<ListGameCardControlViewModel> PagedListGameCards { get; } = [];

    public ObservableCollection<LocalizationOptionModel> SortFieldOptions { get; } = [];

    public ObservableCollection<LocalizationOptionModel> SortDirectionOptions { get; } = [];

    public PaginationControlViewModel PaginationViewModel { get; }

    public PaginationModel Pagination => _pagination;

    public IconKind SearchIconKind => IconKind.Search;

    public bool HasGames => _filteredGames.Count > 0;

    public IRelayCommand ToggleGridViewCommand { get; }

    public IRelayCommand ToggleListViewCommand { get; }

    public IAsyncRelayCommand FilterCommand { get; }

    public IAsyncRelayCommand AddGameCommand { get; }

    public IAsyncRelayCommand ManageGameTypesCommand { get; }

    public IRelayCommand ClearSearchCommand { get; }

    public GameManagementPageViewModel(
        ILocalizationService localizationService,
        IDialogService dialogService,
        INotificationService notificationService,
        IGameCatalogService gameCatalogService,
        IGameTypeCatalogService gameTypeCatalogService,
        IGameFilterService gameFilterService,
        GameModelFactory gameModelFactory,
        GameCardControlViewModelFactory gameCardControlViewModelFactory,
        PaginationControlViewModel paginationViewModel)
        : base(localizationService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _gameFilterService = gameFilterService ?? throw new ArgumentNullException(nameof(gameFilterService));
        _gameModelFactory = gameModelFactory ?? throw new ArgumentNullException(nameof(gameModelFactory));
        _gameCardControlViewModelFactory = gameCardControlViewModelFactory ?? throw new ArgumentNullException(nameof(gameCardControlViewModelFactory));
        PaginationViewModel = paginationViewModel ?? throw new ArgumentNullException(nameof(paginationViewModel));
        ArgumentNullException.ThrowIfNull(gameCatalogService);
        ArgumentNullException.ThrowIfNull(gameTypeCatalogService);

        _selectedBackgroundBrush = AppResourceLookup.GetBrush("OrangeFocusBrush", AppColors.OrangeFocus);
        _selectedForegroundBrush = AppResourceLookup.GetBrush("WhiteBrush", AppColors.White);
        _unselectedBackgroundBrush = AppResourceLookup.GetBrush("WhiteBrush", AppColors.White);
        _unselectedForegroundBrush = AppResourceLookup.GetBrush("BlackBrush", AppColors.Black);

        _pagination = new PaginationModel
        {
            CurrentPage = 1,
            PageSize = GridPageSize,
            MaxVisiblePageButtons = 4,
        };

        PaginationViewModel.Pagination = _pagination;
        _pagination.PropertyChanged += HandlePaginationPropertyChanged;

        GridIconState = new IconState { Kind = IconKind.Grid, Size = 24, AlwaysFilled = true };
        ListIconState = new IconState { Kind = IconKind.List, Size = 24, AlwaysFilled = false };

        GridButtonBackgroundBrush = _selectedBackgroundBrush;
        GridButtonForegroundBrush = _selectedForegroundBrush;
        ListButtonBackgroundBrush = _unselectedBackgroundBrush;
        ListButtonForegroundBrush = _unselectedForegroundBrush;

        ToggleGridViewCommand = new RelayCommand(ExecuteToggleGridView);
        ToggleListViewCommand = new RelayCommand(ExecuteToggleListView);
        FilterCommand = new AsyncRelayCommand(OpenFilterDialogAsync);
        AddGameCommand = new AsyncRelayCommand(ExecuteAddGameAsync);
        ManageGameTypesCommand = new AsyncRelayCommand(ExecuteManageGameTypesAsync);
        ClearSearchCommand = new RelayCommand(ClearSearch);

        _allGameTypes = gameTypeCatalogService
            .GetGameTypes()
            .ToList();
        _allGames = gameCatalogService
            .GetGames()
            .Select(_gameModelFactory.Create)
            .ToList();
        _gridCardViewModelsByGame = _allGames.ToDictionary(
            game => game,
            CreateGridCardViewModel);
        _listCardViewModelsByGame = _allGames.ToDictionary(
            game => game,
            CreateListCardViewModel);

        RefreshLocalizedText();
        _isInitialized = true;
        ApplyFiltersAndSorting(resetToFirstPage: true);
    }

    protected override void RefreshLocalizedText()
    {
        AddGameButtonText = LocalizationService.GetString("GameAddNewGameLabel");
        ManageGameTypesButtonText = LocalizationService.GetString("GameManagementPageManageGameTypesButton");
        FilterButtonText = LocalizationService.GetString("GameManagementPageFilterButton");
        SearchPlaceholderText = LocalizationService.GetString("SearchPlaceholderText");
        SortFieldPlaceholderText = LocalizationService.GetString("GameManagementPageSortFieldComboBox.PlaceholderText");
        SortDirectionPlaceholderText = LocalizationService.GetString("GameManagementPageSortDirectionComboBox.PlaceholderText");
        NoGamesText = LocalizationService.GetString("GameManagementPageNoGamesText");

        RefreshSortOptions();
        UpdatePageMetadata();
    }

    public new void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _pagination.PropertyChanged -= HandlePaginationPropertyChanged;
        foreach (GridGameCardControlViewModel viewModel in _gridCardViewModelsByGame.Values)
        {
            viewModel.Dispose();
        }

        foreach (ListGameCardControlViewModel viewModel in _listCardViewModelsByGame.Values)
        {
            viewModel.Dispose();
        }

        PaginationViewModel.Dispose();
        _isDisposed = true;
        base.Dispose();
    }

    partial void OnSearchTextChanged(string value)
    {
        if (!_isInitialized)
        {
            return;
        }

        ApplyFiltersAndSorting(resetToFirstPage: true);
    }

    partial void OnSelectedSortFieldChanged(string? value)
    {
        if (!_isInitialized || _isUpdatingSelectionOptions)
        {
            return;
        }

        ApplyFiltersAndSorting(resetToFirstPage: true);
    }

    partial void OnSelectedSortDirectionChanged(string? value)
    {
        if (!_isInitialized || _isUpdatingSelectionOptions)
        {
            return;
        }

        ApplyFiltersAndSorting(resetToFirstPage: true);
    }

    public void UpdateGridGamesLayout(double availableWidth)
    {
        if (availableWidth <= 0)
        {
            return;
        }

        int columns = CalculateGridGamesColumnCount(availableWidth);
        double totalSpacing = GameCardsColumnSpacing * (columns - 1);

        GridGamesMaximumRowsOrColumns = columns;
        GridGamesMinItemWidth = Math.Floor((availableWidth - totalSpacing + LayoutPrecisionEpsilon) / columns);
        GridGamesMinItemHeight = GameCardOuterHeight;

        if (IsGridView)
        {
            int newPageSize = columns >= 3 ? columns * 3 : 10;
            SetPageSize(newPageSize);
        }
    }

    private void SetPageSize(int newPageSize)
    {
        if (_pagination.PageSize == newPageSize)
        {
            return;
        }

        _pagination.PageSize = newPageSize;

        int totalPages = Math.Max(1, (int)Math.Ceiling(_filteredGames.Count / (double)Math.Max(_pagination.PageSize, 1)));
        if (_pagination.CurrentPage > totalPages)
        {
            _pagination.CurrentPage = totalPages;
        }
    }

    private void HandlePaginationPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(PaginationModel.CurrentPage) or
            nameof(PaginationModel.PageSize) or
            nameof(PaginationModel.TotalItems))
        {
            RefreshPagedGames();
        }
    }

    private void ExecuteToggleGridView()
    {
        if (IsGridView)
        {
            return;
        }

        IsGridView = true;
        GridIconState = new IconState { Kind = IconKind.Grid, Size = 24, AlwaysFilled = true };
        ListIconState = new IconState { Kind = IconKind.List, Size = 24, AlwaysFilled = false };

        GridButtonBackgroundBrush = _selectedBackgroundBrush;
        GridButtonForegroundBrush = _selectedForegroundBrush;
        ListButtonBackgroundBrush = _unselectedBackgroundBrush;
        ListButtonForegroundBrush = _unselectedForegroundBrush;

        GridButtonBackgroundHoverColor = AppColors.OrangeFocus;
        GridButtonForegroundHoverColor = AppColors.White;
        ListButtonBackgroundHoverColor = AppColors.VeryLightGray;
        ListButtonForegroundHoverColor = AppColors.Black;

        int columns = GridGamesMaximumRowsOrColumns;
        SetPageSize(columns >= 3 ? columns * 3 : 10);
    }

    private void ExecuteToggleListView()
    {
        if (!IsGridView)
        {
            return;
        }

        IsGridView = false;
        GridIconState = new IconState { Kind = IconKind.Grid, Size = 24, AlwaysFilled = false };
        ListIconState = new IconState { Kind = IconKind.List, Size = 24, AlwaysFilled = true };

        GridButtonBackgroundBrush = _unselectedBackgroundBrush;
        GridButtonForegroundBrush = _unselectedForegroundBrush;
        ListButtonBackgroundBrush = _selectedBackgroundBrush;
        ListButtonForegroundBrush = _selectedForegroundBrush;

        GridButtonBackgroundHoverColor = AppColors.VeryLightGray;
        GridButtonForegroundHoverColor = AppColors.Black;
        ListButtonBackgroundHoverColor = AppColors.OrangeFocus;
        ListButtonForegroundHoverColor = AppColors.White;

        SetPageSize(ListPageSize);
    }

    private async Task OpenFilterDialogAsync()
    {
        await _dialogService.ShowDialogAsync(
            "GameFilter",
            new GameFilterDialogRequest
            {
                AvailableGameTypes = _allGameTypes,
                InitialCriteria = BuildCurrentFilterCriteria(),
                OnSubmittedAsync = HandleFilterSubmittedAsync,
            });
    }

    private async Task ExecuteAddGameAsync()
    {
        await _dialogService.ShowDialogAsync(
            "Game",
            new GameDialogRequest
            {
                Mode = UpsertDialogMode.Add,
                Model = CreateNewGameDraft(),
                AvailableGameTypes = _allGameTypes,
                OnSubmittedAsync = HandleGameCreatedAsync,
            });
    }

    private async Task ExecuteManageGameTypesAsync()
    {
        var gameTypesCollection = new ObservableCollection<GameType>(_allGameTypes);

        await _dialogService.ShowDialogAsync(
            "GameType",
            new GameTypeDialogRequest
            {
                GameTypes = gameTypesCollection,
                OnGameTypeAddedAsync = gameType =>
                {
                    if (!_allGameTypes.Contains(gameType))
                    {
                        _allGameTypes.Add(gameType);
                    }
                    return Task.CompletedTask;
                },
                OnGameTypeDeletedAsync = gameType =>
                {
                    _allGameTypes.Remove(gameType);
                    return Task.CompletedTask;
                },
                OnGameTypeUpdatedAsync = gameType => Task.CompletedTask
            });
    }

    private void ClearSearch()
    {
        SearchText = string.Empty;
    }

    private Task HandleEditGameAsync(GameModel game)
    {
        if (game is null)
        {
            return Task.CompletedTask;
        }

        return _dialogService.ShowDialogAsync(
            "Game",
            new GameDialogRequest
            {
                Mode = UpsertDialogMode.Edit,
                Model = game,
                AvailableGameTypes = _allGameTypes,
                OnSubmittedAsync = HandleGameUpdatedAsync,
            });
    }

    private async Task HandleDeleteGameAsync(GameModel game)
    {
        if (game is null)
        {
            return;
        }

        bool isConfirmed = await _dialogService.ShowConfirmationAsync(
            titleKey: "ConfirmDeleteGameTitle",
            messageKey: "ConfirmDeleteGameMessage",
            confirmButtonTextKey: "ConfirmDeleteGameButton",
            cancelButtonTextKey: "CancelButtonText");

        if (!isConfirmed)
        {
            return;
        }

        if (!_allGames.Remove(game))
        {
            return;
        }

        RemoveCardViewModelsForGame(game);
        ApplyFiltersAndSorting(resetToFirstPage: false);

        await _notificationService.SendAsync(
            LocalizationService.GetString("GameDeletedSuccessTitle"),
            string.Format(
                LocalizationService.Culture,
                LocalizationService.GetString("GameDeletedSuccessMessage"),
                game.Name),
            NotificationType.Success);
    }

    private void HandleIncreaseStock(GameModel game)
    {
        game.StockQuantity += 1;
    }

    private void HandleDecreaseStock(GameModel game)
    {
        if (game.StockQuantity <= 0)
        {
            return;
        }

        game.StockQuantity -= 1;
    }

    private Task HandleFilterSubmittedAsync(BoardGameFilter criteria)
    {
        _activeGameTypeFilter = criteria.GameType;
        _activeDifficultyFilter = criteria.GameDifficulty;
        _activePlayerCountFilter = criteria.PlayerCount;
        _activeHourlyPriceMinFilter = criteria.HourlyPriceMin;
        _activeHourlyPriceMaxFilter = criteria.HourlyPriceMax;

        ApplyFiltersAndSorting(resetToFirstPage: true);
        return Task.CompletedTask;
    }

    private async Task HandleGameCreatedAsync(GameModel game)
    {
        if (!_allGames.Contains(game))
        {
            _allGames.Insert(0, game);
            AddCardViewModelsForGame(game);
        }

        ApplyFiltersAndSorting(resetToFirstPage: true);

        await _notificationService.SendAsync(
            LocalizationService.GetString("GameCreatedSuccessTitle"),
            string.Format(
                LocalizationService.Culture,
                LocalizationService.GetString("GameCreatedSuccessMessage"),
                game.Name),
            NotificationType.Success);
    }

    private async Task HandleGameUpdatedAsync(GameModel game)
    {
        ApplyFiltersAndSorting(resetToFirstPage: false);

        await _notificationService.SendAsync(
            LocalizationService.GetString("GameUpdatedSuccessTitle"),
            string.Format(
                LocalizationService.Culture,
                LocalizationService.GetString("GameUpdatedSuccessMessage"),
                game.Name),
            NotificationType.Success);
    }

    private void ApplyFiltersAndSorting(bool resetToFirstPage)
    {
        IReadOnlyList<GameModel> filteredGames = _gameFilterService.Apply(_allGames, BuildCurrentFilterCriteria());

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filteredGames = filteredGames
                .Where(MatchesSearch)
                .ToList();
        }

        _filteredGames = SortGames(filteredGames);
        _pagination.TotalItems = _filteredGames.Count;

        if (resetToFirstPage)
        {
            if (_pagination.CurrentPage != 1)
            {
                _pagination.CurrentPage = 1;
                return;
            }

            RefreshPagedGames();
            return;
        }

        RefreshPagedGames();
    }

    private BoardGameFilter BuildCurrentFilterCriteria()
    {
        return new BoardGameFilter
        {
            GameType = _activeGameTypeFilter,
            GameDifficulty = _activeDifficultyFilter,
            PlayerCount = _activePlayerCountFilter,
            HourlyPriceMin = _activeHourlyPriceMinFilter,
            HourlyPriceMax = _activeHourlyPriceMaxFilter,
        };
    }

    private bool MatchesSearch(GameModel game)
    {
        return LocalizationService.Culture.CompareInfo.IndexOf(game.Name, SearchText, CompareOptions.IgnoreCase) >= 0
            || LocalizationService.Culture.CompareInfo.IndexOf(game.GameType.Name, SearchText, CompareOptions.IgnoreCase) >= 0;
    }

    private IReadOnlyList<GameModel> SortGames(IEnumerable<GameModel> games)
    {
        bool isDescending = string.Equals(SelectedSortDirection, SortDescending, StringComparison.Ordinal);

        IOrderedEnumerable<GameModel> orderedGames = (SelectedSortField ?? SortFieldName) switch
        {
            SortFieldPrice => isDescending
                ? games.OrderByDescending(game => game.HourlyPrice).ThenBy(game => game.Name, StringComparer.CurrentCultureIgnoreCase)
                : games.OrderBy(game => game.HourlyPrice).ThenBy(game => game.Name, StringComparer.CurrentCultureIgnoreCase),
            SortFieldDifficulty => isDescending
                ? games.OrderByDescending(game => game.GameDifficulty).ThenBy(game => game.Name, StringComparer.CurrentCultureIgnoreCase)
                : games.OrderBy(game => game.GameDifficulty).ThenBy(game => game.Name, StringComparer.CurrentCultureIgnoreCase),
            SortFieldStock => isDescending
                ? games.OrderByDescending(game => game.StockQuantity).ThenBy(game => game.Name, StringComparer.CurrentCultureIgnoreCase)
                : games.OrderBy(game => game.StockQuantity).ThenBy(game => game.Name, StringComparer.CurrentCultureIgnoreCase),
            SortFieldPlayers => isDescending
                ? games.OrderByDescending(game => game.MaxPlayers).ThenByDescending(game => game.MinPlayers).ThenBy(game => game.Name, StringComparer.CurrentCultureIgnoreCase)
                : games.OrderBy(game => game.MinPlayers).ThenBy(game => game.MaxPlayers).ThenBy(game => game.Name, StringComparer.CurrentCultureIgnoreCase),
            _ => isDescending
                ? games.OrderByDescending(game => game.Name, StringComparer.CurrentCultureIgnoreCase)
                : games.OrderBy(game => game.Name, StringComparer.CurrentCultureIgnoreCase),
        };

        return orderedGames.ToList();
    }

    private void RefreshPagedGames()
    {
        PagedGridGameCards.Clear();
        PagedListGameCards.Clear();

        int startIndex = Math.Max(0, (_pagination.CurrentPage - 1) * _pagination.PageSize);
        foreach (GameModel game in _filteredGames.Skip(startIndex).Take(_pagination.PageSize))
        {
            if (_gridCardViewModelsByGame.TryGetValue(game, out GridGameCardControlViewModel? gridViewModel))
            {
                PagedGridGameCards.Add(gridViewModel);
            }

            if (_listCardViewModelsByGame.TryGetValue(game, out ListGameCardControlViewModel? listViewModel))
            {
                PagedListGameCards.Add(listViewModel);
            }
        }

        OnPropertyChanged(nameof(HasGames));
        UpdatePageMetadata();
    }

    private void RefreshSortOptions()
    {
        _isUpdatingSelectionOptions = true;

        try
        {
            string currentSortField = SelectedSortField ?? SortFieldName;
            string currentSortDirection = SelectedSortDirection ?? SortAscending;

            ReplaceOptions(
                SortFieldOptions,
                [
                    new LocalizationOptionModel { Value = SortFieldName, DisplayName = LocalizationService.GetString("GameManagementPageSortFieldNameOption") },
                    new LocalizationOptionModel { Value = SortFieldPrice, DisplayName = LocalizationService.GetString("GameManagementPageSortFieldPriceOption") },
                    new LocalizationOptionModel { Value = SortFieldDifficulty, DisplayName = LocalizationService.GetString("GameManagementPageSortFieldDifficultyOption") },
                    new LocalizationOptionModel { Value = SortFieldStock, DisplayName = LocalizationService.GetString("GameManagementPageSortFieldStockOption") },
                    new LocalizationOptionModel { Value = SortFieldPlayers, DisplayName = LocalizationService.GetString("GameManagementPageSortFieldPlayersOption") },
                ]);

            ReplaceOptions(
                SortDirectionOptions,
                [
                    new LocalizationOptionModel { Value = SortAscending, DisplayName = LocalizationService.GetString("GameManagementPageSortDirectionAscendingOption") },
                    new LocalizationOptionModel { Value = SortDescending, DisplayName = LocalizationService.GetString("GameManagementPageSortDirectionDescendingOption") },
                ]);

            SelectedSortField = SortFieldOptions.Any(option => option.Value == currentSortField)
                ? currentSortField
                : SortFieldName;
            SelectedSortDirection = SortDirectionOptions.Any(option => option.Value == currentSortDirection)
                ? currentSortDirection
                : SortAscending;
        }
        finally
        {
            _isUpdatingSelectionOptions = false;
        }
    }

    private void UpdatePageMetadata()
    {
        int totalItems = _filteredGames.Count;
        int startItem = totalItems == 0 ? 0 : ((_pagination.CurrentPage - 1) * _pagination.PageSize) + 1;
        int endItem = totalItems == 0 ? 0 : Math.Min(_pagination.CurrentPage * _pagination.PageSize, totalItems);
        int totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)Math.Max(_pagination.PageSize, 1)));

        PageInfoText = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("GameManagementPagePageInfoFormat"),
            startItem,
            endItem,
            totalItems,
            _pagination.CurrentPage,
            totalPages);
    }

    private GridGameCardControlViewModel CreateGridCardViewModel(GameModel game)
    {
        return _gameCardControlViewModelFactory.CreateGrid(
            game,
            HandleEditGameAsync,
            HandleDeleteGameAsync,
            HandleIncreaseStock,
            HandleDecreaseStock);
    }

    private ListGameCardControlViewModel CreateListCardViewModel(GameModel game)
    {
        return _gameCardControlViewModelFactory.CreateList(
            game,
            HandleEditGameAsync,
            HandleDeleteGameAsync,
            HandleIncreaseStock,
            HandleDecreaseStock);
    }

    private void AddCardViewModelsForGame(GameModel game)
    {
        _gridCardViewModelsByGame[game] = CreateGridCardViewModel(game);
        _listCardViewModelsByGame[game] = CreateListCardViewModel(game);
    }

    private void RemoveCardViewModelsForGame(GameModel game)
    {
        if (_gridCardViewModelsByGame.Remove(game, out GridGameCardControlViewModel? gridViewModel))
        {
            gridViewModel.Dispose();
        }

        if (_listCardViewModelsByGame.Remove(game, out ListGameCardControlViewModel? listViewModel))
        {
            listViewModel.Dispose();
        }
    }

    private GameModel CreateNewGameDraft()
    {
        return new GameModel
        {
            Name = string.Empty,
            GameType = _allGameTypes.FirstOrDefault() ?? new GameType { Name = string.Empty },
            GameDifficulty = GameDifficulty.Medium,
            MinPlayers = 2,
            MaxPlayers = 4,
            HourlyPrice = 0m,
            StockQuantity = 1,
            BorrowedQuantity = 0,
            ImageUri = "ms-appx:///Assets/Mock.png",
        };
    }

    private static void ReplaceOptions(
        ObservableCollection<LocalizationOptionModel> collection,
        IReadOnlyList<LocalizationOptionModel> items)
    {
        collection.Clear();
        foreach (LocalizationOptionModel item in items)
        {
            collection.Add(item);
        }
    }

    private static int CalculateGridGamesColumnCount(double availableWidth)
    {
        if (availableWidth < GameCardOuterWidth)
        {
            return 1;
        }

        int maxPossibleColumns = (int)((availableWidth + GameCardsColumnSpacing) / (GameCardOuterWidth + GameCardsColumnSpacing));
        return Math.Max(1, Math.Min(PreferredGridGamesPerRow, maxPossibleColumns));
    }
}
