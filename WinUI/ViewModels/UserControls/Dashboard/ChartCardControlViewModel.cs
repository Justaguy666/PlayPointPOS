using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Application.Services;
using Application.UseCases.Analytics.Contracts.Enums;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using WinUI.Resources;
using WinUI.UIModels;
using WinUI.UIModels.Dashboard;
using WinUI.UIModels.Enums;
using WinUI.ViewModels;

namespace WinUI.ViewModels.UserControls.Dashboard;

public partial class ChartCardControlViewModel : LocalizedViewModelBase
{
    private const double MinBarHeight = 150.0;
    private const double MaxBarHeight = 240.0;

    private readonly Dictionary<(ChartMetricType Metric, ChartRangeType Range), ChartDataset> _datasets;
    private readonly Brush _whiteBrush;
    private readonly Brush _orangeBrush;
    private readonly Brush _mutedTextBrush;
    private readonly Brush _darkTextBrush;
    private readonly Brush _lightBorderBrush;
    private readonly Brush _rangeBorderBrush;
    private readonly Brush _inactiveBarBrush;
    private readonly Brush _calloutBackgroundBrush;

    private ChartMetricType _selectedMetric = ChartMetricType.Revenue;
    private ChartRangeType _selectedRange = ChartRangeType.ThisWeek;

    private string _title = string.Empty;
    private string _description = string.Empty;
    private IconState _iconState = new()
    {
        Kind = IconKind.Chart,
        AlwaysFilled = true,
        Size = 24,
    };
    private IconState _exportIconState = new()
    {
        Kind = IconKind.Export,
        AlwaysFilled = true,
        Size = 24,
    };
    private ObservableCollection<ChartTabItemModel> _tabs = [];
    private ObservableCollection<TimeRangeItemModel> _timeRanges = [];
    private ObservableCollection<ChartBarItemModel> _bars = [];
    private double _chartContentWidth;

    public ChartCardControlViewModel(ILocalizationService localizationService)
        : base(localizationService)
    {
        _whiteBrush = AppResourceLookup.GetBrush("WhiteBrush", Colors.White);
        _orangeBrush = AppResourceLookup.GetBrush("PrimaryOrangeBrush", AppColors.PrimaryOrange);
        _mutedTextBrush = AppResourceLookup.GetBrush("DashboardMutedTextBrush", ColorHelper.FromArgb(255, 113, 128, 150));
        _darkTextBrush = AppResourceLookup.GetBrush("BlackBrush", AppColors.Black);
        _lightBorderBrush = AppResourceLookup.GetBrush("LightGrayBrush", AppColors.LightGray);
        _rangeBorderBrush = AppResourceLookup.GetBrush("DashboardChartRangeBorderBrush", ColorHelper.FromArgb(255, 255, 220, 206));
        _inactiveBarBrush = AppResourceLookup.GetBrush("DashboardChartInactiveBarBrush", ColorHelper.FromArgb(255, 255, 162, 130));
        _calloutBackgroundBrush = AppResourceLookup.GetBrush("DashboardChartCalloutBackgroundBrush", ColorHelper.FromArgb(255, 249, 232, 224));

        _datasets = CreateDatasets();

        SelectMetricCommand = new RelayCommand<ChartMetricType>(SelectMetric);
        SelectTimeRangeCommand = new RelayCommand<ChartRangeType>(SelectTimeRange);
        ExportCommand = new RelayCommand(static () => { });
        RefreshLocalizedText();
    }

    public IRelayCommand<ChartMetricType> SelectMetricCommand { get; }

    public IRelayCommand<ChartRangeType> SelectTimeRangeCommand { get; }

    public IRelayCommand ExportCommand { get; }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public IconState IconState
    {
        get => _iconState;
        set => SetProperty(ref _iconState, value);
    }

    public IconState ExportIconState
    {
        get => _exportIconState;
        set => SetProperty(ref _exportIconState, value);
    }

    public ObservableCollection<ChartTabItemModel> Tabs
    {
        get => _tabs;
        set => SetProperty(ref _tabs, value);
    }

    public ObservableCollection<TimeRangeItemModel> TimeRanges
    {
        get => _timeRanges;
        set => SetProperty(ref _timeRanges, value);
    }

    public ObservableCollection<ChartBarItemModel> Bars
    {
        get => _bars;
        set => SetProperty(ref _bars, value);
    }

    public double ChartContentWidth
    {
        get => _chartContentWidth;
        set => SetProperty(ref _chartContentWidth, value);
    }

    private void SelectMetric(ChartMetricType metric)
    {
        if (_selectedMetric == metric)
            return;

        _selectedMetric = metric;
        RebuildViewState();
    }

    private void SelectTimeRange(ChartRangeType range)
    {
        if (_selectedRange == range)
            return;

        _selectedRange = range;
        RebuildViewState();
    }

