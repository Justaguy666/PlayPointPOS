using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Application.Products;
using Application.Services;
using Application.Services.Products;
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
using WinUI.ViewModels.UserControls.Products;

namespace WinUI.ViewModels.Pages;

public partial class ProductManagementPageViewModel : LocalizedViewModelBase
{
    private const int GridPageSize = 9;
    private const int ListPageSize = 6;
    private const string SortFieldName = "name";
    private const string SortFieldType = "type";
    private const string SortFieldPrice = "price";
    private const string SortAscending = "asc";
    private const string SortDescending = "desc";
    private const double ProductCardOuterWidth = 300;
    private const double ProductCardOuterHeight = 340;
    private const double ProductCardsColumnSpacing = 16;
    private const int PreferredGridProductsPerRow = 5;
    private const int GridRowsPerPage = 3;
    private const int CompactGridPageSize = 10;

    private readonly ProductManagementDialogCoordinator _dialogs;
    private readonly IProductFilterService _productFilterService;
    private readonly IResponsiveLayoutService _responsiveLayoutService;
    private readonly ProductDraftFactory _draftFactory;
    private readonly ProductCardControlViewModelFactory _cardFactory;
    private readonly ManagementQueryState<ProductFilter, ManagementSortState> _queryState;
    private readonly ManagementCollectionController<ProductModel, object> _products;
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
    public partial int GridProductsMaximumRowsOrColumns { get; set; } = PreferredGridProductsPerRow;

    [ObservableProperty]
    public partial double GridProductsMinItemWidth { get; set; } = ProductCardOuterWidth;

    [ObservableProperty]
    public partial double GridProductsMinItemHeight { get; set; } = ProductCardOuterHeight;

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SearchPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SortFieldPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SortDirectionPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AddProductButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FilterButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PageInfoText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NoProductsText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? SelectedSortField { get; set; } = SortFieldName;

    [ObservableProperty]
    public partial string? SelectedSortDirection { get; set; } = SortAscending;

    [ObservableProperty]
    public partial IconState GridIconState { get; set; }

    [ObservableProperty]
    public partial IconState ListIconState { get; set; }

    [ObservableProperty]
    public partial IconState FilterIconState { get; set; } = new() { Kind = IconKind.Filter, Size = 20 };

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

    public IconState NoProductsIconState { get; } = new() { Kind = IconKind.Product, Size = 52, AlwaysFilled = true };

    public Color GridButtonBackgroundHoverColor { get; set; } = AppColors.OrangeFocus;

    public Color GridButtonForegroundHoverColor { get; set; } = AppColors.White;

    public Color ListButtonBackgroundHoverColor { get; set; } = AppColors.VeryLightGray;

    public Color ListButtonForegroundHoverColor { get; set; } = AppColors.Black;

    public ObservableCollection<GridProductCardControlViewModel> PagedGridProductCards { get; } = [];

    public ObservableCollection<ListProductCardControlViewModel> PagedListProductCards { get; } = [];

    public ObservableCollection<LocalizationOptionModel> SortFieldOptions { get; } = [];

    public ObservableCollection<LocalizationOptionModel> SortDirectionOptions { get; } = [];

    public PaginationControlViewModel PaginationViewModel { get; }

    public PaginationModel Pagination { get; }

    public IconKind SearchIconKind => IconKind.Search;

    public bool HasProducts => _products.HasItems;

    public IRelayCommand ToggleGridViewCommand { get; }

    public IRelayCommand ToggleListViewCommand { get; }

    public IAsyncRelayCommand FilterCommand { get; }

    public IAsyncRelayCommand AddProductCommand { get; }

    public IRelayCommand ClearSearchCommand { get; }

