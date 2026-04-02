using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels;

namespace WinUI.ViewModels.UserControls.Dashboard;

public partial class QuickStatsControlViewModel : LocalizedViewModelBase
{
    private const double AverageHourPerTableMetric = 2.5;
    private const decimal AverageRevenuePerInvoiceMetric = 385_000m;
    private const double ReturnRateMetric = 68;
    private const double TableUsageRateMetric = 68;
    private const int NewMembersPerMonthMetric = 23;

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AverageHourPerTable { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AverageRevenuePerInvoice { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ComebackPercent { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TableCoveragePerHourPercent { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewMemberPerMonth { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AverageHourPerTableValue { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AverageRevenuePerInvoiceValue { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ComebackPercentValue { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TableCoveragePerHourPercentValue { get; set; } = string.Empty;

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
        AverageHourPerTable = LocalizationService.GetString("QuickStatsControlAverageHourPerTable");
        AverageRevenuePerInvoice = LocalizationService.GetString("QuickStatsControlAverageRevenuePerInvoice");
        ComebackPercent = LocalizationService.GetString("QuickStatsControlComebackPercent");
        TableCoveragePerHourPercent = LocalizationService.GetString("QuickStatsControlTableCoveragePerHourPercent");
        NewMemberPerMonth = LocalizationService.GetString("QuickStatsControlNewMemberPerMonth");

        AverageHourPerTableValue = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("DashboardHourValueFormat"),
            AverageHourPerTableMetric);

        AverageRevenuePerInvoiceValue = LocalizationService.FormatCurrency(AverageRevenuePerInvoiceMetric);

        ComebackPercentValue = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("DashboardPercentValueFormat"),
            ReturnRateMetric);

        TableCoveragePerHourPercentValue = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("DashboardPercentValueFormat"),
            TableUsageRateMetric);

        NewMemberPerMonthValue = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("DashboardSignedNumberValueFormat"),
            NewMembersPerMonthMetric);
    }
}
