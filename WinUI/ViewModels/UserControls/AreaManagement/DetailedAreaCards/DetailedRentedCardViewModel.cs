using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Enums;
using Microsoft.UI.Dispatching;
using WinUI.UIModels;
using WinUI.UIModels.Management;
using Application.Services.Areas;
using Application.Services.Games;
using Application.Services.Products;
using WinUI.ViewModels.Dialogs.Management;
using WinUI.Services.Dialogs;
using WinUI.UIModels.Enums;

namespace WinUI.ViewModels.AreaManagement.DetailedAreaCards;

public partial class DetailedRentedCardViewModel : LocalizedViewModelBase, IDetailedAreaCardViewModel, IDisposable
{
    private readonly IDialogService _dialogService;
    private readonly INotificationService _notificationService;
    private readonly IAreaSessionService _areaSessionService;
    private readonly IGameCatalogService _gameCatalogService;
    private readonly IProductCatalogService _productCatalogService;
    private readonly DispatcherQueueTimer? _timer;
    private bool _isDisposed;

    public DetailedRentedCardViewModel(
        ILocalizationService localizationService,
        IDialogService dialogService,
        INotificationService notificationService,
        IAreaSessionService areaSessionService,
        IGameCatalogService gameCatalogService,
        IProductCatalogService productCatalogService,
        AreaModel model)
        : base(localizationService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _areaSessionService = areaSessionService ?? throw new ArgumentNullException(nameof(areaSessionService));
        _gameCatalogService = gameCatalogService ?? throw new ArgumentNullException(nameof(gameCatalogService));
        _productCatalogService = productCatalogService ?? throw new ArgumentNullException(nameof(productCatalogService));
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Model.PropertyChanged += HandleModelPropertyChanged;

        PaidCommand = new AsyncRelayCommand(OpenPaymentDialogAsync);
        EndSessionCommand = new AsyncRelayCommand(ToggleSessionAsync);
        AddGameCommand = new RelayCommand<AvailableServiceItemModel>(AddGame);
        AddProductCommand = new RelayCommand<AvailableServiceItemModel>(AddProduct);

        var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        if (dispatcherQueue is not null)
        {
            _timer = dispatcherQueue.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += HandleTimerTick;
        }

        SyncTimerState();
        RefreshLocalizedText();
    }

    public AreaModel Model { get; }

    public string AreaName => Model.AreaName;

    public IconState IconState => CreateIconState();

    public IconState SessionInfoIconState { get; } = new()
    {
        Kind = IconKind.Clock,
        Size = 16,
        AlwaysFilled = true,
    };

    public IconState GameSectionIconState { get; } = new()
    {
        Kind = IconKind.Game,
        Size = 16,
        AlwaysFilled = true,
    };

    public IconState ProductSectionIconState { get; } = new()
    {
        Kind = IconKind.Dinner,
        Size = 16,
        AlwaysFilled = true,
    };

    public bool IsSessionPaused => Model.IsSessionPaused;

    public bool IsSessionActive => !Model.IsSessionPaused;

    [ObservableProperty]
    public partial string MaxCapacityText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PaidButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EndSessionButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SessionInfoTitleText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SessionStartTimeLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SessionStartTimeText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SessionDurationLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SessionDurationText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AreaHourlyPriceLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AreaHourlyPriceText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AreaSessionTotalPriceLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AreaSessionTotalPriceText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string GameServicesTitleText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AddGameButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<ServiceInSessionModel> GameServicesInSession { get; set; } = new();

    [ObservableProperty]
    public partial string GameServicesCurrentTotalPriceLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string GameServicesCurrentTotalPriceText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ProductServicesTitleText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AddProductButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<ServiceInSessionModel> ProductServicesInSession { get; set; } = new();

