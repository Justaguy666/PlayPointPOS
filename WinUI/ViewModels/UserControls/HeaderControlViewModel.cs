using CommunityToolkit.Mvvm.ComponentModel;
using Application.Services;

namespace WinUI.ViewModels.UserControls;

public partial class HeaderControlViewModel : ObservableObject
{
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    public partial string TodayRevenueLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TodayRevenue { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ActiveAreasLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ActiveAreas { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ReservedAreasLabelText { get; set; } = string.Empty;

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
        TodayRevenueLabelText = _localizationService.GetString("HeaderTodayRevenueLabel");
        ActiveAreasLabelText = _localizationService.GetString("HeaderActiveAreasLabel");
        ReservedAreasLabelText = _localizationService.GetString("HeaderReservedAreasLabel");
    }
}
