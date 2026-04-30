using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Application.Services;
using Application.Services.Transactions;
using Application.Transactions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Enums;
using WinUI.Services.Factories;
using WinUI.Services.Layout;
using WinUI.Services.Management;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.UIModels.Management;
using WinUI.ViewModels.Common;
using WinUI.ViewModels.UserControls;
using WinUI.ViewModels.UserControls.Transactions;

namespace WinUI.ViewModels.Pages;

public partial class TransactionManagementPageViewModel : LocalizedViewModelBase
{
    private const int PageSize = 10;
    private const double TransactionCardOuterWidth = 320;
    private const double TransactionCardOuterHeight = 136;
    private const double TransactionCardsColumnSpacing = 14;
    private const int PreferredGridTransactionsPerRow = 2;

    private readonly TransactionManagementDialogCoordinator _dialogs;
    private readonly IResponsiveLayoutService _responsiveLayoutService;
    private readonly TransactionCardControlViewModelFactory _cardFactory;
    private readonly ManagementQueryState<TransactionFilter, ManagementSortState> _queryState;
    private readonly ManagementCollectionController<TransactionModel, TransactionCardControlViewModel> _transactions;
    private bool _isInitialized;
    private bool _isDisposed;

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SearchPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FilterButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PageInfoText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NoTransactionsText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IconState FilterIconState { get; set; } = new() { Kind = IconKind.Filter, Size = 20, AlwaysFilled = false };

    [ObservableProperty]
    public partial int TransactionCardsMaximumRowsOrColumns { get; set; } = PreferredGridTransactionsPerRow;

    [ObservableProperty]
    public partial double TransactionCardsMinItemWidth { get; set; } = TransactionCardOuterWidth;

    [ObservableProperty]
    public partial double TransactionCardsMinItemHeight { get; set; } = TransactionCardOuterHeight;

    public IconState NoTransactionsIconState { get; } = new()
    {
        Kind = IconKind.Game,
        Size = 52,
        AlwaysFilled = true,
    };

    public IconKind SearchIconKind => IconKind.Search;

    public ObservableCollection<TransactionCardControlViewModel> PagedTransactionCards { get; } = [];

    public PaginationControlViewModel PaginationViewModel { get; }

    public PaginationModel Pagination { get; }

    public bool HasTransactions => _transactions.HasItems;

    public IAsyncRelayCommand FilterCommand { get; }

    public IRelayCommand ClearSearchCommand { get; }

    public TransactionManagementPageViewModel(
        ILocalizationService localizationService,
        TransactionManagementDialogCoordinator dialogs,
        ITransactionCatalogService transactionCatalogService,
        IResponsiveLayoutService responsiveLayoutService,
        TransactionModelFactory transactionModelFactory,
        TransactionCardControlViewModelFactory cardFactory,
        PaginationControlViewModel paginationViewModel)
        : base(localizationService)
    {
        _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
        _responsiveLayoutService = responsiveLayoutService ?? throw new ArgumentNullException(nameof(responsiveLayoutService));
        _cardFactory = cardFactory ?? throw new ArgumentNullException(nameof(cardFactory));
        PaginationViewModel = paginationViewModel ?? throw new ArgumentNullException(nameof(paginationViewModel));
        ArgumentNullException.ThrowIfNull(transactionCatalogService);
        ArgumentNullException.ThrowIfNull(transactionModelFactory);

        Pagination = new PaginationModel { CurrentPage = 1, PageSize = PageSize, MaxVisiblePageButtons = 4 };
        PaginationViewModel.Pagination = Pagination;
        _queryState = new ManagementQueryState<TransactionFilter, ManagementSortState>(
            new TransactionFilter(),
            new ManagementSortState(nameof(TransactionModel.CreatedAt), "desc"));

        List<TransactionModel> transactions = transactionCatalogService
            .GetTransactions()
            .Select(transactionModelFactory.Create)
            .ToList();
        _transactions = new ManagementCollectionController<TransactionModel, TransactionCardControlViewModel>(
            transactions,
            Pagination,
            PagedTransactionCards,
            QueryTransactions,
            CreateCardViewModel);
        _transactions.Refreshed += HandleTransactionsRefreshed;

        FilterCommand = new AsyncRelayCommand(OpenFilterDialogAsync);
        ClearSearchCommand = new RelayCommand(ClearSearch);

        RefreshLocalizedText();
        _isInitialized = true;
        ApplyFiltersAndSorting(resetToFirstPage: true);
    }

    protected override void RefreshLocalizedText()
    {
        SearchPlaceholderText = LocalizationService.GetString("TransactionManagementPageSearchPlaceholderText");
        FilterButtonText = LocalizationService.GetString("TransactionManagementPageFilterButton");
        NoTransactionsText = LocalizationService.GetString("TransactionManagementPageNoTransactionsText");
        UpdatePageMetadata();
    }

