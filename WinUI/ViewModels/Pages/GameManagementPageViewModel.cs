using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Application.Games;
using Application.Services;
using Application.Services.Games;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Enums;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using WinUI.Resources;
using WinUI.Services.Factories;
using WinUI.Services.Layout;
using WinUI.Services.Management;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.UIModels.Management;
using WinUI.ViewModels.Common;
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
    private const int GridRowsPerPage = 3;
    private const int CompactGridPageSize = 10;

    private readonly GameManagementDialogCoordinator _dialogs;
    private readonly IGameFilterService _gameFilterService;
    private readonly IResponsiveLayoutService _responsiveLayoutService;
    private readonly GameDraftFactory _draftFactory;
    private readonly GameCardControlViewModelFactory _cardFactory;
    private readonly List<GameType> _allGameTypes;
    private readonly ManagementQueryState<BoardGameFilter, ManagementSortState> _queryState;
    private readonly ManagementCollectionController<GameModel, object> _games;
    private readonly Brush _selectedBackgroundBrush;
    private readonly Brush _selectedForegroundBrush;
    private readonly Brush _unselectedBackgroundBrush;
    private readonly Brush _unselectedForegroundBrush;
    private bool _isUpdatingSelectionOptions;
    private bool _isInitialized;
    private bool _isDisposed;

    [ObservableProperty]
    public partial bool IsGridView { get; set; } = true;

    [ObservableProperty]
    public partial int GridGamesMaximumRowsOrColumns { get; set; } = PreferredGridGamesPerRow;

    [ObservableProperty]
    public partial double GridGamesMinItemWidth { get; set; } = GameCardOuterWidth;

    [ObservableProperty]
    public partial double GridGamesMinItemHeight { get; set; } = GameCardOuterHeight;

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

    [ObservableProperty]
    public partial Color GridButtonBackgroundHoverColor { get; set; } = AppColors.OrangeFocus;

    [ObservableProperty]
    public partial Color GridButtonForegroundHoverColor { get; set; } = AppColors.White;

    [ObservableProperty]
    public partial Color ListButtonBackgroundHoverColor { get; set; } = AppColors.VeryLightGray;

    [ObservableProperty]
    public partial Color ListButtonForegroundHoverColor { get; set; } = AppColors.Black;

    public IconState NoGamesIconState { get; } = new()
    {
        Kind = IconKind.Dice,
        Size = 52,
        AlwaysFilled = true,
    };

    public ObservableCollection<GridGameCardControlViewModel> PagedGridGameCards { get; } = [];

    public ObservableCollection<ListGameCardControlViewModel> PagedListGameCards { get; } = [];

    public ObservableCollection<LocalizationOptionModel> SortFieldOptions { get; } = [];

    public ObservableCollection<LocalizationOptionModel> SortDirectionOptions { get; } = [];

    public PaginationControlViewModel PaginationViewModel { get; }

    public PaginationModel Pagination { get; }

    public IconKind SearchIconKind => IconKind.Search;

    public bool HasGames => _games.HasItems;

    public IRelayCommand ToggleGridViewCommand { get; }

    public IRelayCommand ToggleListViewCommand { get; }

    public IAsyncRelayCommand FilterCommand { get; }

    public IAsyncRelayCommand AddGameCommand { get; }

    public IAsyncRelayCommand ManageGameTypesCommand { get; }

    public IRelayCommand ClearSearchCommand { get; }

    public GameManagementPageViewModel(
        ILocalizationService localizationService,
        GameManagementDialogCoordinator dialogs,
        IGameCatalogService gameCatalogService,
        IGameTypeCatalogService gameTypeCatalogService,
        IGameFilterService gameFilterService,
        IResponsiveLayoutService responsiveLayoutService,
        GameModelFactory gameModelFactory,
        GameDraftFactory draftFactory,
        GameCardControlViewModelFactory cardFactory,
        PaginationControlViewModel paginationViewModel)
        : base(localizationService)
    {
        _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
        _gameFilterService = gameFilterService ?? throw new ArgumentNullException(nameof(gameFilterService));
        _responsiveLayoutService = responsiveLayoutService ?? throw new ArgumentNullException(nameof(responsiveLayoutService));
        _draftFactory = draftFactory ?? throw new ArgumentNullException(nameof(draftFactory));
        _cardFactory = cardFactory ?? throw new ArgumentNullException(nameof(cardFactory));
        PaginationViewModel = paginationViewModel ?? throw new ArgumentNullException(nameof(paginationViewModel));
        ArgumentNullException.ThrowIfNull(gameCatalogService);
        ArgumentNullException.ThrowIfNull(gameTypeCatalogService);
        ArgumentNullException.ThrowIfNull(gameModelFactory);

        _selectedBackgroundBrush = AppResourceLookup.GetBrush("OrangeFocusBrush", AppColors.OrangeFocus);
        _selectedForegroundBrush = AppResourceLookup.GetBrush("WhiteBrush", AppColors.White);
        _unselectedBackgroundBrush = AppResourceLookup.GetBrush("WhiteBrush", AppColors.White);
        _unselectedForegroundBrush = AppResourceLookup.GetBrush("BlackBrush", AppColors.Black);

        Pagination = new PaginationModel { CurrentPage = 1, PageSize = GridPageSize, MaxVisiblePageButtons = 4 };
        PaginationViewModel.Pagination = Pagination;
        _queryState = new ManagementQueryState<BoardGameFilter, ManagementSortState>(
            new BoardGameFilter(),
            new ManagementSortState(SortFieldName, SortAscending));

        _allGameTypes = gameTypeCatalogService.GetGameTypes().ToList();
        List<GameModel> games = gameCatalogService.GetGames()
            .Select(gameModelFactory.Create)
            .ToList();
        _games = new ManagementCollectionController<GameModel, object>(
            games,
            Pagination,
            ClearPagedGameCards,
            AddPagedGameCard,
            QueryGames,
            CreateActiveCard,
            () => IsGridView ? "grid" : "list");
        _games.Refreshed += HandleGamesRefreshed;

        ApplyViewToggleState();
        ToggleGridViewCommand = new RelayCommand(ExecuteToggleGridView);
        ToggleListViewCommand = new RelayCommand(ExecuteToggleListView);
        FilterCommand = new AsyncRelayCommand(OpenFilterDialogAsync);
        AddGameCommand = new AsyncRelayCommand(ExecuteAddGameAsync);
        ManageGameTypesCommand = new AsyncRelayCommand(ExecuteManageGameTypesAsync);
        ClearSearchCommand = new RelayCommand(ClearSearch);

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

        _games.Refreshed -= HandleGamesRefreshed;
        _games.Dispose();
        PaginationViewModel.Dispose();
        _isDisposed = true;
        base.Dispose();
    }

    partial void OnSearchTextChanged(string value)
    {
        if (_isInitialized)
        {
            _queryState.SearchText = value;
            ApplyFiltersAndSorting(resetToFirstPage: true);
        }
    }

    partial void OnSelectedSortFieldChanged(string? value) => HandleSortChanged();

    partial void OnSelectedSortDirectionChanged(string? value) => HandleSortChanged();

    public void UpdateGridGamesLayout(double availableWidth)
    {
        if (availableWidth <= 0)
        {
            return;
        }

        CardGridLayout layout = _responsiveLayoutService.CalculateCardGrid(
            availableWidth,
            GameCardOuterWidth,
            GameCardOuterHeight,
            GameCardsColumnSpacing,
            PreferredGridGamesPerRow,
            GridRowsPerPage,
            CompactGridPageSize);

        GridGamesMaximumRowsOrColumns = layout.Columns;
        GridGamesMinItemWidth = layout.ItemWidth;
        GridGamesMinItemHeight = layout.ItemHeight;

        if (IsGridView)
        {
            _games.SetPageSize(layout.PageSize);
        }
    }

    private void HandleSortChanged()
    {
        if (!_isInitialized || _isUpdatingSelectionOptions)
        {
            return;
        }

        SyncSortState();
        ApplyFiltersAndSorting(resetToFirstPage: true);
    }

    private void ExecuteToggleGridView()
    {
        if (IsGridView)
        {
            return;
        }

        IsGridView = true;
        ApplyViewToggleState();
        _games.SetPageSize(GridGamesMaximumRowsOrColumns >= 3
            ? GridGamesMaximumRowsOrColumns * GridRowsPerPage
            : CompactGridPageSize);
        _games.RefreshPage();
    }

    private void ExecuteToggleListView()
    {
        if (!IsGridView)
        {
            return;
        }

        IsGridView = false;
        ApplyViewToggleState();
        _games.SetPageSize(ListPageSize);
        _games.RefreshPage();
    }

    private Task OpenFilterDialogAsync()
    {
        return _dialogs.OpenFilterAsync(_allGameTypes, _queryState.Filter, HandleFilterSubmittedAsync);
    }

    private Task ExecuteAddGameAsync()
    {
        return _dialogs.OpenAddAsync(_draftFactory.Create(_allGameTypes), _allGameTypes, HandleGameCreatedAsync);
    }

    private Task ExecuteManageGameTypesAsync()
    {
        return _dialogs.OpenGameTypesAsync(
            new ObservableCollection<GameType>(_allGameTypes),
            HandleGameTypeAddedAsync,
            HandleGameTypeDeletedAsync,
            HandleGameTypeUpdatedAsync);
    }

    private void ClearSearch()
    {
        SearchText = string.Empty;
    }

    private Task HandleEditGameAsync(GameModel game)
    {
        return game is null
            ? Task.CompletedTask
            : _dialogs.OpenEditAsync(game, _allGameTypes, HandleGameUpdatedAsync);
    }

    private async Task HandleDeleteGameAsync(GameModel game)
    {
        if (game is null || !await _dialogs.ConfirmDeleteAsync())
        {
            return;
        }

        if (!_games.Remove(game))
        {
            return;
        }

        ApplyFiltersAndSorting(resetToFirstPage: false);
        await _dialogs.NotifyDeletedAsync(game);
    }

    private void HandleIncreaseStock(GameModel game)
    {
        game.StockQuantity += 1;
    }

    private void HandleDecreaseStock(GameModel game)
    {
        if (game.StockQuantity > 0)
        {
            game.StockQuantity -= 1;
        }
    }

    private Task HandleFilterSubmittedAsync(BoardGameFilter criteria)
    {
        _queryState.Filter = criteria;
        ApplyFiltersAndSorting(resetToFirstPage: true);
        return Task.CompletedTask;
    }

    private async Task HandleGameCreatedAsync(GameModel game)
    {
        if (!_games.Contains(game))
        {
            _games.Insert(0, game);
        }

        ApplyFiltersAndSorting(resetToFirstPage: true);
        await _dialogs.NotifyCreatedAsync(game);
    }

    private async Task HandleGameUpdatedAsync(GameModel game)
    {
        ApplyFiltersAndSorting(resetToFirstPage: false);
        await _dialogs.NotifyUpdatedAsync(game);
    }

    private Task HandleGameTypeAddedAsync(GameType gameType)
    {
        if (!_allGameTypes.Contains(gameType))
        {
            _allGameTypes.Add(gameType);
        }

        return Task.CompletedTask;
    }

    private Task HandleGameTypeDeletedAsync(GameType gameType)
    {
        _allGameTypes.Remove(gameType);
        if (IsSameGameType(_queryState.Filter.GameType, gameType))
        {
            _queryState.Filter = _queryState.Filter with { GameType = null };
            ApplyFiltersAndSorting(resetToFirstPage: true);
        }

        return Task.CompletedTask;
    }

    private Task HandleGameTypeUpdatedAsync(GameType gameType)
    {
        ApplyFiltersAndSorting(resetToFirstPage: false);
        return Task.CompletedTask;
    }

    private void ApplyFiltersAndSorting(bool resetToFirstPage)
    {
        _queryState.SearchText = SearchText;
        SyncSortState();
        _games.Refresh(resetToFirstPage);
    }

    private IReadOnlyList<GameModel> QueryGames(IEnumerable<GameModel> source)
    {
        IEnumerable<GameModel> games = _gameFilterService.Apply(source, _queryState.Filter);

        if (!string.IsNullOrWhiteSpace(_queryState.SearchText))
        {
            games = games.Where(MatchesSearch);
        }

        return SortGames(games);
    }

    private bool MatchesSearch(GameModel game)
    {
        return ManagementCollectionFlow.ContainsSearchText(LocalizationService.Culture, game.Name, _queryState.SearchText)
            || ManagementCollectionFlow.ContainsSearchText(LocalizationService.Culture, game.GameType.Name, _queryState.SearchText);
    }

    private IReadOnlyList<GameModel> SortGames(IEnumerable<GameModel> games)
    {
        bool isDescending = string.Equals(_queryState.Sort.Direction, SortDescending, StringComparison.Ordinal);
        IOrderedEnumerable<GameModel> orderedGames = (_queryState.Sort.Field ?? SortFieldName) switch
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

    private void RefreshSortOptions()
    {
        _isUpdatingSelectionOptions = true;

        try
        {
            string currentSortField = SelectedSortField ?? SortFieldName;
            string currentSortDirection = SelectedSortDirection ?? SortAscending;
            ManagementCollectionFlow.ReplaceOptions(
                SortFieldOptions,
                [
                    new LocalizationOptionModel { Value = SortFieldName, DisplayName = LocalizationService.GetString("GameManagementPageSortFieldNameOption") },
                    new LocalizationOptionModel { Value = SortFieldPrice, DisplayName = LocalizationService.GetString("GameManagementPageSortFieldPriceOption") },
                    new LocalizationOptionModel { Value = SortFieldDifficulty, DisplayName = LocalizationService.GetString("GameManagementPageSortFieldDifficultyOption") },
                    new LocalizationOptionModel { Value = SortFieldStock, DisplayName = LocalizationService.GetString("GameManagementPageSortFieldStockOption") },
                    new LocalizationOptionModel { Value = SortFieldPlayers, DisplayName = LocalizationService.GetString("GameManagementPageSortFieldPlayersOption") },
                ]);
            ManagementCollectionFlow.ReplaceOptions(
                SortDirectionOptions,
                [
                    new LocalizationOptionModel { Value = SortAscending, DisplayName = LocalizationService.GetString("GameManagementPageSortDirectionAscendingOption") },
                    new LocalizationOptionModel { Value = SortDescending, DisplayName = LocalizationService.GetString("GameManagementPageSortDirectionDescendingOption") },
                ]);

            SelectedSortField = SortFieldOptions.Any(option => option.Value == currentSortField) ? currentSortField : SortFieldName;
            SelectedSortDirection = SortDirectionOptions.Any(option => option.Value == currentSortDirection) ? currentSortDirection : SortAscending;
            SyncSortState();
        }
        finally
        {
            _isUpdatingSelectionOptions = false;
        }
    }

    private void UpdatePageMetadata()
    {
        PageInfoSnapshot pageInfo = _games.BuildPageInfo();
        PageInfoText = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("GameManagementPagePageInfoFormat"),
            pageInfo.StartItem,
            pageInfo.EndItem,
            pageInfo.TotalItems,
            pageInfo.CurrentPage,
            pageInfo.TotalPages);
    }

    private void ApplyViewToggleState()
    {
        GridIconState = new IconState { Kind = IconKind.Grid, Size = 24, AlwaysFilled = IsGridView };
        ListIconState = new IconState { Kind = IconKind.List, Size = 24, AlwaysFilled = !IsGridView };
        GridButtonBackgroundBrush = IsGridView ? _selectedBackgroundBrush : _unselectedBackgroundBrush;
        GridButtonForegroundBrush = IsGridView ? _selectedForegroundBrush : _unselectedForegroundBrush;
        ListButtonBackgroundBrush = IsGridView ? _unselectedBackgroundBrush : _selectedBackgroundBrush;
        ListButtonForegroundBrush = IsGridView ? _unselectedForegroundBrush : _selectedForegroundBrush;
        GridButtonBackgroundHoverColor = IsGridView ? AppColors.OrangeFocus : AppColors.VeryLightGray;
        GridButtonForegroundHoverColor = IsGridView ? AppColors.White : AppColors.Black;
        ListButtonBackgroundHoverColor = IsGridView ? AppColors.VeryLightGray : AppColors.OrangeFocus;
        ListButtonForegroundHoverColor = IsGridView ? AppColors.Black : AppColors.White;
    }

    private object CreateActiveCard(GameModel game)
    {
        return IsGridView
            ? _cardFactory.CreateGrid(game, HandleEditGameAsync, HandleDeleteGameAsync, HandleIncreaseStock, HandleDecreaseStock)
            : _cardFactory.CreateList(game, HandleEditGameAsync, HandleDeleteGameAsync, HandleIncreaseStock, HandleDecreaseStock);
    }

    private void ClearPagedGameCards()
    {
        PagedGridGameCards.Clear();
        PagedListGameCards.Clear();
    }

    private void AddPagedGameCard(object card)
    {
        if (card is GridGameCardControlViewModel gridCard)
        {
            PagedGridGameCards.Add(gridCard);
        }
        else if (card is ListGameCardControlViewModel listCard)
        {
            PagedListGameCards.Add(listCard);
        }
    }

    private void HandleGamesRefreshed()
    {
        OnPropertyChanged(nameof(HasGames));
        UpdatePageMetadata();
    }

    private void SyncSortState()
    {
        _queryState.Sort = new ManagementSortState(SelectedSortField, SelectedSortDirection);
    }

    private static bool IsSameGameType(GameType? left, GameType? right)
    {
        return left is not null
            && right is not null
            && string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
    }
}
