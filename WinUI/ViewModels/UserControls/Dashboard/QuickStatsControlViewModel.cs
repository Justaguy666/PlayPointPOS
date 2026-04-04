using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels;

namespace WinUI.ViewModels.UserControls.Dashboard;

public partial class QuickStatsControlViewModel : LocalizedViewModelBase
{
    private const double AverageHourPerAreaMetric = 2.5;
    private const decimal AverageRevenuePerInvoiceMetric = 385_000m;
    private const double ReturnRateMetric = 68;
    private const double AreaUsageRateMetric = 68;
    private const int NewMembersPerMonthMetric = 23;

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AverageHourPerArea { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AverageRevenuePerInvoice { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ComebackPercent { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AreaCoveragePerHourPercent { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewMemberPerMonth { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AverageHourPerAreaValue { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AverageRevenuePerInvoiceValue { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ComebackPercentValue { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AreaCoveragePerHourPercentValue { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewMemberPerMonthValue { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IconState IconState { get; set; } = new()
    {
        Kind = IconKind.Stat,
        AlwaysFilled = true,
        Size = 24,
    };

    public QuickStatsControlViewModel(ILocalizationService localizationService)
        : base(localizationService)
    {
        RefreshLocalizedText();
    }

    protected override void RefreshLocalizedText()
    {
        Title = LocalizationService.GetString("QuickStatsControlTitle");
        AverageHourPerArea = LocalizationService.GetString("QuickStatsControlAverageHourPerArea");
        AverageRevenuePerInvoice = LocalizationService.GetString("QuickStatsControlAverageRevenuePerInvoice");
        ComebackPercent = LocalizationService.GetString("QuickStatsControlComebackPercent");
        AreaCoveragePerHourPercent = LocalizationService.GetString("QuickStatsControlAreaCoveragePerHourPercent");
        NewMemberPerMonth = LocalizationService.GetString("QuickStatsControlNewMemberPerMonth");

        AverageHourPerAreaValue = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("DashboardHourValueFormat"),
            AverageHourPerAreaMetric);

        AverageRevenuePerInvoiceValue = LocalizationService.FormatCurrency(AverageRevenuePerInvoiceMetric);

        ComebackPercentValue = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("DashboardPercentValueFormat"),
            ReturnRateMetric);

        AreaCoveragePerHourPercentValue = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("DashboardPercentValueFormat"),
            AreaUsageRateMetric);

        NewMemberPerMonthValue = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("DashboardSignedNumberValueFormat"),
            NewMembersPerMonthMetric);
    }
}
