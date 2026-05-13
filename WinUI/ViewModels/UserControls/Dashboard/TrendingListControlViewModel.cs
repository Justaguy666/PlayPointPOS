using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels;

namespace WinUI.ViewModels.UserControls.Dashboard;

public partial class TrendingListControlViewModel : LocalizedViewModelBase
{
    private string _topGameName = string.Empty;
    private string _topFoodName = string.Empty;
    private string _topDrinkName = string.Empty;

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
        GameName = _topGameName;
        FoodLabel = LocalizationService.GetString("TrendingListFoodLabel");
        FoodName = _topFoodName;
        DrinkLabel = LocalizationService.GetString("TrendingListDrinkLabel");
        DrinkName = _topDrinkName;
    }

    public void ApplyTrending(string? topGameName, string? topFoodName, string? topDrinkName)
    {
        _topGameName = string.IsNullOrWhiteSpace(topGameName) ? "-" : topGameName.Trim();
        _topFoodName = string.IsNullOrWhiteSpace(topFoodName) ? "-" : topFoodName.Trim();
        _topDrinkName = string.IsNullOrWhiteSpace(topDrinkName) ? "-" : topDrinkName.Trim();
        RefreshLocalizedText();
    }
}