    public new void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _transactions.Refreshed -= HandleTransactionsRefreshed;
        _transactions.Dispose();
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

        _queryState.SearchText = value;
        ApplyFiltersAndSorting(resetToFirstPage: true);
    }

    public void UpdateTransactionCardsLayout(double availableWidth)
    {
        if (availableWidth <= 0)
        {
            return;
        }

        CardGridLayout layout = _responsiveLayoutService.CalculateCardGrid(
            availableWidth,
            TransactionCardOuterWidth,
            TransactionCardOuterHeight,
            TransactionCardsColumnSpacing,
            PreferredGridTransactionsPerRow,
            PageSize,
            PageSize);

        TransactionCardsMaximumRowsOrColumns = layout.Columns;
        TransactionCardsMinItemWidth = layout.ItemWidth;
        TransactionCardsMinItemHeight = layout.ItemHeight;
        _transactions.SetPageSize(layout.PageSize);
    }

    private Task OpenFilterDialogAsync()
    {
        return _dialogs.OpenFilterAsync(_queryState.Filter, HandleFilterSubmittedAsync);
    }

    private void ClearSearch()
    {
        SearchText = string.Empty;
    }

    private Task HandleFilterSubmittedAsync(TransactionFilter criteria)
    {
        _queryState.Filter = criteria;
        ApplyFiltersAndSorting(resetToFirstPage: true);
        return Task.CompletedTask;
    }

    private Task HandleShowDetailAsync(TransactionModel transaction)
    {
        return _dialogs.OpenDetailAsync(transaction);
    }

    private static Task HandleTogglePaymentMethodAsync(TransactionModel transaction)
    {
        transaction.PaymentMethod = transaction.PaymentMethod == PaymentMethod.Cash
            ? PaymentMethod.Banking
            : PaymentMethod.Cash;
        return Task.CompletedTask;
    }

    private void ApplyFiltersAndSorting(bool resetToFirstPage)
    {
        _queryState.SearchText = SearchText;
        _transactions.Refresh(resetToFirstPage);
    }

    private IReadOnlyList<TransactionModel> QueryTransactions(IEnumerable<TransactionModel> source)
    {
        IEnumerable<TransactionModel> transactions = ApplyTransactionFilter(source, _queryState.Filter);

        if (!string.IsNullOrWhiteSpace(_queryState.SearchText))
        {
            transactions = transactions.Where(MatchesSearch);
        }

        return transactions
            .OrderByDescending(transaction => transaction.CreatedAt)
            .ToList();
    }

    private static IEnumerable<TransactionModel> ApplyTransactionFilter(
        IEnumerable<TransactionModel> source,
        TransactionFilter filter)
    {
        if (filter.PaymentMethod.HasValue)
        {
            source = source.Where(transaction => transaction.PaymentMethod == filter.PaymentMethod.Value);
        }

        if (filter.AmountMin.HasValue)
        {
            source = source.Where(transaction => transaction.TotalAmount >= filter.AmountMin.Value);
        }

        if (filter.AmountMax.HasValue)
        {
            source = source.Where(transaction => transaction.TotalAmount <= filter.AmountMax.Value);
        }

        if (filter.DateFrom.HasValue)
        {
            source = source.Where(transaction => transaction.CreatedAt >= filter.DateFrom.Value);
        }

        if (filter.DateTo.HasValue)
        {
            source = source.Where(transaction => transaction.CreatedAt <= filter.DateTo.Value);
        }

        return source;
    }

    private bool MatchesSearch(TransactionModel transaction)
    {
        return ManagementCollectionFlow.ContainsSearchText(LocalizationService.Culture, transaction.Code, _queryState.SearchText)
            || ManagementCollectionFlow.ContainsSearchText(LocalizationService.Culture, transaction.CustomerName, _queryState.SearchText)
            || ManagementCollectionFlow.ContainsSearchText(LocalizationService.Culture, transaction.MemberId, _queryState.SearchText);
    }

    private void UpdatePageMetadata()
    {
        PageInfoSnapshot pageInfo = _transactions.BuildPageInfo();
        PageInfoText = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("TransactionManagementPagePageInfoFormat"),
            pageInfo.StartItem,
            pageInfo.EndItem,
            pageInfo.TotalItems,
            pageInfo.CurrentPage,
            pageInfo.TotalPages);
    }

    private TransactionCardControlViewModel CreateCardViewModel(TransactionModel transaction)
    {
        return _cardFactory.Create(
            transaction,
            HandleShowDetailAsync,
            HandleTogglePaymentMethodAsync);
    }

    private void HandleTransactionsRefreshed()
    {
        OnPropertyChanged(nameof(HasTransactions));
        UpdatePageMetadata();
    }
}
