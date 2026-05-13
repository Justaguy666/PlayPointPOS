using System;
using System.Collections.Generic;
using System.Linq;
using Application.Areas;
using Application.Members;
using Application.Products;
using Application.Services.Areas;
using Application.Services.Members;
using Application.Services.Products;
using Application.Services.Transactions;
using Domain.Entities;
using Domain.Enums;
using WinUI.Services.Factories;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.UserControls.Dashboard;

namespace WinUI.ViewModels.Pages;

public sealed class DashboardPageViewModel : IDisposable
{

    private readonly IDisposable[] _ownedViewModels;
    private bool _isDisposed;

    public DashboardPageViewModel(
        StatCardControlViewModelFactory statCardViewModelFactory,
        PopularCardControlViewModelFactory popularCardViewModelFactory,
        ITransactionCatalogService transactionCatalogService,
        IAreaCatalogService areaCatalogService,
        IMemberCatalogService memberCatalogService,
        IProductCatalogService productCatalogService,
        RevenueChartControlViewModel revenueChartViewModel,
        TrendingListControlViewModel trendingListViewModel,
        QuickStatsControlViewModel quickStatsViewModel,
        GoalProgressControlViewModel goalProgressViewModel)
    {
        ArgumentNullException.ThrowIfNull(statCardViewModelFactory);
        ArgumentNullException.ThrowIfNull(popularCardViewModelFactory);
        ArgumentNullException.ThrowIfNull(transactionCatalogService);
        ArgumentNullException.ThrowIfNull(areaCatalogService);
        ArgumentNullException.ThrowIfNull(memberCatalogService);
        ArgumentNullException.ThrowIfNull(productCatalogService);

        IReadOnlyList<Transaction> transactions = transactionCatalogService.GetTransactions();
        IReadOnlyList<AreaRecord> areas = areaCatalogService.GetAreas();
        IReadOnlyList<MemberRecord> members = memberCatalogService.GetMembers();
        IReadOnlyList<ProductRecord> products = productCatalogService.GetProducts();
        Dictionary<string, ProductType> productTypeByName = products
            .Where(p => !string.IsNullOrWhiteSpace(p.Name))
            .GroupBy(p => p.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Type, StringComparer.OrdinalIgnoreCase);

        DateTime todayLocalDate = DateTime.Now.Date;
        List<Transaction> todayTransactions = transactions
            .Where(t => NormalizeToLocal(t.CreatedAt).Date == todayLocalDate)
            .ToList();

        decimal todayRevenueAmount = todayTransactions.Sum(t => t.TotalAmount);
        int todayGameSessionCount = todayTransactions
            .SelectMany(t => t.Lines)
            .Count(line => line.Type == TransactionLineType.AreaRental);
        int todayCustomerCount = todayTransactions
            .Select(BuildCustomerIdentity)
            .Where(identity => !string.IsNullOrWhiteSpace(identity))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        decimal todayFoodAndDrinkAmount = todayTransactions
            .SelectMany(t => t.Lines)
            .Where(line => line.Type == TransactionLineType.ProductSale)
            .Sum(line => line.TotalAmount);

        IReadOnlyList<TransactionLine> allLines = transactions.SelectMany(t => t.Lines).ToList();
        IReadOnlyList<PopularCardItemData> topGameItems = BuildPopularItems(
            allLines.Where(line => line.Type == TransactionLineType.BoardGameRental));
        IReadOnlyList<PopularCardItemData> topFoodItems = BuildPopularItems(
            allLines.Where(line =>
                line.Type == TransactionLineType.ProductSale
                && IsProductType(line.ItemName, productTypeByName, ProductType.Food)));
        IReadOnlyList<PopularCardItemData> topDrinkItems = BuildPopularItems(
            allLines.Where(line =>
                line.Type == TransactionLineType.ProductSale
                && IsProductType(line.ItemName, productTypeByName, ProductType.Drink)));
        string topGameName = topGameItems.FirstOrDefault()?.Name ?? "-";
        string topFoodName = topFoodItems.FirstOrDefault()?.Name ?? "-";
        string topDrinkName = topDrinkItems.FirstOrDefault()?.Name ?? "-";

        DateTime now = DateTime.Now;
        DateTime monthStart = new DateTime(now.Year, now.Month, 1);
        List<Transaction> monthTransactions = transactions
            .Where(t => NormalizeToLocal(t.CreatedAt) >= monthStart)
            .ToList();
        int occupiedAreas = areas.Count(area => area.Status is PlayAreaStatus.Rented or PlayAreaStatus.Reserved);
        int totalAreas = areas.Count;
        double averageHoursPerArea = areas
            .Where(area => area.Status == PlayAreaStatus.Rented && area.StartTime.HasValue)
            .Select(area => Math.Max(0d, (DateTime.UtcNow - area.StartTime!.Value).TotalHours))
            .DefaultIfEmpty(0d)
            .Average();
        decimal averageRevenuePerInvoice = todayTransactions.Count == 0
            ? 0m
            : todayTransactions.Average(t => t.TotalAmount);
        double returnRate = todayTransactions.Count == 0
            ? 0d
            : (double)todayTransactions.Count(t => !string.IsNullOrWhiteSpace(t.MemberId)) / todayTransactions.Count * 100d;
        double areaUsageRate = totalAreas == 0
            ? 0d
            : (double)occupiedAreas / totalAreas * 100d;
        int newMembersThisMonth = monthTransactions
            .Where(t => !string.IsNullOrWhiteSpace(t.MemberId))
            .Select(t => t.MemberId!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        decimal monthRevenue = monthTransactions.Sum(t => t.TotalAmount);
        int monthRevenueInMillions = (int)Math.Round(monthRevenue / 1_000_000m, MidpointRounding.AwayFromZero);
        int monthCustomerCount = monthTransactions
            .Select(BuildCustomerIdentity)
            .Where(identity => !string.IsNullOrWhiteSpace(identity))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        int memberCount = members.Count;

        TodayRevenueStatCardViewModel = statCardViewModelFactory.Create(
            "TodayRevenueStatCard",
            IconKind.BagOfCoins,
            usesPositiveTrendColors: true,
            localizationService => localizationService.FormatCurrency(todayRevenueAmount));

        TodayGameSessionStatCardViewModel = statCardViewModelFactory.Create(
            "TodayGameSessionStatCard",
            IconKind.Dice,
            usesPositiveTrendColors: true,
            localizationService => todayGameSessionCount.ToString(localizationService.Culture));

        TodayCustomerStatCardViewModel = statCardViewModelFactory.Create(
            "TodayCustomerStatCard",
            IconKind.Customer,
            usesPositiveTrendColors: false,
            localizationService => todayCustomerCount.ToString(localizationService.Culture));

        TodayProductStatCardViewModel = statCardViewModelFactory.Create(
            "TodayProductStatCard",
            IconKind.Dinner,
            usesPositiveTrendColors: true,
            localizationService => localizationService.FormatCurrency(todayFoodAndDrinkAmount));

        TopGamesCardViewModel = popularCardViewModelFactory.Create(
            "PopularGamesCard",
            IconKind.Game,
            "PopularGamesActivityFormat",
            topGameItems);

        TopFoodsCardViewModel = popularCardViewModelFactory.Create(
            "PopularFoodsCard",
            IconKind.Food,
            "PopularFoodsActivityFormat",
            topFoodItems);

        TopDrinksCardViewModel = popularCardViewModelFactory.Create(
            "PopularDrinksCard",
            IconKind.Drink,
            "PopularDrinksActivityFormat",
            topDrinkItems);

        RevenueChartViewModel = revenueChartViewModel ?? throw new ArgumentNullException(nameof(revenueChartViewModel));
        TrendingListViewModel = trendingListViewModel ?? throw new ArgumentNullException(nameof(trendingListViewModel));
        QuickStatsViewModel = quickStatsViewModel ?? throw new ArgumentNullException(nameof(quickStatsViewModel));
        GoalProgressViewModel = goalProgressViewModel ?? throw new ArgumentNullException(nameof(goalProgressViewModel));
        TrendingListViewModel.ApplyTrending(topGameName, topFoodName, topDrinkName);
        QuickStatsViewModel.ApplyMetrics(
            averageHoursPerArea: Math.Round(averageHoursPerArea, 1, MidpointRounding.AwayFromZero),
            averageRevenuePerInvoice: averageRevenuePerInvoice,
            returnRate: Math.Round(returnRate, 1, MidpointRounding.AwayFromZero),
            areaUsageRate: Math.Round(areaUsageRate, 1, MidpointRounding.AwayFromZero),
            newMembersPerMonth: newMembersThisMonth);
        GoalProgressViewModel.ApplyCurrentValues(
            revenueCurrentValue: monthRevenueInMillions,
            customerCurrentValue: monthCustomerCount,
            memberCurrentValue: memberCount);

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

    private static DateTime NormalizeToLocal(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value.ToLocalTime(),
            DateTimeKind.Local => value,
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc).ToLocalTime(),
        };
    }

