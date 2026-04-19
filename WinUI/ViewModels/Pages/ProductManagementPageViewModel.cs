using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.UIModels.Management;
using WinUI.ViewModels.Dialogs.Management;
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
    private const double LayoutPrecisionEpsilon = 0.5;

    private readonly IDialogService _dialogService;
    private readonly INotificationService _notificationService;
    private readonly IProductFilterService _productFilterService;
    private readonly ProductModelFactory _productModelFactory;
    private readonly ProductCardControlViewModelFactory _productCardControlViewModelFactory;
    private readonly Brush _selectedBackgroundBrush;
    private readonly Brush _selectedForegroundBrush;
    private readonly Brush _unselectedBackgroundBrush;
    private readonly Brush _unselectedForegroundBrush;
    private readonly List<ProductModel> _allProducts;
    private readonly Dictionary<ProductModel, GridProductCardControlViewModel> _gridCardViewModelsByProduct;
    private readonly Dictionary<ProductModel, ListProductCardControlViewModel> _listCardViewModelsByProduct;
    private readonly PaginationModel _pagination;
    private IReadOnlyList<ProductModel> _filteredProducts = [];
    private ProductType? _activeProductTypeFilter;
    private decimal? _activePriceMinFilter;
    private decimal? _activePriceMaxFilter;
    private bool _isUpdatingSelectionOptions;
    private bool _isInitialized;
    private bool _isDisposed;

    [ObservableProperty]
    public partial bool IsGridView { get; set; } = true;

    private int _gridProductsMaximumRowsOrColumns = 5;
    public int GridProductsMaximumRowsOrColumns
    {
        get => _gridProductsMaximumRowsOrColumns;
        set => SetProperty(ref _gridProductsMaximumRowsOrColumns, value);
    }

    private double _gridProductsMinItemWidth = 300;
    public double GridProductsMinItemWidth
    {
        get => _gridProductsMinItemWidth;
        set => SetProperty(ref _gridProductsMinItemWidth, value);
    }

    private double _gridProductsMinItemHeight = 400;
    public double GridProductsMinItemHeight
    {
        get => _gridProductsMinItemHeight;
        set => SetProperty(ref _gridProductsMinItemHeight, value);
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

    public IconState NoProductsIconState { get; } = new()
    {
        Kind = IconKind.Product,
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

    public ObservableCollection<GridProductCardControlViewModel> PagedGridProductCards { get; } = [];

    public ObservableCollection<ListProductCardControlViewModel> PagedListProductCards { get; } = [];

    public ObservableCollection<LocalizationOptionModel> SortFieldOptions { get; } = [];

    public ObservableCollection<LocalizationOptionModel> SortDirectionOptions { get; } = [];

    public PaginationControlViewModel PaginationViewModel { get; }

    public PaginationModel Pagination => _pagination;

    public IconKind SearchIconKind => IconKind.Search;

    public bool HasProducts => _filteredProducts.Count > 0;

    public IRelayCommand ToggleGridViewCommand { get; }

    public IRelayCommand ToggleListViewCommand { get; }

    public IAsyncRelayCommand FilterCommand { get; }

    public IAsyncRelayCommand AddProductCommand { get; }

    public IRelayCommand ClearSearchCommand { get; }

    public ProductManagementPageViewModel(
        ILocalizationService localizationService,
        IDialogService dialogService,
        INotificationService notificationService,
        IProductCatalogService productCatalogService,
        IProductFilterService productFilterService,
        ProductModelFactory productModelFactory,
        ProductCardControlViewModelFactory productCardControlViewModelFactory,
        PaginationControlViewModel paginationViewModel)
        : base(localizationService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _productFilterService = productFilterService ?? throw new ArgumentNullException(nameof(productFilterService));
        _productModelFactory = productModelFactory ?? throw new ArgumentNullException(nameof(productModelFactory));
        _productCardControlViewModelFactory = productCardControlViewModelFactory ?? throw new ArgumentNullException(nameof(productCardControlViewModelFactory));
        PaginationViewModel = paginationViewModel ?? throw new ArgumentNullException(nameof(paginationViewModel));
        ArgumentNullException.ThrowIfNull(productCatalogService);

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
        AddProductCommand = new AsyncRelayCommand(ExecuteAddProductAsync);
        ClearSearchCommand = new RelayCommand(ClearSearch);

        _allProducts = productCatalogService
            .GetProducts()
            .Select(_productModelFactory.Create)
            .ToList();
        _gridCardViewModelsByProduct = _allProducts.ToDictionary(
            product => product,
            CreateGridCardViewModel);
        _listCardViewModelsByProduct = _allProducts.ToDictionary(
            product => product,
            CreateListCardViewModel);

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
        {
            return;
        }

        _pagination.PropertyChanged -= HandlePaginationPropertyChanged;
        foreach (GridProductCardControlViewModel viewModel in _gridCardViewModelsByProduct.Values)
        {
            viewModel.Dispose();
        }

        foreach (ListProductCardControlViewModel viewModel in _listCardViewModelsByProduct.Values)
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

    public void UpdateGridProductsLayout(double availableWidth)
    {
        if (availableWidth <= 0)
        {
            return;
        }

        int columns = CalculateGridProductsColumnCount(availableWidth);
        double totalSpacing = ProductCardsColumnSpacing * (columns - 1);

        GridProductsMaximumRowsOrColumns = columns;
        GridProductsMinItemWidth = Math.Floor((availableWidth - totalSpacing + LayoutPrecisionEpsilon) / columns);
        GridProductsMinItemHeight = ProductCardOuterHeight;

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

        int totalPages = Math.Max(1, (int)Math.Ceiling(_filteredProducts.Count / (double)Math.Max(_pagination.PageSize, 1)));
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
            RefreshPagedProducts();
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

        int columns = GridProductsMaximumRowsOrColumns;
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
            "ProductFilter",
            new ProductFilterDialogRequest
            {
                InitialCriteria = BuildCurrentFilterCriteria(),
                OnSubmittedAsync = HandleFilterSubmittedAsync,
            });
    }

    private async Task ExecuteAddProductAsync()
    {
        await _dialogService.ShowDialogAsync(
            "Product",
            new ProductDialogRequest
            {
                Mode = UpsertDialogMode.Add,
                Model = CreateNewProductDraft(),
                OnSubmittedAsync = HandleProductCreatedAsync,
            });
    }

    private void ClearSearch()
    {
        SearchText = string.Empty;
    }

    private Task HandleEditProductAsync(ProductModel product)
    {
        if (product is null)
        {
            return Task.CompletedTask;
        }

        return _dialogService.ShowDialogAsync(
            "Product",
            new ProductDialogRequest
            {
                Mode = UpsertDialogMode.Edit,
                Model = product,
                OnSubmittedAsync = HandleProductUpdatedAsync,
            });
    }

    private async Task HandleDeleteProductAsync(ProductModel product)
    {
        if (product is null)
        {
            return;
        }

        bool isConfirmed = await _dialogService.ShowConfirmationAsync(
            titleKey: "ConfirmDeleteProductTitle",
            messageKey: "ConfirmDeleteProductMessage",
            confirmButtonTextKey: "ConfirmDeleteProductButton",
            cancelButtonTextKey: "CancelButtonText");

        if (!isConfirmed)
        {
            return;
        }

        if (!_allProducts.Remove(product))
        {
            return;
        }

        RemoveCardViewModelsForProduct(product);
        ApplyFiltersAndSorting(resetToFirstPage: false);

        await _notificationService.SendAsync(
            LocalizationService.GetString("ProductDeletedSuccessTitle"),
            string.Format(
                LocalizationService.Culture,
                LocalizationService.GetString("ProductDeletedSuccessMessage"),
                product.Name),
            NotificationType.Success);
    }

    private Task HandleFilterSubmittedAsync(ProductFilter criteria)
    {
        _activeProductTypeFilter = criteria.ProductType;
        _activePriceMinFilter = criteria.PriceMin;
        _activePriceMaxFilter = criteria.PriceMax;

        ApplyFiltersAndSorting(resetToFirstPage: true);
        return Task.CompletedTask;
    }

    private async Task HandleProductCreatedAsync(ProductModel product)
    {
        if (!_allProducts.Contains(product))
        {
            _allProducts.Insert(0, product);
            AddCardViewModelsForProduct(product);
        }

        ApplyFiltersAndSorting(resetToFirstPage: true);

        await _notificationService.SendAsync(
            LocalizationService.GetString("ProductCreatedSuccessTitle"),
            string.Format(
                LocalizationService.Culture,
                LocalizationService.GetString("ProductCreatedSuccessMessage"),
                product.Name),
            NotificationType.Success);
    }

    private async Task HandleProductUpdatedAsync(ProductModel product)
    {
        ApplyFiltersAndSorting(resetToFirstPage: false);

        await _notificationService.SendAsync(
            LocalizationService.GetString("ProductUpdatedSuccessTitle"),
            string.Format(
                LocalizationService.Culture,
                LocalizationService.GetString("ProductUpdatedSuccessMessage"),
                product.Name),
            NotificationType.Success);
    }

    private void ApplyFiltersAndSorting(bool resetToFirstPage)
    {
        IReadOnlyList<ProductModel> filteredProducts = _productFilterService.Apply(_allProducts, BuildCurrentFilterCriteria());

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filteredProducts = filteredProducts
                .Where(MatchesSearch)
                .ToList();
        }

        _filteredProducts = SortProducts(filteredProducts);
        _pagination.TotalItems = _filteredProducts.Count;

        if (resetToFirstPage)
        {
            if (_pagination.CurrentPage != 1)
            {
                _pagination.CurrentPage = 1;
                return;
            }

            RefreshPagedProducts();
            return;
        }

        RefreshPagedProducts();
    }

    private ProductFilter BuildCurrentFilterCriteria()
    {
        return new ProductFilter
        {
            ProductType = _activeProductTypeFilter,
            PriceMin = _activePriceMinFilter,
            PriceMax = _activePriceMaxFilter,
        };
    }

    private bool MatchesSearch(ProductModel product)
    {
        string productTypeDisplayName = GetProductTypeDisplayName(product.ProductType);

        return LocalizationService.Culture.CompareInfo.IndexOf(product.Name, SearchText, CompareOptions.IgnoreCase) >= 0
            || LocalizationService.Culture.CompareInfo.IndexOf(productTypeDisplayName, SearchText, CompareOptions.IgnoreCase) >= 0;
    }

    private IReadOnlyList<ProductModel> SortProducts(IEnumerable<ProductModel> products)
    {
        bool isDescending = string.Equals(SelectedSortDirection, SortDescending, StringComparison.Ordinal);

        IOrderedEnumerable<ProductModel> orderedProducts = (SelectedSortField ?? SortFieldName) switch
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

    private void RefreshPagedProducts()
    {
        PagedGridProductCards.Clear();
        PagedListProductCards.Clear();

        int startIndex = Math.Max(0, (_pagination.CurrentPage - 1) * _pagination.PageSize);
        foreach (ProductModel product in _filteredProducts.Skip(startIndex).Take(_pagination.PageSize))
        {
            if (_gridCardViewModelsByProduct.TryGetValue(product, out GridProductCardControlViewModel? gridViewModel))
            {
                PagedGridProductCards.Add(gridViewModel);
            }

            if (_listCardViewModelsByProduct.TryGetValue(product, out ListProductCardControlViewModel? listViewModel))
            {
                PagedListProductCards.Add(listViewModel);
            }
        }

        OnPropertyChanged(nameof(HasProducts));
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
                    new LocalizationOptionModel { Value = SortFieldName, DisplayName = LocalizationService.GetString("ProductManagementPageSortFieldNameOption") },
                    new LocalizationOptionModel { Value = SortFieldType, DisplayName = LocalizationService.GetString("ProductManagementPageSortFieldTypeOption") },
                    new LocalizationOptionModel { Value = SortFieldPrice, DisplayName = LocalizationService.GetString("ProductManagementPageSortFieldPriceOption") },
                ]);

            ReplaceOptions(
                SortDirectionOptions,
                [
                    new LocalizationOptionModel { Value = SortAscending, DisplayName = LocalizationService.GetString("ProductManagementPageSortDirectionAscendingOption") },
                    new LocalizationOptionModel { Value = SortDescending, DisplayName = LocalizationService.GetString("ProductManagementPageSortDirectionDescendingOption") },
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
        int totalItems = _filteredProducts.Count;
        int startItem = totalItems == 0 ? 0 : ((_pagination.CurrentPage - 1) * _pagination.PageSize) + 1;
        int endItem = totalItems == 0 ? 0 : Math.Min(_pagination.CurrentPage * _pagination.PageSize, totalItems);
        int totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)Math.Max(_pagination.PageSize, 1)));

        PageInfoText = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("ProductManagementPagePageInfoFormat"),
            startItem,
            endItem,
            totalItems,
            _pagination.CurrentPage,
            totalPages);
    }

    private GridProductCardControlViewModel CreateGridCardViewModel(ProductModel product)
    {
        return _productCardControlViewModelFactory.CreateGrid(
            product,
            HandleEditProductAsync,
            HandleDeleteProductAsync);
    }

    private ListProductCardControlViewModel CreateListCardViewModel(ProductModel product)
    {
        return _productCardControlViewModelFactory.CreateList(
            product,
            HandleEditProductAsync,
            HandleDeleteProductAsync);
    }

    private void AddCardViewModelsForProduct(ProductModel product)
    {
        _gridCardViewModelsByProduct[product] = CreateGridCardViewModel(product);
        _listCardViewModelsByProduct[product] = CreateListCardViewModel(product);
    }

    private void RemoveCardViewModelsForProduct(ProductModel product)
    {
        if (_gridCardViewModelsByProduct.Remove(product, out GridProductCardControlViewModel? gridViewModel))
        {
            gridViewModel.Dispose();
        }

        if (_listCardViewModelsByProduct.Remove(product, out ListProductCardControlViewModel? listViewModel))
        {
            listViewModel.Dispose();
        }
    }

    private ProductModel CreateNewProductDraft()
    {
        return new ProductModel
        {
            Name = string.Empty,
            ProductType = ProductType.Food,
            Price = 0m,
            StockQuantity = 0,
            ImageUri = "ms-appx:///Assets/Mock.png",
        };
    }

    private string GetProductTypeDisplayName(ProductType productType)
    {
        return productType switch
        {
            ProductType.Drink => LocalizationService.GetString("ProductTypeDrinkText"),
            _ => LocalizationService.GetString("ProductTypeFoodText"),
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

    private static int CalculateGridProductsColumnCount(double availableWidth)
    {
        if (availableWidth < ProductCardOuterWidth)
        {
            return 1;
        }

        int maxPossibleColumns = (int)((availableWidth + ProductCardsColumnSpacing) / (ProductCardOuterWidth + ProductCardsColumnSpacing));
        return Math.Max(1, Math.Min(PreferredGridProductsPerRow, maxPossibleColumns));
    }
}
