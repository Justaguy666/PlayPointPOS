using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Application.Services;
using Application.Services.Transactions;
using Application.Transactions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Enums;
using WinUI.Services.Factories;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.UIModels.Management;
using WinUI.ViewModels.Dialogs.Management;
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
    private const double LayoutPrecisionEpsilon = 0.5;

    private readonly IDialogService _dialogService;
    private readonly INotificationService _notificationService;
    private readonly ITransactionFilterService _transactionFilterService;
    private readonly TransactionCardControlViewModelFactory _cardControlViewModelFactory;
    private readonly List<TransactionModel> _allTransactions;
    private readonly Dictionary<TransactionModel, TransactionCardControlViewModel> _cardViewModelsByTransaction;
    private readonly PaginationModel _pagination;
    private IReadOnlyList<TransactionModel> _filteredTransactions = [];
    private PaymentMethod? _activePaymentMethodFilter;
    private decimal? _activeAmountMinFilter;
    private decimal? _activeAmountMaxFilter;
    private DateTime? _activeDateFromFilter;
    private DateTime? _activeDateToFilter;
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

    private int _transactionCardsMaximumRowsOrColumns = PreferredGridTransactionsPerRow;
    public int TransactionCardsMaximumRowsOrColumns
    {
        get => _transactionCardsMaximumRowsOrColumns;
        set => SetProperty(ref _transactionCardsMaximumRowsOrColumns, value);
    }

    private double _transactionCardsMinItemWidth = TransactionCardOuterWidth;
    public double TransactionCardsMinItemWidth
    {
        get => _transactionCardsMinItemWidth;
        set => SetProperty(ref _transactionCardsMinItemWidth, value);
    }

    private double _transactionCardsMinItemHeight = TransactionCardOuterHeight;
    public double TransactionCardsMinItemHeight
    {
        get => _transactionCardsMinItemHeight;
        set => SetProperty(ref _transactionCardsMinItemHeight, value);
    }

    public IconState NoTransactionsIconState { get; } = new()
    {
        Kind = IconKind.Game,
        Size = 52,
        AlwaysFilled = true,
    };

    public IconKind SearchIconKind => IconKind.Search;

    public ObservableCollection<TransactionCardControlViewModel> PagedTransactionCards { get; } = [];

    public PaginationControlViewModel PaginationViewModel { get; }

    public PaginationModel Pagination => _pagination;

    public bool HasTransactions => _filteredTransactions.Count > 0;

    public IAsyncRelayCommand FilterCommand { get; }

    public IRelayCommand ClearSearchCommand { get; }

    public TransactionManagementPageViewModel(
        ILocalizationService localizationService,
        IDialogService dialogService,
        INotificationService notificationService,
        ITransactionCatalogService transactionCatalogService,
        ITransactionFilterService transactionFilterService,
        TransactionModelFactory transactionModelFactory,
        TransactionCardControlViewModelFactory cardControlViewModelFactory,
        PaginationControlViewModel paginationViewModel)
        : base(localizationService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _transactionFilterService = transactionFilterService ?? throw new ArgumentNullException(nameof(transactionFilterService));
        _cardControlViewModelFactory = cardControlViewModelFactory ?? throw new ArgumentNullException(nameof(cardControlViewModelFactory));
        PaginationViewModel = paginationViewModel ?? throw new ArgumentNullException(nameof(paginationViewModel));
        ArgumentNullException.ThrowIfNull(transactionCatalogService);
        ArgumentNullException.ThrowIfNull(transactionModelFactory);

        _pagination = new PaginationModel
        {
            CurrentPage = 1,
            PageSize = PageSize,
            MaxVisiblePageButtons = 4,
        };

        PaginationViewModel.Pagination = _pagination;
        _pagination.PropertyChanged += HandlePaginationPropertyChanged;

        FilterCommand = new AsyncRelayCommand(OpenFilterDialogAsync);
        ClearSearchCommand = new RelayCommand(ClearSearch);

        _allTransactions = transactionCatalogService
            .GetTransactions()
            .Select(transactionModelFactory.Create)
            .ToList();
        _cardViewModelsByTransaction = _allTransactions.ToDictionary(
            transaction => transaction,
            CreateCardViewModel);

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

        _pagination.PropertyChanged -= HandlePaginationPropertyChanged;
        foreach (TransactionCardControlViewModel viewModel in _cardViewModelsByTransaction.Values)
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

    public void UpdateTransactionCardsLayout(double availableWidth)
    {
        if (availableWidth <= 0)
        {
            return;
        }

        int columns = CalculateTransactionCardsColumnCount(availableWidth);
        double totalSpacing = TransactionCardsColumnSpacing * (columns - 1);

        TransactionCardsMaximumRowsOrColumns = columns;
        TransactionCardsMinItemWidth = Math.Floor((availableWidth - totalSpacing + LayoutPrecisionEpsilon) / columns);
        TransactionCardsMinItemHeight = TransactionCardOuterHeight;
    }

    private void HandlePaginationPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(PaginationModel.CurrentPage) or
            nameof(PaginationModel.PageSize) or
            nameof(PaginationModel.TotalItems))
        {
            RefreshPagedTransactions();
        }
    }

    private async Task OpenFilterDialogAsync()
    {
        await _dialogService.ShowDialogAsync(
            "TransactionFilter",
            new TransactionFilterDialogRequest
            {
                InitialCriteria = BuildCurrentFilterCriteria(),
                OnSubmittedAsync = HandleFilterSubmittedAsync,
            });
    }

    private void ClearSearch()
    {
        SearchText = string.Empty;
    }

    private Task HandleFilterSubmittedAsync(TransactionFilter criteria)
    {
        _activePaymentMethodFilter = criteria.PaymentMethod;
        _activeAmountMinFilter = criteria.AmountMin;
        _activeAmountMaxFilter = criteria.AmountMax;
        _activeDateFromFilter = criteria.DateFrom;
        _activeDateToFilter = criteria.DateTo;

        ApplyFiltersAndSorting(resetToFirstPage: true);
        return Task.CompletedTask;
    }

    private Task HandleShowDetailAsync(TransactionModel transaction)
    {
        return _dialogService.ShowDialogAsync(
            "TransactionDetail",
            new TransactionDetailDialogRequest { Model = transaction });
    }

    private Task HandleTogglePaymentMethodAsync(TransactionModel transaction)
    {
        transaction.PaymentMethod = transaction.PaymentMethod == PaymentMethod.Cash
            ? PaymentMethod.Banking
            : PaymentMethod.Cash;
        return Task.CompletedTask;
    }

    private void ApplyFiltersAndSorting(bool resetToFirstPage)
    {
        IReadOnlyList<TransactionModel> filtered = ApplyDomainFilter();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered
                .Where(MatchesSearch)
                .ToList();
        }

        _filteredTransactions = filtered
            .OrderByDescending(t => t.CreatedAt)
            .ToList();

        _pagination.TotalItems = _filteredTransactions.Count;

        if (resetToFirstPage)
        {
            if (_pagination.CurrentPage != 1)
            {
                _pagination.CurrentPage = 1;
                return;
            }

            RefreshPagedTransactions();
            return;
        }

        RefreshPagedTransactions();
    }

    private IReadOnlyList<TransactionModel> ApplyDomainFilter()
    {
        TransactionFilter criteria = BuildCurrentFilterCriteria();
        IEnumerable<TransactionModel> result = _allTransactions;

        if (criteria.PaymentMethod.HasValue)
        {
            result = result.Where(t => t.PaymentMethod == criteria.PaymentMethod.Value);
        }

        if (criteria.AmountMin.HasValue)
        {
            result = result.Where(t => t.TotalAmount >= criteria.AmountMin.Value);
        }

        if (criteria.AmountMax.HasValue)
        {
            result = result.Where(t => t.TotalAmount <= criteria.AmountMax.Value);
        }

        if (criteria.DateFrom.HasValue)
        {
            result = result.Where(t => t.CreatedAt >= criteria.DateFrom.Value);
        }

        if (criteria.DateTo.HasValue)
        {
            result = result.Where(t => t.CreatedAt <= criteria.DateTo.Value);
        }

        return result.ToList();
    }

    private TransactionFilter BuildCurrentFilterCriteria()
    {
        return new TransactionFilter
        {
            PaymentMethod = _activePaymentMethodFilter,
            AmountMin = _activeAmountMinFilter,
            AmountMax = _activeAmountMaxFilter,
            DateFrom = _activeDateFromFilter,
            DateTo = _activeDateToFilter,
        };
    }

    private bool MatchesSearch(TransactionModel transaction)
    {
        return LocalizationService.Culture.CompareInfo.IndexOf(transaction.Code, SearchText, CompareOptions.IgnoreCase) >= 0
            || LocalizationService.Culture.CompareInfo.IndexOf(transaction.CustomerName, SearchText, CompareOptions.IgnoreCase) >= 0
            || LocalizationService.Culture.CompareInfo.IndexOf(transaction.MemberId ?? "", SearchText, CompareOptions.IgnoreCase) >= 0;
    }

    private void RefreshPagedTransactions()
    {
        PagedTransactionCards.Clear();

        int startIndex = Math.Max(0, (_pagination.CurrentPage - 1) * _pagination.PageSize);
        foreach (TransactionModel transaction in _filteredTransactions.Skip(startIndex).Take(_pagination.PageSize))
        {
            if (_cardViewModelsByTransaction.TryGetValue(transaction, out TransactionCardControlViewModel? cardViewModel))
            {
                PagedTransactionCards.Add(cardViewModel);
            }
        }

        OnPropertyChanged(nameof(HasTransactions));
        UpdatePageMetadata();
    }

    private void UpdatePageMetadata()
    {
        int totalItems = _filteredTransactions.Count;
        int startItem = totalItems == 0 ? 0 : ((_pagination.CurrentPage - 1) * _pagination.PageSize) + 1;
        int endItem = totalItems == 0 ? 0 : Math.Min(_pagination.CurrentPage * _pagination.PageSize, totalItems);
        int totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)Math.Max(_pagination.PageSize, 1)));

        PageInfoText = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("TransactionManagementPagePageInfoFormat"),
            startItem,
            endItem,
            totalItems,
            _pagination.CurrentPage,
            totalPages);
    }

    private TransactionCardControlViewModel CreateCardViewModel(TransactionModel transaction)
    {
        return _cardControlViewModelFactory.Create(
            transaction,
            HandleShowDetailAsync,
            HandleTogglePaymentMethodAsync);
    }

    private static int CalculateTransactionCardsColumnCount(double availableWidth)
    {
        if (availableWidth < TransactionCardOuterWidth)
        {
            return 1;
        }

        int maxPossibleColumns = (int)((availableWidth + TransactionCardsColumnSpacing) / (TransactionCardOuterWidth + TransactionCardsColumnSpacing));
        return Math.Max(1, Math.Min(PreferredGridTransactionsPerRow, maxPossibleColumns));
    }
}
