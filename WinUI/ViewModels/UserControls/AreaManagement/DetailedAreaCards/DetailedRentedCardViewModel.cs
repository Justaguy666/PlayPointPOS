using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Enums;
using Microsoft.UI.Dispatching;
using WinUI.UIModels;
using WinUI.UIModels.AreaManagement;
using WinUI.UIModels.Enums;

namespace WinUI.ViewModels.AreaManagement.DetailedAreaCards;

public partial class DetailedRentedCardViewModel : LocalizedViewModelBase, IDetailedAreaCardViewModel, IDisposable
{
    private readonly IDialogService _dialogService;
    private readonly DispatcherQueueTimer? _timer;
    private bool _isDisposed;

    public DetailedRentedCardViewModel(
        ILocalizationService localizationService,
        IDialogService dialogService,
        AreaModel model)
        : base(localizationService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Model.PropertyChanged += HandleModelPropertyChanged;

        PaidCommand = new AsyncRelayCommand(OpenPaymentDialogAsync);
        EndSessionCommand = new AsyncRelayCommand(ToggleSessionAsync);
        AddGameCommand = new AsyncRelayCommand(ExecuteNoopAsync);
        AddProductCommand = new AsyncRelayCommand(ExecuteNoopAsync);

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

        _isDisposed = true;
        base.Dispose();
    }

    void IDisposable.Dispose()
    {
        Dispose();
    }

    private IReadOnlyList<ServiceInSessionModel> BuildGameServices()
    {
        return Array.Empty<ServiceInSessionModel>();
    }

    private IReadOnlyList<ServiceInSessionModel> BuildProductServices()
    {
        return Array.Empty<ServiceInSessionModel>();
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
        TimeSpan elapsedTime = Model.GetSessionElapsedTime(DateTime.UtcNow);

        decimal areaSessionTotal = CalculateAreaSessionTotal(elapsedTime);
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
        Model.TotalAmount = currentTotal;
    }

    private decimal CalculateAreaSessionTotal(TimeSpan elapsedTime)
    {
        if (Model.HourlyPrice <= 0m || elapsedTime <= TimeSpan.Zero)
        {
            return 0m;
        }

        int billableHalfHours = Math.Max(1, (int)Math.Floor(elapsedTime.TotalMinutes / 30d));
        return Model.HourlyPrice * billableHalfHours / 2m;
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

    private static Task ExecuteNoopAsync()
    {
        return Task.CompletedTask;
    }

    private Task OpenPaymentDialogAsync()
    {
        return _dialogService.ShowDialogAsync("Payment", Model);
    }

    private Task ToggleSessionAsync()
    {
        DateTime utcNow = DateTime.UtcNow;

        if (Model.IsSessionPaused)
        {
            Model.ResumeSession(utcNow);
        }
        else
        {
            Model.PauseSession(utcNow);
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
