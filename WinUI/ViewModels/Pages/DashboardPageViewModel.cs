using System;
using WinUI.Services.Factories;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.UserControls.Dashboard;

namespace WinUI.ViewModels.Pages;

public sealed class DashboardPageViewModel : IDisposable
{
    private const decimal TodayRevenueAmount = 12_450_000m;
    private const int TodayGameSessionCount = 234;
    private const int TodayCustomerCount = 156;
    private const decimal TodayFoodAndDrinkAmount = 5_680_000m;

    private readonly IDisposable[] _ownedViewModels;
    private bool _isDisposed;

    public DashboardPageViewModel(
        StatCardControlViewModelFactory statCardViewModelFactory,
        PopularCardControlViewModelFactory popularCardViewModelFactory,
        RevenueChartControlViewModel revenueChartViewModel,
        TrendingListControlViewModel trendingListViewModel,
        QuickStatsControlViewModel quickStatsViewModel,
        GoalProgressControlViewModel goalProgressViewModel)
    {
        ArgumentNullException.ThrowIfNull(statCardViewModelFactory);
        ArgumentNullException.ThrowIfNull(popularCardViewModelFactory);

        TodayRevenueStatCardViewModel = statCardViewModelFactory.Create(
            "TodayRevenueStatCard",
            IconKind.BagOfCoins,
            usesPositiveTrendColors: true,
            localizationService => localizationService.FormatCurrency(TodayRevenueAmount));

        TodayGameSessionStatCardViewModel = statCardViewModelFactory.Create(
            "TodayGameSessionStatCard",
            IconKind.Dice,
            usesPositiveTrendColors: true,
            localizationService => TodayGameSessionCount.ToString(localizationService.Culture));

        TodayCustomerStatCardViewModel = statCardViewModelFactory.Create(
            "TodayCustomerStatCard",
            IconKind.Customer,
            usesPositiveTrendColors: false,
            localizationService => TodayCustomerCount.ToString(localizationService.Culture));

        TodayProductStatCardViewModel = statCardViewModelFactory.Create(
            "TodayProductStatCard",
            IconKind.Dinner,
            usesPositiveTrendColors: true,
            localizationService => localizationService.FormatCurrency(TodayFoodAndDrinkAmount));

        TopGamesCardViewModel = popularCardViewModelFactory.Create(
            "PopularGamesCard",
            IconKind.Game,
            "PopularGamesActivityFormat",
            [
                new PopularCardItemData(1, "Catan", 45, 2_250_000m),
                new PopularCardItemData(2, "Uno", 38, 1_900_000m),
                new PopularCardItemData(3, "Monopoly", 32, 1_600_000m),
                new PopularCardItemData(4, "Exploding Kittens", 28, 1_400_000m),
                new PopularCardItemData(5, "Codenames", 25, 1_250_000m),
            ]);

        TopFoodsCardViewModel = popularCardViewModelFactory.Create(
            "PopularFoodsCard",
            IconKind.Food,
            "PopularFoodsActivityFormat",
            [
                new PopularCardItemData(1, "Spicy noodles", 58, 1_740_000m),
                new PopularCardItemData(2, "Fried chicken", 47, 1_410_000m),
                new PopularCardItemData(3, "French fries", 41, 820_000m),
                new PopularCardItemData(4, "Cheese sticks", 33, 990_000m),
                new PopularCardItemData(5, "Sausage skewers", 29, 870_000m),
            ]);

        TopDrinksCardViewModel = popularCardViewModelFactory.Create(
            "PopularDrinksCard",
            IconKind.Drink,
            "PopularDrinksActivityFormat",
            [
                new PopularCardItemData(1, "B\u1EA1c x\u1EC9u", 62, 1_550_000m),
                new PopularCardItemData(2, "Peach tea", 54, 1_350_000m),
                new PopularCardItemData(3, "Matcha latte", 46, 1_380_000m),
                new PopularCardItemData(4, "Americano", 35, 875_000m),
                new PopularCardItemData(5, "Mojito", 28, 980_000m),
            ]);

        RevenueChartViewModel = revenueChartViewModel ?? throw new ArgumentNullException(nameof(revenueChartViewModel));
        TrendingListViewModel = trendingListViewModel ?? throw new ArgumentNullException(nameof(trendingListViewModel));
        QuickStatsViewModel = quickStatsViewModel ?? throw new ArgumentNullException(nameof(quickStatsViewModel));
        GoalProgressViewModel = goalProgressViewModel ?? throw new ArgumentNullException(nameof(goalProgressViewModel));

        _ownedViewModels =
        [
            TodayRevenueStatCardViewModel,
            TodayGameSessionStatCardViewModel,
            TodayCustomerStatCardViewModel,
            TodayProductStatCardViewModel,
            TopGamesCardViewModel,
            TopFoodsCardViewModel,
            TopDrinksCardViewModel,
            RevenueChartViewModel,
            TrendingListViewModel,
            QuickStatsViewModel,
            GoalProgressViewModel,
        ];
    }

    public StatCardControlViewModel TodayRevenueStatCardViewModel { get; }

    public StatCardControlViewModel TodayGameSessionStatCardViewModel { get; }

    public StatCardControlViewModel TodayCustomerStatCardViewModel { get; }

    public StatCardControlViewModel TodayProductStatCardViewModel { get; }

    public RevenueChartControlViewModel RevenueChartViewModel { get; }

    public TrendingListControlViewModel TrendingListViewModel { get; }

    public QuickStatsControlViewModel QuickStatsViewModel { get; }

    public GoalProgressControlViewModel GoalProgressViewModel { get; }

    public PopularCardControlViewModel TopGamesCardViewModel { get; }

    public PopularCardControlViewModel TopFoodsCardViewModel { get; }

    public PopularCardControlViewModel TopDrinksCardViewModel { get; }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        foreach (var viewModel in _ownedViewModels)
        {
            viewModel.Dispose();
        }
    }
}