    public ProductManagementPageViewModel(
        ILocalizationService localizationService,
        ProductManagementDialogCoordinator dialogs,
        IProductCatalogService productCatalogService,
        IProductFilterService productFilterService,
        IResponsiveLayoutService responsiveLayoutService,
        ProductModelFactory productModelFactory,
        ProductDraftFactory draftFactory,
        ProductCardControlViewModelFactory cardFactory,
        PaginationControlViewModel paginationViewModel)
        : base(localizationService)
    {
        _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
        _productFilterService = productFilterService ?? throw new ArgumentNullException(nameof(productFilterService));
        _responsiveLayoutService = responsiveLayoutService ?? throw new ArgumentNullException(nameof(responsiveLayoutService));
        _draftFactory = draftFactory ?? throw new ArgumentNullException(nameof(draftFactory));
        _cardFactory = cardFactory ?? throw new ArgumentNullException(nameof(cardFactory));
        PaginationViewModel = paginationViewModel ?? throw new ArgumentNullException(nameof(paginationViewModel));
        ArgumentNullException.ThrowIfNull(productCatalogService);
        ArgumentNullException.ThrowIfNull(productModelFactory);

        _selectedBackgroundBrush = AppResourceLookup.GetBrush("OrangeFocusBrush", AppColors.OrangeFocus);
        _selectedForegroundBrush = AppResourceLookup.GetBrush("WhiteBrush", AppColors.White);
        _unselectedBackgroundBrush = AppResourceLookup.GetBrush("WhiteBrush", AppColors.White);
        _unselectedForegroundBrush = AppResourceLookup.GetBrush("BlackBrush", AppColors.Black);

        Pagination = new PaginationModel { CurrentPage = 1, PageSize = GridPageSize, MaxVisiblePageButtons = 4 };
        PaginationViewModel.Pagination = Pagination;
        _queryState = new ManagementQueryState<ProductFilter, ManagementSortState>(
            new ProductFilter(),
            new ManagementSortState(SortFieldName, SortAscending));

        List<ProductModel> products = productCatalogService.GetProducts()
            .Select(productModelFactory.Create)
            .ToList();
        _products = new ManagementCollectionController<ProductModel, object>(
            products,
            Pagination,
            ClearPagedProductCards,
            AddPagedProductCard,
            QueryProducts,
            CreateActiveCard,
            () => IsGridView ? "grid" : "list");
        _products.Refreshed += HandleProductsRefreshed;

        GridIconState = new IconState { Kind = IconKind.Grid, Size = 24, AlwaysFilled = true };
        ListIconState = new IconState { Kind = IconKind.List, Size = 24 };
        GridButtonBackgroundBrush = _selectedBackgroundBrush;
        GridButtonForegroundBrush = _selectedForegroundBrush;
        ListButtonBackgroundBrush = _unselectedBackgroundBrush;
        ListButtonForegroundBrush = _unselectedForegroundBrush;

        ToggleGridViewCommand = new RelayCommand(ExecuteToggleGridView);
        ToggleListViewCommand = new RelayCommand(ExecuteToggleListView);
        FilterCommand = new AsyncRelayCommand(OpenFilterDialogAsync);
        AddProductCommand = new AsyncRelayCommand(ExecuteAddProductAsync);
        ClearSearchCommand = new RelayCommand(ClearSearch);

        RefreshLocalizedText();
        _isInitialized = true;
        ApplyFiltersAndSorting(resetToFirstPage: true);
    }

    protected override void RefreshLocalizedText()
    {
        AddProductButtonText = LocalizationService.GetString("ProductManagementPageAddProductButton");
        FilterButtonText = LocalizationService.GetString("ProductManagementPageFilterButton");
        SearchPlaceholderText = LocalizationService.GetString("ProductManagementPageSearchPlaceholderText");
        SortFieldPlaceholderText = LocalizationService.GetString("ProductManagementPageSortFieldComboBox.PlaceholderText");
        SortDirectionPlaceholderText = LocalizationService.GetString("ProductManagementPageSortDirectionComboBox.PlaceholderText");
        NoProductsText = LocalizationService.GetString("ProductManagementPageNoProductsText");
        RefreshSortOptions();
        UpdatePageMetadata();
    }

