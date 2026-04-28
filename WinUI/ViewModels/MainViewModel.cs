using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WinUI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private const double CompactNavigationBreakpoint = 920.0;
    private const decimal HeaderTodayRevenueAmount = 1_005_000m;
    private readonly ILocalizationService _localizationService;

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

    public MainViewModel(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
        _localizationService.LanguageChanged += UpdateHeaderTexts;
        _localizationService.CurrencyChanged += UpdateHeaderMetrics;

        UpdateHeaderTexts();
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
        TodayRevenue = _localizationService.FormatCurrency(HeaderTodayRevenueAmount);
    }
}
