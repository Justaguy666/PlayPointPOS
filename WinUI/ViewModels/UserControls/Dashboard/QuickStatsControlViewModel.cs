using System;
using System.Collections.Generic;
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
    public partial IReadOnlyList<LabelValueRowModel> MetricRows { get; set; } = Array.Empty<LabelValueRowModel>();

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
        string averageHoursPerAreaValue = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("DashboardHourValueFormat"),
            AverageHoursPerAreaMetric);

        string averageRevenuePerInvoiceValue = LocalizationService.FormatCurrency(AverageRevenuePerInvoiceMetric);

        string returnRateValue = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("DashboardPercentValueFormat"),
            ReturnRateMetric);

        string areaUsageRateValue = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("DashboardPercentValueFormat"),
            AreaUsageRateMetric);

        string newMembersPerMonthValue = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("DashboardSignedNumberValueFormat"),
            NewMembersPerMonthMetric);

        MetricRows =
        [
            new LabelValueRowModel(
                LocalizationService.GetString("QuickStatsAverageHoursPerAreaLabel"),
                averageHoursPerAreaValue),
            new LabelValueRowModel(
                LocalizationService.GetString("QuickStatsAverageRevenuePerInvoiceLabel"),
                averageRevenuePerInvoiceValue),
            new LabelValueRowModel(
                LocalizationService.GetString("QuickStatsReturnRateLabel"),
                returnRateValue),
            new LabelValueRowModel(
                LocalizationService.GetString("QuickStatsAreaUsageRateLabel"),
                areaUsageRateValue),
            new LabelValueRowModel(
                LocalizationService.GetString("QuickStatsNewMembersPerMonthLabel"),
                newMembersPerMonthValue,
                showDivider: false),
        ];
    }
}
