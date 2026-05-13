using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using Application.Services;
using Application.Services.Products;
using Application.Services.Games;
using Application.Products;
using Application.Games;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Enums;
using Microsoft.UI.Dispatching;
using WinUI.Services;
using WinUI.UIModels;
using WinUI.UIModels.Management;
using WinUI.UIModels.Enums;
using Application.Services.Areas;

namespace WinUI.ViewModels.AreaManagement.DetailedAreaCards;

public partial class DetailedRentedCardViewModel : LocalizedViewModelBase, IDetailedAreaCardViewModel, IDisposable
{
    private readonly IDialogService _dialogService;
    private readonly IAreaSessionService _areaSessionService;
    private readonly SessionSalePickerService _sessionSalePicker;
    private readonly IProductCatalogService _productCatalog;
    private readonly IGameCatalogService _gameCatalog;
    private readonly DispatcherQueueTimer? _timer;
    private bool _isDisposed;

    public DetailedRentedCardViewModel(
        ILocalizationService localizationService,
        IDialogService dialogService,
        IAreaSessionService areaSessionService,
        SessionSalePickerService sessionSalePicker,
        IProductCatalogService productCatalog,
        IGameCatalogService gameCatalog,
        AreaModel model)
        : base(localizationService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _areaSessionService = areaSessionService ?? throw new ArgumentNullException(nameof(areaSessionService));
        _sessionSalePicker = sessionSalePicker ?? throw new ArgumentNullException(nameof(sessionSalePicker));
        _productCatalog = productCatalog ?? throw new ArgumentNullException(nameof(productCatalog));
        _gameCatalog = gameCatalog ?? throw new ArgumentNullException(nameof(gameCatalog));
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Model.PropertyChanged += HandleModelPropertyChanged;
        Model.PendingSessionLines.CollectionChanged += HandlePendingLinesChanged;

        PaidCommand = new AsyncRelayCommand(OpenPaymentDialogAsync);
        EndSessionCommand = new AsyncRelayCommand(ToggleSessionAsync);
        AddGameCommand = new AsyncRelayCommand(AddGameAsync);
        AddProductCommand = new AsyncRelayCommand(AddProductAsync);

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
    public partial IReadOnlyList<ServiceInSessionModel> GameServicesInSession { get; set; } = Array.Empty<ServiceInSessionModel>();

    [ObservableProperty]
    public partial string GameServicesCurrentTotalPriceLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string GameServicesCurrentTotalPriceText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ProductServicesTitleText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AddProductButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IReadOnlyList<ServiceInSessionModel> ProductServicesInSession { get; set; } = Array.Empty<ServiceInSessionModel>();

    [ObservableProperty]
    public partial string ProductServicesTotalPriceLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ProductServicesTotalPriceText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CurrentTotalPriceLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CurrentTotalPriceText { get; set; } = string.Empty;

    public IAsyncRelayCommand PaidCommand { get; }

    public IAsyncRelayCommand EndSessionCommand { get; }

    public IAsyncRelayCommand AddGameCommand { get; }

    public IAsyncRelayCommand AddProductCommand { get; }

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

        GameServicesInSession = BuildGameServices();
        ProductServicesInSession = BuildProductServices();

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
        Model.PendingSessionLines.CollectionChanged -= HandlePendingLinesChanged;

        _isDisposed = true;
        base.Dispose();
    }

    void IDisposable.Dispose()
    {
        Dispose();
    }

