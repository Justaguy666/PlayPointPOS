using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels;

namespace WinUI.ViewModels.UserControls.Dashboard;

public partial class QuickStatsControlViewModel : LocalizedViewModelBase
{
    private const double AverageHoursPerAreaMetric = 2.5;
    private const decimal AverageRevenuePerInvoiceMetric = 385_000m;
    private const double ReturnRateMetric = 68;
    private const double AreaUsageRateMetric = 68;
    private const int NewMembersPerMonthMetric = 23;

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AverageHoursPerAreaLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AverageRevenuePerInvoiceLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ReturnRateLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AreaUsageRateLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewMembersPerMonthLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AverageHoursPerAreaValue { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AverageRevenuePerInvoiceValue { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ReturnRateValue { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AreaUsageRateValue { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewMembersPerMonthValue { get; set; } = string.Empty;

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
        Title = LocalizationService.GetString("QuickStatsTitle");
        AverageHoursPerAreaLabel = LocalizationService.GetString("QuickStatsAverageHoursPerAreaLabel");
        AverageRevenuePerInvoiceLabel = LocalizationService.GetString("QuickStatsAverageRevenuePerInvoiceLabel");
        ReturnRateLabel = LocalizationService.GetString("QuickStatsReturnRateLabel");
        AreaUsageRateLabel = LocalizationService.GetString("QuickStatsAreaUsageRateLabel");
        NewMembersPerMonthLabel = LocalizationService.GetString("QuickStatsNewMembersPerMonthLabel");

        AverageHoursPerAreaValue = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("DashboardHourValueFormat"),
            AverageHoursPerAreaMetric);

        AverageRevenuePerInvoiceValue = LocalizationService.FormatCurrency(AverageRevenuePerInvoiceMetric);

        ReturnRateValue = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("DashboardPercentValueFormat"),
            ReturnRateMetric);

        AreaUsageRateValue = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("DashboardPercentValueFormat"),
            AreaUsageRateMetric);

        NewMembersPerMonthValue = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("DashboardSignedNumberValueFormat"),
            NewMembersPerMonthMetric);
    }
}
