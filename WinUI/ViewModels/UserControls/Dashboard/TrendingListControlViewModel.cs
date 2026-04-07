using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels;

namespace WinUI.ViewModels.UserControls.Dashboard;

public partial class TrendingListControlViewModel : LocalizedViewModelBase
{
    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string GameLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string GameName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FoodLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FoodName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DrinkLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DrinkName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IconState IconState { get; set; } = new()
    {
        Kind = IconKind.Trending,
        AlwaysFilled = true,
        Size = 24,
    };

    public TrendingListControlViewModel(ILocalizationService localizationService)
        : base(localizationService)
    {
        RefreshLocalizedText();
    }

    protected override void RefreshLocalizedText()
    {
        Title = LocalizationService.GetString("TrendingListTitle");
        GameLabel = LocalizationService.GetString("TrendingListGameLabel");
        GameName = LocalizationService.GetString("TrendingListGameName");
        FoodLabel = LocalizationService.GetString("TrendingListFoodLabel");
        FoodName = LocalizationService.GetString("TrendingListFoodName");
        DrinkLabel = LocalizationService.GetString("TrendingListDrinkLabel");
        DrinkName = LocalizationService.GetString("TrendingListDrinkName");
    }
}