    public new void Dispose()
    {
        if (_isDisposed)
            return;

        _products.Refreshed -= HandleProductsRefreshed;
        _products.Dispose();
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

    public void UpdateGridProductsLayout(double availableWidth)
    {
        if (availableWidth <= 0)
            return;

        CardGridLayout layout = _responsiveLayoutService.CalculateCardGrid(
            availableWidth,
            ProductCardOuterWidth,
            ProductCardOuterHeight,
            ProductCardsColumnSpacing,
            PreferredGridProductsPerRow,
            GridRowsPerPage,
            CompactGridPageSize);

        GridProductsMaximumRowsOrColumns = layout.Columns;
        GridProductsMinItemWidth = layout.ItemWidth;
        GridProductsMinItemHeight = layout.ItemHeight;

        if (IsGridView)
        {
            _products.SetPageSize(layout.PageSize);
        }
    }

    private void HandleSortChanged()
    {
        if (!_isInitialized || _isUpdatingSelectionOptions)
            return;

        SyncSortState();
        ApplyFiltersAndSorting(resetToFirstPage: true);
    }

    private void ExecuteToggleGridView()
    {
        if (IsGridView)
            return;

        IsGridView = true;
        ApplyViewToggleState();
        _products.SetPageSize(GridProductsMaximumRowsOrColumns >= 3
            ? GridProductsMaximumRowsOrColumns * GridRowsPerPage
            : CompactGridPageSize);
        _products.RefreshPage();
    }

    private void ExecuteToggleListView()
    {
        if (!IsGridView)
            return;

        IsGridView = false;
        ApplyViewToggleState();
        _products.SetPageSize(ListPageSize);
        _products.RefreshPage();
    }

    private Task OpenFilterDialogAsync()
    {
        return _dialogs.OpenFilterAsync(_queryState.Filter, HandleFilterSubmittedAsync);
    }

    private Task ExecuteAddProductAsync()
    {
        return _dialogs.OpenAddAsync(_draftFactory.Create(), HandleProductCreatedAsync);
    }

    private void ClearSearch()
    {
        SearchText = string.Empty;
    }

    private Task HandleEditProductAsync(ProductModel product)
    {
        return product is null
            ? Task.CompletedTask
            : _dialogs.OpenEditAsync(product, HandleProductUpdatedAsync);
    }

    private async Task HandleDeleteProductAsync(ProductModel product)
    {
        if (product is null || !await _dialogs.ConfirmDeleteAsync())
            return;

        if (!_products.Remove(product))
            return;

        ApplyFiltersAndSorting(resetToFirstPage: false);
        await _dialogs.NotifyDeletedAsync(product);
    }

    private Task HandleFilterSubmittedAsync(ProductFilter criteria)
    {
        _queryState.Filter = criteria;
        ApplyFiltersAndSorting(resetToFirstPage: true);
        return Task.CompletedTask;
    }

    private async Task HandleProductCreatedAsync(ProductModel product)
    {
        if (!_products.Contains(product))
        {
            _products.Insert(0, product);
        }

        ApplyFiltersAndSorting(resetToFirstPage: true);
        await _dialogs.NotifyCreatedAsync(product);
    }

    private async Task HandleProductUpdatedAsync(ProductModel product)
    {
        ApplyFiltersAndSorting(resetToFirstPage: false);
        await _dialogs.NotifyUpdatedAsync(product);
    }

    private void ApplyFiltersAndSorting(bool resetToFirstPage)
    {
        _queryState.SearchText = SearchText;
        SyncSortState();
        _products.Refresh(resetToFirstPage);
    }

    private IReadOnlyList<ProductModel> QueryProducts(IEnumerable<ProductModel> source)
    {
        IEnumerable<ProductModel> products = _productFilterService.Apply(source, _queryState.Filter);

        if (!string.IsNullOrWhiteSpace(_queryState.SearchText))
        {
            products = products.Where(MatchesSearch);
        }

        return SortProducts(products);
    }

    private bool MatchesSearch(ProductModel product)
    {
        string productTypeDisplayName = GetProductTypeDisplayName(product.ProductType);
        return ManagementCollectionFlow.ContainsSearchText(LocalizationService.Culture, product.Name, _queryState.SearchText)
            || ManagementCollectionFlow.ContainsSearchText(LocalizationService.Culture, productTypeDisplayName, _queryState.SearchText);
    }

    private IReadOnlyList<ProductModel> SortProducts(IEnumerable<ProductModel> products)
    {
        bool isDescending = string.Equals(_queryState.Sort.Direction, SortDescending, StringComparison.Ordinal);
        IOrderedEnumerable<ProductModel> orderedProducts = (_queryState.Sort.Field ?? SortFieldName) switch
        {
            SortFieldType => isDescending
                ? products.OrderByDescending(product => product.ProductType).ThenBy(product => product.Name, StringComparer.CurrentCultureIgnoreCase)
                : products.OrderBy(product => product.ProductType).ThenBy(product => product.Name, StringComparer.CurrentCultureIgnoreCase),
            SortFieldPrice => isDescending
                ? products.OrderByDescending(product => product.Price).ThenBy(product => product.Name, StringComparer.CurrentCultureIgnoreCase)
                : products.OrderBy(product => product.Price).ThenBy(product => product.Name, StringComparer.CurrentCultureIgnoreCase),
            _ => isDescending
                ? products.OrderByDescending(product => product.Name, StringComparer.CurrentCultureIgnoreCase)
                : products.OrderBy(product => product.Name, StringComparer.CurrentCultureIgnoreCase),
        };

        return orderedProducts.ToList();
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
                    new LocalizationOptionModel { Value = SortFieldName, DisplayName = LocalizationService.GetString("ProductManagementPageSortFieldNameOption") },
                    new LocalizationOptionModel { Value = SortFieldType, DisplayName = LocalizationService.GetString("ProductManagementPageSortFieldTypeOption") },
                    new LocalizationOptionModel { Value = SortFieldPrice, DisplayName = LocalizationService.GetString("ProductManagementPageSortFieldPriceOption") },
                ]);

