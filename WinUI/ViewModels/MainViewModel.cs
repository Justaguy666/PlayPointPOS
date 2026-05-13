using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Services;
using Application.Services.Areas;
using Application.Services.Transactions;
using CommunityToolkit.Mvvm.ComponentModel;
using Domain.Enums;

namespace WinUI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private const double CompactNavigationBreakpoint = 920.0;
    private readonly ILocalizationService _localizationService;
    private readonly IAreaCatalogService _areaCatalogService;
    private readonly ITransactionCatalogService _transactionCatalogService;
    private decimal _todayRevenueAmount;
    private int _activeAreasCount;
    private int _reservedAreasCount;
    private int _totalAreasCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSidebarVisible))]
    [NotifyPropertyChangedFor(nameof(IsCompactNavigationVisible))]
    public partial bool IsNavigationVisible { get; set; } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSidebarVisible))]
    [NotifyPropertyChangedFor(nameof(IsCompactNavigationVisible))]
    public partial bool IsCompactNavigationMode { get; set; }

    [ObservableProperty]
    public partial string TodayRevenue { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ActiveAreas { get; set; } = "4/9";

    [ObservableProperty]
    public partial string ReservedAreas { get; set; } = "2";

    [ObservableProperty]
    public partial string TodayRevenueLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ActiveAreasLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ReservedAreasLabelText { get; set; } = string.Empty;

    public bool IsSidebarVisible => IsNavigationVisible && !IsCompactNavigationMode;

    public bool IsCompactNavigationVisible => IsNavigationVisible && IsCompactNavigationMode;

    public MainViewModel(
        ILocalizationService localizationService,
        IAreaCatalogService areaCatalogService,
        ITransactionCatalogService transactionCatalogService)
    {
        _localizationService = localizationService;
        _areaCatalogService = areaCatalogService;
        _transactionCatalogService = transactionCatalogService;
        _localizationService.LanguageChanged += UpdateHeaderTexts;
        _localizationService.CurrencyChanged += UpdateHeaderMetrics;

        UpdateHeaderTexts();
        _ = RefreshHeaderMetricsAsync();
    }

    public void UpdateNavigationLayout(double windowWidth)
    {
        if (double.IsNaN(windowWidth) || windowWidth <= 0)
            return;

        IsCompactNavigationMode = windowWidth < CompactNavigationBreakpoint;
    }

    private void UpdateHeaderTexts()
    {
        TodayRevenueLabelText = _localizationService.GetString("HeaderTodayRevenueLabel");
        ActiveAreasLabelText = _localizationService.GetString("HeaderActiveAreasLabel");
        ReservedAreasLabelText = _localizationService.GetString("HeaderReservedAreasLabel");
        UpdateHeaderMetrics();
    }

    private void UpdateHeaderMetrics()
    {
        TodayRevenue = _localizationService.FormatCurrency(_todayRevenueAmount);
        ActiveAreas = _totalAreasCount > 0
            ? $"{_activeAreasCount}/{_totalAreasCount}"
            : "0/0";
        ReservedAreas = _reservedAreasCount.ToString(_localizationService.Culture);
    }

    public async Task RefreshHeaderMetricsAsync()
    {
        (decimal todayRevenue, int totalAreas, int activeAreas, int reservedAreas) = await Task.Run(() =>
        {
            try
            {
                var areas = _areaCatalogService.GetAreas();
                var transactions = _transactionCatalogService.GetTransactions();
                DateTime today = DateTime.Now.Date;

                decimal todayRevenue = transactions
                    .Where(t => NormalizeToLocal(t.CreatedAt).Date == today)
                    .Sum(t => t.TotalAmount);
                int totalAreas = areas.Count;
                int activeAreas = areas.Count(area => area.Status == PlayAreaStatus.Rented);
                int reservedAreas = areas.Count(area => area.Status == PlayAreaStatus.Reserved);
                return (todayRevenue, totalAreas, activeAreas, reservedAreas);
            }
            catch
            {
                return (0m, 0, 0, 0);
            }
        });

        _todayRevenueAmount = todayRevenue;
        _totalAreasCount = totalAreas;
        _activeAreasCount = activeAreas;
        _reservedAreasCount = reservedAreas;
        UpdateHeaderMetrics();
    }

    private static DateTime NormalizeToLocal(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value.ToLocalTime(),
            DateTimeKind.Local => value,
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc).ToLocalTime(),
        };
    }
}
