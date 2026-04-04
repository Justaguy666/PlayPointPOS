using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WinUI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private const decimal HeaderTodayRevenueAmount = 1_005_000m;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    public partial bool IsNavigationVisible { get; set; } = true;

    [ObservableProperty]
    public partial string TodayRevenue { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ActiveAreas { get; set; } = "4/9";

    [ObservableProperty]
    public partial string ReservedAreas { get; set; } = "2";

    [ObservableProperty]
    public partial string TodayRevenueLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ActiveAreasLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ReservedAreasLabelDisplay { get; set; } = string.Empty;

    public MainViewModel(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
        _localizationService.LanguageChanged += UpdateHeaderTexts;
        _localizationService.CurrencyChanged += UpdateHeaderMetrics;

        UpdateHeaderTexts();
    }

    private void UpdateHeaderTexts()
    {
        TodayRevenueLabelDisplay = _localizationService.GetString("HeaderTodayRevenueLabel");
        ActiveAreasLabelDisplay = _localizationService.GetString("HeaderActiveAreasLabel");
        ReservedAreasLabelDisplay = _localizationService.GetString("HeaderReservedAreasLabel");
        UpdateHeaderMetrics();
    }

    private void UpdateHeaderMetrics()
    {
        TodayRevenue = _localizationService.FormatCurrency(HeaderTodayRevenueAmount);
    }
}