    protected override void RefreshLocalizedText()
    {
        Title = GetLocalizedText("RevenueChartCardTitle");
        RebuildViewState();
    }

    private void RebuildViewState()
    {
        Description = GetRangeDescription(_selectedRange);
        Tabs = new ObservableCollection<ChartTabItemModel>(CreateMetricTabs());
        TimeRanges = new ObservableCollection<TimeRangeItemModel>(CreateTimeRanges());
        Bars = new ObservableCollection<ChartBarItemModel>(CreateBars());

        double contentWidth = Bars.Count * 104.0 + Math.Max(Bars.Count - 1, 0) * 16.0;
        ChartContentWidth = Math.Max(contentWidth, 360.0);
    }

    private IReadOnlyList<ChartTabItemModel> CreateMetricTabs()
    {
        return
        [
            CreateMetricTab(ChartMetricType.Revenue, GetLocalizedText("RevenueChartCardRevenueLabel"), 124),
            CreateMetricTab(ChartMetricType.Customer, GetLocalizedText("RevenueChartCardCustomerLabel"), 132),
        ];
    }

    private ChartTabItemModel CreateMetricTab(ChartMetricType metricType, string title, double width)
    {
        bool isSelected = _selectedMetric == metricType;
        return new ChartTabItemModel
        {
            Title = title,
            MetricType = metricType,
            Width = width,
            IsSelected = isSelected,
            Background = isSelected ? _orangeBrush : _whiteBrush,
            BorderBrush = isSelected ? _orangeBrush : _lightBorderBrush,
            Foreground = isSelected ? _whiteBrush : _mutedTextBrush,
        };
    }

    private IReadOnlyList<TimeRangeItemModel> CreateTimeRanges()
    {
        return
        [
            CreateTimeRange(ChartRangeType.Today, GetLocalizedText("RevenueChartCardTodayLabel"), 108),
            CreateTimeRange(ChartRangeType.ThisWeek, GetLocalizedText("RevenueChartCardWeekLabel"), 116),
            CreateTimeRange(ChartRangeType.ThisMonth, GetLocalizedText("RevenueChartCardMonthLabel"), 124),
            CreateTimeRange(ChartRangeType.ThisQuater, GetLocalizedText("RevenueChartCardQuarterLabel"), 132),
            CreateTimeRange(ChartRangeType.ThisYear, GetLocalizedText("RevenueChartCardYearLabel"), 118),
        ];
    }

    private TimeRangeItemModel CreateTimeRange(ChartRangeType rangeType, string title, double width)
    {
        bool isSelected = _selectedRange == rangeType;
        return new TimeRangeItemModel
        {
            Title = title,
            RangeType = rangeType,
            Width = width,
            IsSelected = isSelected,
            Background = isSelected ? _orangeBrush : _whiteBrush,
            BorderBrush = isSelected ? _orangeBrush : _rangeBorderBrush,
            Foreground = isSelected ? _whiteBrush : _orangeBrush,
        };
    }

    private IReadOnlyList<ChartBarItemModel> CreateBars()
    {
        var dataset = _datasets[(_selectedMetric, _selectedRange)];
        double maxValue = dataset.Values.Max();
        double minValue = dataset.Values.Min();

        return dataset.Values
            .Select((value, index) =>
            {
                double ratio = maxValue <= minValue
                    ? 1.0
                    : (value - minValue) / (maxValue - minValue);

                return new ChartBarItemModel
                {
                    Label = dataset.Labels[index],
                    Value = value,
                    DisplayValue = FormatMetricValue(_selectedMetric, value),
                    PopupValue = FormatPopupValue(_selectedMetric, value),
                    NormalizedValue = MinBarHeight + (ratio * (MaxBarHeight - MinBarHeight)),
                    IsHovered = false,
                    IsMax = Math.Abs(value - maxValue) < 0.0001,
                    DefaultFill = _inactiveBarBrush,
                    HoverFill = _orangeBrush,
                    Fill = _inactiveBarBrush,
                    LabelForeground = _mutedTextBrush,
                    CalloutBackground = _calloutBackgroundBrush,
                    CalloutForeground = _darkTextBrush,
                };
            })
            .ToArray();
    }

    private string GetRangeDescription(ChartRangeType range)
    {
        return range switch
        {
            ChartRangeType.Today => GetLocalizedText("RevenueChartCardSubtitleLast24Hours"),
            ChartRangeType.ThisWeek => GetLocalizedText("RevenueChartCardSubtitleLastWeek"),
            ChartRangeType.ThisMonth => GetLocalizedText("RevenueChartCardSubtitleLast30Days"),
            ChartRangeType.ThisQuater => GetLocalizedText("RevenueChartCardSubtitleLast3Months"),
            ChartRangeType.ThisYear => GetLocalizedText("RevenueChartCardSubtitleLast12Months"),
            _ => string.Empty,
        };
    }