    private static string BuildCustomerIdentity(Transaction transaction)
    {
        string? memberId = transaction.MemberId?.Trim();
        if (!string.IsNullOrWhiteSpace(memberId))
        {
            return $"member:{memberId}";
        }

        string customer = transaction.CustomerName?.Trim() ?? string.Empty;
        return string.IsNullOrWhiteSpace(customer)
            ? string.Empty
            : $"customer:{customer}";
    }

    private static bool IsProductType(
        string itemName,
        IReadOnlyDictionary<string, ProductType> productTypeByName,
        ProductType expected)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            return false;
        }

        return productTypeByName.TryGetValue(itemName.Trim(), out ProductType productType)
            && productType == expected;
    }

    private static IReadOnlyList<PopularCardItemData> BuildPopularItems(IEnumerable<TransactionLine> lines)
    {
        return lines
            .Where(line => !string.IsNullOrWhiteSpace(line.ItemName))
            .GroupBy(line => line.ItemName.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                decimal quantity = group.Sum(line => line.Quantity);
                int activityCount = Math.Max(1, (int)Math.Round(quantity, MidpointRounding.AwayFromZero));
                decimal amount = group.Sum(line => line.TotalAmount);
                return new
                {
                    Name = group.Key,
                    ActivityCount = activityCount,
                    Amount = amount,
                };
            })
            .OrderByDescending(item => item.Amount)
            .ThenByDescending(item => item.ActivityCount)
            .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .Select((item, index) => new PopularCardItemData(
                Rank: index + 1,
                Name: item.Name,
                ActivityCount: item.ActivityCount,
                Amount: item.Amount))
            .ToList();
    }

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
