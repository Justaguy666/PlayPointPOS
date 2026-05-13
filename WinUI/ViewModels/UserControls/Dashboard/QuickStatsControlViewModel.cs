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
    private double _averageHoursPerAreaMetric;
    private decimal _averageRevenuePerInvoiceMetric;
    private double _returnRateMetric;
    private double _areaUsageRateMetric;
    private int _newMembersPerMonthMetric;

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
            _averageHoursPerAreaMetric);

        string averageRevenuePerInvoiceValue = LocalizationService.FormatCurrency(_averageRevenuePerInvoiceMetric);

        string returnRateValue = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("DashboardPercentValueFormat"),
            _returnRateMetric);

        string areaUsageRateValue = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("DashboardPercentValueFormat"),
            _areaUsageRateMetric);

        string newMembersPerMonthValue = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("DashboardSignedNumberValueFormat"),
            _newMembersPerMonthMetric);

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

    public void ApplyMetrics(
        double averageHoursPerArea,
        decimal averageRevenuePerInvoice,
        double returnRate,
        double areaUsageRate,
        int newMembersPerMonth)
    {
        _averageHoursPerAreaMetric = averageHoursPerArea;
        _averageRevenuePerInvoiceMetric = averageRevenuePerInvoice;
        _returnRateMetric = returnRate;
        _areaUsageRateMetric = areaUsageRate;
        _newMembersPerMonthMetric = newMembersPerMonth;
        RefreshLocalizedText();
    }
}