    private IReadOnlyList<ServiceInSessionModel> BuildGameServices()
    {
        var list = new List<ServiceInSessionModel>();

        foreach (PendingSessionSaleLine line in Model.PendingSessionLines.Where(l => l.IsGame))
        {
            DateTime? startLocal = line.GameRentalStartUtc is DateTime utcStart
                ? ConvertUtcToConfiguredTime(utcStart)
                : null;

            list.Add(new ServiceInSessionModel(
                LocalizationService,
                line.DisplayName,
                ServiceInSessionModel.ServiceType.Game,
                startLocal,
                null,
                line.UnitPrice));
        }

        HashSet<int> rentedGameIds = Model.PendingSessionLines
            .Where(l => l.IsGame)
            .Select(l => l.CatalogId)
            .ToHashSet();

        foreach (GameRecord game in _gameCatalog.GetGames().OrderBy(g => g.Name))
        {
            if (!int.TryParse(game.Id?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int gid) || gid <= 0)
            {
                continue;
            }

            if (game.StockQuantity <= 0)
            {
                continue;
            }

            if (rentedGameIds.Contains(gid))
            {
                continue;
            }

            list.Add(new ServiceInSessionModel(
                LocalizationService,
                game.Name,
                ServiceInSessionModel.ServiceType.Game,
                startTime: null,
                quantity: null,
                game.HourlyPrice));
        }

        return list;
    }

    private IReadOnlyList<ServiceInSessionModel> BuildProductServices()
    {
        var list = new List<ServiceInSessionModel>();

        foreach (PendingSessionSaleLine line in Model.PendingSessionLines.Where(l => !l.IsGame))
        {
            list.Add(new ServiceInSessionModel(
                LocalizationService,
                line.DisplayName,
                ServiceInSessionModel.ServiceType.Product,
                null,
                line.ProductQuantity,
                line.UnitPrice));
        }

        HashSet<int> chargedProductIds = Model.PendingSessionLines
            .Where(l => !l.IsGame)
            .Select(l => l.CatalogId)
            .ToHashSet();

        foreach (ProductRecord product in _productCatalog.GetProducts().OrderBy(p => p.Name))
        {
            if (!int.TryParse(product.Id?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int pid) || pid <= 0)
            {
                continue;
            }

            if (chargedProductIds.Contains(pid))
            {
                continue;
            }

            list.Add(new ServiceInSessionModel(
                LocalizationService,
                product.Name,
                ServiceInSessionModel.ServiceType.Product,
                null,
                quantity: null,
                product.Price));
        }

        return list;
    }

    private void HandlePendingLinesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_isDisposed)
        {
            return;
        }

        RefreshLocalizedText();
    }

    private void HandleTimerTick(DispatcherQueueTimer sender, object args)
    {
        if (_isDisposed)
        {
            return;
        }

        UpdateDynamicTexts();
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
        GameServicesCurrentTotalPriceText = LocalizationService.FormatCurrency(gameServicesTotal);
        ProductServicesTotalPriceText = LocalizationService.FormatCurrency(productServicesTotal);
        CurrentTotalPriceText = LocalizationService.FormatCurrency(currentTotal);
        if (Model.TotalAmount != currentTotal)
        {
            Model.TotalAmount = currentTotal;
        }
    }

    private DateTime ConvertUtcToConfiguredTime(DateTime value)
    {
        DateTime utcValue = value.Kind switch
        {
            DateTimeKind.Utc => value,
            // In this flow, timestamps from backend are UTC instants even when kind metadata is inconsistent.
            DateTimeKind.Local => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };

        int configuredOffset = ParseTimeZoneOffset(LocalizationService.TimeZone);
        return utcValue.AddHours(configuredOffset);
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
        return _dialogService.ShowDialogAsync("Payment", Model);
    }

    private async Task AddGameAsync()
    {
        PendingSessionSaleLine? line = await _sessionSalePicker.PickGameAsync();
        if (line is not null)
        {
            Model.PendingSessionLines.Add(line);
        }
    }

    private async Task AddProductAsync()
    {
        PendingSessionSaleLine? line = await _sessionSalePicker.PickProductAsync();
        if (line is not null)
        {
            Model.PendingSessionLines.Add(line);
        }
    }

    private Task ToggleSessionAsync()
    {
        DateTime utcNow = DateTime.UtcNow;

        if (Model.IsSessionPaused)
        {
            _areaSessionService.ResumeSession(Model, utcNow);
        }
        else
        {
            _areaSessionService.PauseSession(Model, utcNow);
        }

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

        // Guard against self-triggered recursion:
        // UpdateDynamicTexts sets Model.TotalAmount, which raises PropertyChanged.
        if (string.Equals(e.PropertyName, nameof(AreaModel.TotalAmount), StringComparison.Ordinal))
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
