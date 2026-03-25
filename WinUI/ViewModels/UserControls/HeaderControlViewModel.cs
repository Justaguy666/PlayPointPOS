using CommunityToolkit.Mvvm.ComponentModel;
using Application.Services;

namespace WinUI.ViewModels.UserControls;

public partial class HeaderControlViewModel : ObservableObject
{
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    public partial string TodayRevenueLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TodayRevenue { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ActiveTablesLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ActiveTables { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ReservedTablesLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ReservedTables { get; set; } = string.Empty;

    public HeaderControlViewModel(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
        _localizationService.LanguageChanged += UpdateTexts;
        UpdateTexts();
    }

    private void UpdateTexts()
    {
        TodayRevenueLabelDisplay = _localizationService.GetString("HeaderTodayRevenueLabel");
        ActiveTablesLabelDisplay = _localizationService.GetString("HeaderActiveTablesLabel");
        ReservedTablesLabelDisplay = _localizationService.GetString("HeaderReservedTablesLabel");
    }
}