            ManagementCollectionFlow.ReplaceOptions(
                SortDirectionOptions,
                [
                    new LocalizationOptionModel { Value = SortAscending, DisplayName = LocalizationService.GetString("ProductManagementPageSortDirectionAscendingOption") },
                    new LocalizationOptionModel { Value = SortDescending, DisplayName = LocalizationService.GetString("ProductManagementPageSortDirectionDescendingOption") },
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
        PageInfoSnapshot pageInfo = _products.BuildPageInfo();
        PageInfoText = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("ProductManagementPagePageInfoFormat"),
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

    private object CreateActiveCard(ProductModel product)
    {
        return IsGridView
            ? _cardFactory.CreateGrid(product, HandleEditProductAsync, HandleDeleteProductAsync)
            : _cardFactory.CreateList(product, HandleEditProductAsync, HandleDeleteProductAsync);
    }

    private void ClearPagedProductCards()
    {
        PagedGridProductCards.Clear();
        PagedListProductCards.Clear();
    }

    private void AddPagedProductCard(object card)
    {
        if (card is GridProductCardControlViewModel gridCard)
        {
            PagedGridProductCards.Add(gridCard);
        }
        else if (card is ListProductCardControlViewModel listCard)
        {
            PagedListProductCards.Add(listCard);
        }
    }

    private void HandleProductsRefreshed()
    {
        OnPropertyChanged(nameof(HasProducts));
        UpdatePageMetadata();
    }

    private void SyncSortState()
    {
        _queryState.Sort = new ManagementSortState(SelectedSortField, SelectedSortDirection);
    }

    private string GetProductTypeDisplayName(ProductType productType)
    {
        return productType switch
        {
            ProductType.Drink => LocalizationService.GetString("ProductTypeDrinkText"),
            _ => LocalizationService.GetString("ProductTypeFoodText"),
        };
    }
}