    [ObservableProperty]
    public partial string ProductServicesTotalPriceLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ProductServicesTotalPriceText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CurrentTotalPriceLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CurrentTotalPriceText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string GameSearchPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ProductSearchPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string GameSearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ProductSearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FoodPivotHeaderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DrinkPivotHeaderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AddItemButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NoDataText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoGames))]
    public partial IReadOnlyList<AvailableServiceItemModel> AvailableGames { get; set; } = Array.Empty<AvailableServiceItemModel>();

    public bool HasNoGames => AvailableGames.Count == 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoFoods))]
    public partial IReadOnlyList<AvailableServiceItemModel> AvailableFoods { get; set; } = Array.Empty<AvailableServiceItemModel>();

    public bool HasNoFoods => AvailableFoods.Count == 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoDrinks))]
    public partial IReadOnlyList<AvailableServiceItemModel> AvailableDrinks { get; set; } = Array.Empty<AvailableServiceItemModel>();

    public bool HasNoDrinks => AvailableDrinks.Count == 0;

    public IAsyncRelayCommand PaidCommand { get; }

    public IAsyncRelayCommand EndSessionCommand { get; }

    public IRelayCommand<AvailableServiceItemModel> AddGameCommand { get; }

    public IRelayCommand<AvailableServiceItemModel> AddProductCommand { get; }

    protected override void RefreshLocalizedText()
    {
        int normalizedMaxCapacity = Model.MaxCapacity > 0 ? Model.MaxCapacity : Model.Capacity;

        MaxCapacityText = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("DetailedRentedCardCapacityFormat"),
            Model.Capacity,
            normalizedMaxCapacity);

        PaidButtonText = LocalizationService.GetString("PaidButtonText");
        EndSessionButtonText = LocalizationService.GetString(
            Model.IsSessionPaused
                ? "ResumeSessionButtonText"
                : "EndSessionButtonText");

        SessionInfoTitleText = LocalizationService.GetString("SessionInfoTitleText");
        SessionStartTimeLabelText = LocalizationService.GetString("SessionStartTimeLabelText");
        SessionDurationLabelText = LocalizationService.GetString("SessionDurationLabelText");
        AreaHourlyPriceLabelText = LocalizationService.GetString("AreaHourlyPriceLabelText");
        AreaSessionTotalPriceLabelText = LocalizationService.GetString("AreaSessionTotalPriceLabelText");
        AreaHourlyPriceText = LocalizationService.FormatCurrency(Model.HourlyPrice);

        GameServicesTitleText = LocalizationService.GetString("GameServicesTitleText");
        AddGameButtonText = LocalizationService.GetString("AddGameButtonText");
        GameServicesCurrentTotalPriceLabelText = LocalizationService.GetString("GameServicesCurrentTotalPriceLabelText");

        ProductServicesTitleText = LocalizationService.GetString("ProductServicesTitleText");
        AddProductButtonText = LocalizationService.GetString("AddProductButtonText");
        ProductServicesTotalPriceLabelText = LocalizationService.GetString("ProductServicesTotalPriceLabelText");

        CurrentTotalPriceLabelText = LocalizationService.GetString("CurrentTotalPriceLabelText");

        GameSearchPlaceholderText = LocalizationService.GetString("DetailedRentedCardGameSearchPlaceholder");
        ProductSearchPlaceholderText = LocalizationService.GetString("DetailedRentedCardProductSearchPlaceholder");
        FoodPivotHeaderText = LocalizationService.GetString("DetailedRentedCardFoodPivotHeader");
        DrinkPivotHeaderText = LocalizationService.GetString("DetailedRentedCardDrinkPivotHeader");
        AddItemButtonText = LocalizationService.GetString("DetailedRentedCardAddItemButtonText");
        NoDataText = LocalizationService.GetString("DetailedRentedCardNoDataText");

        RefreshAvailableServices();
        UpdateDynamicTexts();
    }

    public new void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        if (_timer is not null)
        {
            _timer.Stop();
            _timer.Tick -= HandleTimerTick;
        }

        Model.PropertyChanged -= HandleModelPropertyChanged;

        _isDisposed = true;
        base.Dispose();
    }

    void IDisposable.Dispose()
    {
        Dispose();
    }



    private void RefreshAvailableServices()
    {
        var games = _gameCatalogService.GetGames();
        AvailableGames = games.Select(g => new AvailableServiceItemModel(
            g, 
            g.Name, 
            LocalizationService.FormatCurrency(g.HourlyPrice) + "/h")).ToList();

        var products = _productCatalogService.GetProducts();
        AvailableFoods = products
            .Where(p => p.Type == ProductType.Food)
            .Select(p => new AvailableServiceItemModel(
                p, 
                p.Name, 
                LocalizationService.FormatCurrency(p.Price))).ToList();

        AvailableDrinks = products
            .Where(p => p.Type == ProductType.Drink)
            .Select(p => new AvailableServiceItemModel(
                p, 
                p.Name, 
                LocalizationService.FormatCurrency(p.Price))).ToList();
    }

    private void HandleTimerTick(DispatcherQueueTimer sender, object args)
    {
        if (_isDisposed) return;
        
        UpdateDynamicTexts();
        foreach (var game in GameServicesInSession)
        {
            game.RefreshTimer();
        }
        UpdateTotals();
    }

    private void UpdateDynamicTexts()
    {
        DateTime startTime = Model.StartTime ?? DateTime.UtcNow;
        DateTime configuredStartTime = ConvertUtcToConfiguredTime(startTime);
        TimeSpan elapsedTime = _areaSessionService.GetSessionElapsedTime(Model, DateTime.UtcNow);

        decimal areaSessionTotal = _areaSessionService.CalculateAreaSessionTotal(Model, elapsedTime);
        decimal gameServicesTotal = GameServicesInSession.Sum(service => service.TotalPrice);
        decimal productServicesTotal = ProductServicesInSession.Sum(service => service.TotalPrice);
        decimal currentTotal = areaSessionTotal + gameServicesTotal + productServicesTotal;

        SessionStartTimeText = configuredStartTime.ToString("HH:mm:ss", LocalizationService.Culture);
        SessionDurationText = string.Format(
            LocalizationService.Culture,
            "{0:00}:{1:00}:{2:00}",
            (int)elapsedTime.TotalHours,
            elapsedTime.Minutes,
            elapsedTime.Seconds);

        AreaSessionTotalPriceText = LocalizationService.FormatCurrency(areaSessionTotal);
        Model.TotalAmount = currentTotal;
    }

    private DateTime ConvertUtcToConfiguredTime(DateTime value)
    {
        DateTime utcValue = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
        return utcValue.AddHours(ParseTimeZoneOffset(LocalizationService.TimeZone));
    }

    private static int ParseTimeZoneOffset(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        string normalized = value.Trim().ToUpperInvariant().Replace("UTC", string.Empty);
        return int.TryParse(normalized, out int offset) ? offset : 0;
    }

    private Task OpenPaymentDialogAsync()
    {
        return _dialogService.ShowDialogAsync(new PaymentDialogRequest { Model = Model });
    }

    private void AddGame(AvailableServiceItemModel? item)
    {
        if (item is null) return;
        
        GameServicesInSession.Add(new ServiceInSessionModel(
            LocalizationService,
            item.Name,
            ServiceInSessionModel.ServiceType.Game,
            null,
            1,
            ((Application.Games.GameRecord)item.Record).HourlyPrice,
            UpdateTotals,
            RemoveGame,
            HandleStopGameRequestedAsync
        ));
        UpdateTotals();
    }

    private async Task HandleStopGameRequestedAsync(ServiceInSessionModel game)
    {
        bool confirmed = await _dialogService.ShowConfirmationAsync(
            "StopGameConfirmTitle",
            "StopGameConfirmMessage",
            "StopGameConfirmButtonText",
            "CancelButtonText");
        if (confirmed)
        {
            game.EndTime = DateTime.Now;
            game.Refresh();
            UpdateTotals();
        }
    }

    private void RemoveGame(ServiceInSessionModel game)
    {
        GameServicesInSession.Remove(game);
        UpdateTotals();
    }

    private void AddProduct(AvailableServiceItemModel? item)
    {
        if (item is null) return;
        
        var product = (Application.Products.ProductRecord)item.Record;
        var existing = ProductServicesInSession.FirstOrDefault(p => p.Service == product.Name);
        if (existing is not null)
        {
            existing.IncrementQuantityCommand.Execute(null);
        }
        else
        {
            ProductServicesInSession.Add(new ServiceInSessionModel(
                LocalizationService,
                product.Name,
                ServiceInSessionModel.ServiceType.Product,
                null,
                1,
                product.Price,
                UpdateTotals,
                RemoveProduct
            ));
        }
        UpdateTotals();
    }

    private void RemoveProduct(ServiceInSessionModel product)
    {
        ProductServicesInSession.Remove(product);
        UpdateTotals();
    }

    private void UpdateTotals()
    {
        var gameTotal = GameServicesInSession.Sum(g => g.TotalPrice);
        GameServicesCurrentTotalPriceText = LocalizationService.FormatCurrency(gameTotal);

        var productTotal = ProductServicesInSession.Sum(p => p.TotalPrice);
        ProductServicesTotalPriceText = LocalizationService.FormatCurrency(productTotal);

        CurrentTotalPriceText = LocalizationService.FormatCurrency(gameTotal + productTotal);
    }

    private Task ToggleSessionAsync()
    {
        DateTime utcNow = DateTime.UtcNow;

        _areaSessionService.ToggleSession(Model, utcNow);

        SyncTimerState();
        RefreshLocalizedText();

        return Task.CompletedTask;
    }

    private void HandleModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isDisposed)
        {
            return;
        }

        if (e.PropertyName == nameof(Model.TotalAmount))
        {
            return;
        }

        SyncTimerState();
        OnPropertyChanged(nameof(AreaName));
        OnPropertyChanged(nameof(IconState));
        OnPropertyChanged(nameof(IsSessionPaused));
        OnPropertyChanged(nameof(IsSessionActive));
        RefreshLocalizedText();
    }

    private void SyncTimerState()
    {
        if (_timer is null)
        {
            return;
        }

        if (Model.IsSessionPaused)
        {
            _timer.Stop();
        }
        else
        {
            _timer.Start();
        }
    }

    private IconState CreateIconState()
    {
        return new IconState
        {
            Size = 24,
            Kind = Model.PlayAreaType switch
            {
                PlayAreaType.Table => IconKind.Table,
                PlayAreaType.Room => IconKind.Room,
                _ => IconKind.Table,
            },
            AlwaysFilled = true,
        };
    }
}
