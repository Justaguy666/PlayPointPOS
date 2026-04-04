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
    public partial string ActiveAreasLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ActiveAreas { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ReservedAreasLabelDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ReservedAreas { get; set; } = string.Empty;

    public HeaderControlViewModel(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
        _localizationService.LanguageChanged += UpdateTexts;
        UpdateTexts();
    }

    private void UpdateTexts()
    {
        TodayRevenueLabelDisplay = _localizationService.GetString("HeaderTodayRevenueLabel");
        ActiveAreasLabelDisplay = _localizationService.GetString("HeaderActiveAreasLabel");
        ReservedAreasLabelDisplay = _localizationService.GetString("HeaderReservedAreasLabel");
    }
}