    private string FormatMetricValue(ChartMetricType metricType, double value)
    {
        return metricType switch
        {
            ChartMetricType.Customer => string.Format(
                LocalizationService.Culture,
                GetLocalizedText("RevenueChartCardCustomerValueFormat"),
                value),
            _ => LocalizationService.FormatCompactCurrencyFromMillions(value),
        };
    }

    private string FormatPopupValue(ChartMetricType metricType, double value)
    {
        return metricType switch
        {
            ChartMetricType.Customer => Math.Abs(value % 1) < 0.0001
                ? value.ToString("0", LocalizationService.Culture)
                : value.ToString("0.#", LocalizationService.Culture),
            _ => LocalizationService.FormatCompactCurrencyFromMillions(value),
        };
    }

    private Dictionary<(ChartMetricType Metric, ChartRangeType Range), ChartDataset> CreateDatasets()
    {
        string[] todayLabels =
        [
            FormatHourLabel(8),
            FormatHourLabel(10),
            FormatHourLabel(12),
            FormatHourLabel(14),
            FormatHourLabel(16),
            FormatHourLabel(18),
            FormatHourLabel(20),
        ];
        string[] weekLabels =
        [
            GetLocalizedText("RevenueChartCardDayMonShort"),
            GetLocalizedText("RevenueChartCardDayTueShort"),
            GetLocalizedText("RevenueChartCardDayWedShort"),
            GetLocalizedText("RevenueChartCardDayThuShort"),
            GetLocalizedText("RevenueChartCardDayFriShort"),
            GetLocalizedText("RevenueChartCardDaySatShort"),
            GetLocalizedText("RevenueChartCardDaySunShort"),
        ];
        string[] monthLabels =
        [
            GetLocalizedText("RevenueChartCardMonthWeek1"),
            GetLocalizedText("RevenueChartCardMonthWeek2"),
            GetLocalizedText("RevenueChartCardMonthWeek3"),
            GetLocalizedText("RevenueChartCardMonthWeek4"),
        ];
        string[] quarterLabels =
        [
            GetLocalizedText("RevenueChartCardQuarterMonth1"),
            GetLocalizedText("RevenueChartCardQuarterMonth2"),
            GetLocalizedText("RevenueChartCardQuarterMonth3"),
        ];
        string[] yearLabels =
        [
            GetLocalizedText("RevenueChartCardYearJanShort"),
            GetLocalizedText("RevenueChartCardYearMarShort"),
            GetLocalizedText("RevenueChartCardYearMayShort"),
            GetLocalizedText("RevenueChartCardYearJulShort"),
            GetLocalizedText("RevenueChartCardYearSepShort"),
            GetLocalizedText("RevenueChartCardYearNovShort"),
        ];

        return new Dictionary<(ChartMetricType Metric, ChartRangeType Range), ChartDataset>
        {
            [(ChartMetricType.Revenue, ChartRangeType.Today)] = new(todayLabels, [0.6, 0.9, 1.2, 1.05, 1.36, 1.52, 1.28]),
            [(ChartMetricType.Revenue, ChartRangeType.ThisWeek)] = new(weekLabels, [3.0, 3.28, 2.76, 3.39, 3.15, 3.48, 3.55]),
            [(ChartMetricType.Revenue, ChartRangeType.ThisMonth)] = new(monthLabels, [12.4, 14.1, 13.2, 15.6]),
            [(ChartMetricType.Revenue, ChartRangeType.ThisQuater)] = new(quarterLabels, [52, 58, 61]),
            [(ChartMetricType.Revenue, ChartRangeType.ThisYear)] = new(yearLabels, [110, 126, 118, 134, 140, 151]),

            [(ChartMetricType.Customer, ChartRangeType.Today)] = new(todayLabels, [18, 22, 26, 24, 31, 35, 29]),
            [(ChartMetricType.Customer, ChartRangeType.ThisWeek)] = new(weekLabels, [82, 96, 75, 104, 92, 110, 116]),
            [(ChartMetricType.Customer, ChartRangeType.ThisMonth)] = new(monthLabels, [362, 388, 405, 432]),
            [(ChartMetricType.Customer, ChartRangeType.ThisQuater)] = new(quarterLabels, [1180, 1265, 1340]),
            [(ChartMetricType.Customer, ChartRangeType.ThisYear)] = new(yearLabels, [2420, 2570, 2660, 2810, 2975, 3140]),
        };
    }

    private string GetLocalizedText(string key)
    {
        string value = LocalizationService.GetString(key);
        return string.IsNullOrWhiteSpace(value) || value.StartsWith("[", StringComparison.Ordinal)
            ? key
            : value;
    }

    private string FormatHourLabel(int hour)
        => LocalizationService.FormatHourLabel(hour);

    private sealed record ChartDataset(IReadOnlyList<string> Labels, IReadOnlyList<double> Values);
}
