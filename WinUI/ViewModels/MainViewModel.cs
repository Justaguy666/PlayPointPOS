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
    public partial string ActiveTables { get; set; } = "4/9";

    [ObservableProperty]
    public partial string ReservedTables { get; set; } = "2";

    [ObservableProperty]
    public partial string TodayRevenueLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ActiveTablesLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ReservedTablesLabelDisplay { get; set; } = string.Empty;

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
        ActiveTablesLabelDisplay = _localizationService.GetString("HeaderActiveTablesLabel");
        ReservedTablesLabelDisplay = _localizationService.GetString("HeaderReservedTablesLabel");
        UpdateHeaderMetrics();
    }

    private void UpdateHeaderMetrics()
    {
        TodayRevenue = _localizationService.FormatCurrency(HeaderTodayRevenueAmount);
    }
}
