using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Application.Services;
using Application.Services.Transactions;
using Application.UseCases.Analytics.Contracts.Enums;
using ClosedXML.Excel;
using CommunityToolkit.Mvvm.Input;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using Windows.Storage.Pickers;
using WinUI.Resources;
using WinUI.UIModels;
using WinUI.UIModels.Dashboard;
using WinUI.UIModels.Enums;
using WinUI.ViewModels;
using WinRT.Interop;

namespace WinUI.ViewModels.UserControls.Dashboard;

public partial class RevenueChartControlViewModel : LocalizedViewModelBase
{
    private const double MinBarHeight = 150.0;
    private const double MaxBarHeight = 240.0;

    private readonly INotificationService _notificationService;
    private readonly Dictionary<(ChartMetricType Metric, ChartRangeType Range), ChartDataset> _datasets;
    private readonly IReadOnlyList<Transaction> _transactions;
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
    private string _subtitle = string.Empty;
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

    public RevenueChartControlViewModel(
        ILocalizationService localizationService,
        INotificationService notificationService,
        ITransactionCatalogService transactionCatalogService)
        : base(localizationService)
    {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        ArgumentNullException.ThrowIfNull(transactionCatalogService);
        _transactions = transactionCatalogService.GetTransactions();
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
        ExportCommand = new AsyncRelayCommand(ExportAsync);
        RefreshLocalizedText();
    }

    public IRelayCommand<ChartMetricType> SelectMetricCommand { get; }

    public IRelayCommand<ChartRangeType> SelectTimeRangeCommand { get; }

    public IAsyncRelayCommand ExportCommand { get; }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Subtitle
    {
        get => _subtitle;
        set => SetProperty(ref _subtitle, value);
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
        Title = GetLocalizedText("RevenueChartTitle");
        RebuildViewState();
    }

    private void RebuildViewState()
    {
        Subtitle = GetRangeSubtitle(_selectedRange);
        Tabs = new ObservableCollection<ChartTabItemModel>(CreateMetricTabs());
        TimeRanges = new ObservableCollection<TimeRangeItemModel>(CreateTimeRanges());
        Bars = new ObservableCollection<ChartBarItemModel>(CreateBars());

        double contentWidth = Bars.Count * 104.0 + Math.Max(Bars.Count - 1, 0) * 16.0;
        ChartContentWidth = Math.Max(contentWidth, 360.0);
    }

    private async Task ExportAsync()
    {
        try
        {
            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = $"DashboardReport_{DateTime.Now:yyyyMMdd_HHmmss}",
            };
            picker.FileTypeChoices.Add("Excel Workbook (*.xlsx)", [".xlsx"]);

            var window = App.Host?.Services.GetService<MainWindow>();
            if (window is null)
            {
                await _notificationService.SendAsync(
                    Title,
                    "Cannot initialize export window handle.",
                    NotificationType.Error);
                return;
            }

            nint hwnd = WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(picker, hwnd);
            var file = await picker.PickSaveFileAsync();
            if (file is null)
            {
                return;
            }

            using var workbook = new XLWorkbook();
            BuildOverviewSheet(workbook);
            BuildProductQuantitySheet(workbook);
            BuildRevenueProfitSheet(workbook);

            await using Stream stream = await file.OpenStreamForWriteAsync();
            stream.SetLength(0);
            workbook.SaveAs(stream);
            await stream.FlushAsync();

            await _notificationService.SendAsync(
                Title,
                $"Exported report: {file.Path}",
                NotificationType.Success);
        }
        catch (Exception ex)
        {
            await _notificationService.SendAsync(
                Title,
                $"Export failed: {ex.Message}",
                NotificationType.Error);
        }
    }

    private IReadOnlyList<ChartTabItemModel> CreateMetricTabs()
    {
        return
        [
            CreateMetricTab(ChartMetricType.Revenue, GetLocalizedText("RevenueChartRevenueLabel"), 124),
            CreateMetricTab(ChartMetricType.Customer, GetLocalizedText("RevenueChartCustomerLabel"), 132),
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
            CreateTimeRange(ChartRangeType.Today, GetLocalizedText("RevenueChartTodayLabel"), 108),
            CreateTimeRange(ChartRangeType.ThisWeek, GetLocalizedText("RevenueChartWeekLabel"), 116),
            CreateTimeRange(ChartRangeType.ThisMonth, GetLocalizedText("RevenueChartMonthLabel"), 124),
            CreateTimeRange(ChartRangeType.ThisQuarter, GetLocalizedText("RevenueChartQuarterLabel"), 132),
            CreateTimeRange(ChartRangeType.ThisYear, GetLocalizedText("RevenueChartYearLabel"), 118),
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

    private string GetRangeSubtitle(ChartRangeType range)
    {
        return range switch
        {
            ChartRangeType.Today => GetLocalizedText("RevenueChartLast24HoursSubtitle"),
            ChartRangeType.ThisWeek => GetLocalizedText("RevenueChartLastWeekSubtitle"),
            ChartRangeType.ThisMonth => GetLocalizedText("RevenueChartLast30DaysSubtitle"),
            ChartRangeType.ThisQuarter => GetLocalizedText("RevenueChartLast3MonthsSubtitle"),
            ChartRangeType.ThisYear => GetLocalizedText("RevenueChartLast12MonthsSubtitle"),
            _ => string.Empty,
        };
    }

    private string GetMetricLabel(ChartMetricType metricType)
    {
        return metricType switch
        {
            ChartMetricType.Customer => GetLocalizedText("RevenueChartCustomerLabel"),
            _ => GetLocalizedText("RevenueChartRevenueLabel"),
        };
    }

    private string FormatMetricValue(ChartMetricType metricType, double value)
    {
        return metricType switch
        {
            ChartMetricType.Customer => string.Format(
                LocalizationService.Culture,
                GetLocalizedText("RevenueChartCustomerValueFormat"),
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
        DateTime now = DateTime.Now;
        DateTime weekStart = StartOfWeek(now.Date, DayOfWeek.Monday);
        DateTime monthStart = new(now.Year, now.Month, 1);
        int quarterStartMonth = ((now.Month - 1) / 3) * 3 + 1;
        DateTime quarterStart = new(now.Year, quarterStartMonth, 1);
        DateTime yearStart = new(now.Year, 1, 1);
        List<Transaction> localizedTransactions = _transactions
            .Select(t => new Transaction
            {
                CreatedAt = NormalizeToLocal(t.CreatedAt),
                TotalAmount = t.TotalAmount,
                CustomerName = t.CustomerName,
                MemberId = t.MemberId,
            })
            .ToList();

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
            GetLocalizedText("RevenueChartDayMonShort"),
            GetLocalizedText("RevenueChartDayTueShort"),
            GetLocalizedText("RevenueChartDayWedShort"),
            GetLocalizedText("RevenueChartDayThuShort"),
            GetLocalizedText("RevenueChartDayFriShort"),
            GetLocalizedText("RevenueChartDaySatShort"),
            GetLocalizedText("RevenueChartDaySunShort"),
        ];
        string[] monthLabels =
        [
            GetLocalizedText("RevenueChartMonthWeek1"),
            GetLocalizedText("RevenueChartMonthWeek2"),
            GetLocalizedText("RevenueChartMonthWeek3"),
            GetLocalizedText("RevenueChartMonthWeek4"),
        ];
        string[] quarterLabels =
        [
            GetLocalizedText("RevenueChartQuarterMonth1"),
            GetLocalizedText("RevenueChartQuarterMonth2"),
            GetLocalizedText("RevenueChartQuarterMonth3"),
        ];
        string[] yearLabels =
        [
            GetLocalizedText("RevenueChartYearJanShort"),
            GetLocalizedText("RevenueChartYearMarShort"),
            GetLocalizedText("RevenueChartYearMayShort"),
            GetLocalizedText("RevenueChartYearJulShort"),
            GetLocalizedText("RevenueChartYearSepShort"),
            GetLocalizedText("RevenueChartYearNovShort"),
        ];

        double[] revenueToday = todayLabels
            .Select((_, index) =>
            {
                int hour = 8 + index * 2;
                DateTime from = now.Date.AddHours(hour);
                DateTime to = from.AddHours(2);
                decimal amount = localizedTransactions
                    .Where(t => t.CreatedAt >= from && t.CreatedAt < to)
                    .Sum(t => t.TotalAmount);
                return ToMillion(amount);
            })
            .ToArray();
        double[] customerToday = todayLabels
            .Select((_, index) =>
            {
                int hour = 8 + index * 2;
                DateTime from = now.Date.AddHours(hour);
                DateTime to = from.AddHours(2);
                return (double)localizedTransactions
                    .Where(t => t.CreatedAt >= from && t.CreatedAt < to)
                    .Select(BuildCustomerIdentity)
                    .Where(identity => !string.IsNullOrWhiteSpace(identity))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();
            })
            .ToArray();

        double[] revenueWeek = Enumerable.Range(0, 7)
            .Select(offset =>
            {
                DateTime from = weekStart.AddDays(offset);
                DateTime to = from.AddDays(1);
                decimal amount = localizedTransactions
                    .Where(t => t.CreatedAt >= from && t.CreatedAt < to)
                    .Sum(t => t.TotalAmount);
                return ToMillion(amount);
            })
            .ToArray();
        double[] customerWeek = Enumerable.Range(0, 7)
            .Select(offset =>
            {
                DateTime from = weekStart.AddDays(offset);
                DateTime to = from.AddDays(1);
                return (double)localizedTransactions
                    .Where(t => t.CreatedAt >= from && t.CreatedAt < to)
                    .Select(BuildCustomerIdentity)
                    .Where(identity => !string.IsNullOrWhiteSpace(identity))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();
            })
            .ToArray();

        int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
        (int start, int end)[] monthRanges = [(1, 7), (8, 14), (15, 21), (22, daysInMonth)];
        double[] revenueMonth = monthRanges
            .Select(range =>
            {
                DateTime from = monthStart.AddDays(range.start - 1);
                DateTime to = monthStart.AddDays(range.end);
                decimal amount = localizedTransactions
                    .Where(t => t.CreatedAt >= from && t.CreatedAt < to)
                    .Sum(t => t.TotalAmount);
                return ToMillion(amount);
            })
            .ToArray();
        double[] customerMonth = monthRanges
            .Select(range =>
            {
                DateTime from = monthStart.AddDays(range.start - 1);
                DateTime to = monthStart.AddDays(range.end);
                return (double)localizedTransactions
                    .Where(t => t.CreatedAt >= from && t.CreatedAt < to)
                    .Select(BuildCustomerIdentity)
                    .Where(identity => !string.IsNullOrWhiteSpace(identity))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();
            })
            .ToArray();

        double[] revenueQuarter = Enumerable.Range(0, 3)
            .Select(offset =>
            {
                DateTime from = quarterStart.AddMonths(offset);
                DateTime to = from.AddMonths(1);
                decimal amount = localizedTransactions
                    .Where(t => t.CreatedAt >= from && t.CreatedAt < to)
                    .Sum(t => t.TotalAmount);
                return ToMillion(amount);
            })
            .ToArray();
        double[] customerQuarter = Enumerable.Range(0, 3)
            .Select(offset =>
            {
                DateTime from = quarterStart.AddMonths(offset);
                DateTime to = from.AddMonths(1);
                return (double)localizedTransactions
                    .Where(t => t.CreatedAt >= from && t.CreatedAt < to)
                    .Select(BuildCustomerIdentity)
                    .Where(identity => !string.IsNullOrWhiteSpace(identity))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();
            })
            .ToArray();

        int[] months = [1, 3, 5, 7, 9, 11];
        double[] revenueYear = months
            .Select(month =>
            {
                DateTime from = yearStart.AddMonths(month - 1);
                DateTime to = from.AddMonths(1);
                decimal amount = localizedTransactions
                    .Where(t => t.CreatedAt >= from && t.CreatedAt < to)
                    .Sum(t => t.TotalAmount);
                return ToMillion(amount);
            })
            .ToArray();
        double[] customerYear = months
            .Select(month =>
            {
                DateTime from = yearStart.AddMonths(month - 1);
                DateTime to = from.AddMonths(1);
                return (double)localizedTransactions
                    .Where(t => t.CreatedAt >= from && t.CreatedAt < to)
                    .Select(BuildCustomerIdentity)
                    .Where(identity => !string.IsNullOrWhiteSpace(identity))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();
            })
            .ToArray();

        return new Dictionary<(ChartMetricType Metric, ChartRangeType Range), ChartDataset>
        {
            [(ChartMetricType.Revenue, ChartRangeType.Today)] = new(todayLabels, revenueToday),
            [(ChartMetricType.Revenue, ChartRangeType.ThisWeek)] = new(weekLabels, revenueWeek),
            [(ChartMetricType.Revenue, ChartRangeType.ThisMonth)] = new(monthLabels, revenueMonth),
            [(ChartMetricType.Revenue, ChartRangeType.ThisQuarter)] = new(quarterLabels, revenueQuarter),
            [(ChartMetricType.Revenue, ChartRangeType.ThisYear)] = new(yearLabels, revenueYear),

            [(ChartMetricType.Customer, ChartRangeType.Today)] = new(todayLabels, customerToday),
            [(ChartMetricType.Customer, ChartRangeType.ThisWeek)] = new(weekLabels, customerWeek),
            [(ChartMetricType.Customer, ChartRangeType.ThisMonth)] = new(monthLabels, customerMonth),
            [(ChartMetricType.Customer, ChartRangeType.ThisQuarter)] = new(quarterLabels, customerQuarter),
            [(ChartMetricType.Customer, ChartRangeType.ThisYear)] = new(yearLabels, customerYear),
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

    private void BuildOverviewSheet(XLWorkbook workbook)
    {
        var ws = workbook.Worksheets.Add("TongQuan");
        ws.Cell(1, 1).Value = "Bao cao thong ke dashboard";
        ws.Cell(2, 1).Value = "Muc tieu:";
        ws.Cell(3, 1).Value = "- Theo doi tinh trang san pham va don hang";
        ws.Cell(4, 1).Value = "- Theo doi xu huong kinh doanh";
        ws.Cell(6, 1).Value = "Noi dung xuat:";
        ws.Cell(7, 1).Value = "1) San pham va so luong ban theo ngay/tuan/thang/nam";
        ws.Cell(8, 1).Value = "2) Doanh thu va loi nhuan uoc tinh theo ngay/tuan/thang/nam";
        ws.Cell(10, 1).Value = $"GeneratedAt: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        ws.Column(1).Width = 90;
    }

    private void BuildProductQuantitySheet(XLWorkbook workbook)
    {
        var ws = workbook.Worksheets.Add("SanPham_SoLuong");
        ws.Cell(1, 1).Value = "KyBaoCao";
        ws.Cell(1, 2).Value = "MocThoiGian";
        ws.Cell(1, 3).Value = "SanPham";
        ws.Cell(1, 4).Value = "SoLuongBan";

        int row = 2;
        foreach (var period in GetPeriodKinds())
        {
            var rows = _transactions
                .SelectMany(t => t.Lines.Select(line => new { Transaction = t, Line = line }))
                .Where(x => x.Line.Type == Domain.Enums.TransactionLineType.ProductSale)
                .Select(x => new
                {
                    PeriodKind = period.Name,
                    PeriodKey = period.KeySelector(NormalizeToLocal(x.Transaction.CreatedAt)),
                    ProductName = string.IsNullOrWhiteSpace(x.Line.ItemName) ? "Unknown" : x.Line.ItemName.Trim(),
                    Quantity = x.Line.Quantity,
                    GroupKey = $"{period.Name}|{period.KeySelector(NormalizeToLocal(x.Transaction.CreatedAt))}|{(string.IsNullOrWhiteSpace(x.Line.ItemName) ? "Unknown" : x.Line.ItemName.Trim())}",
                })
                .GroupBy(x => x.GroupKey, StringComparer.OrdinalIgnoreCase)
                .Select(g => new
                {
                    PeriodKind = g.First().PeriodKind,
                    PeriodKey = g.First().PeriodKey,
                    ProductName = g.First().ProductName,
                    Quantity = g.Sum(x => x.Quantity),
                })
                .OrderBy(x => x.PeriodKind, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.PeriodKey, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.ProductName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var item in rows)
            {
                ws.Cell(row, 1).Value = item.PeriodKind;
                ws.Cell(row, 2).Value = item.PeriodKey;
                ws.Cell(row, 3).Value = item.ProductName;
                ws.Cell(row, 4).Value = item.Quantity;
                row++;
            }
        }

        ws.Columns().AdjustToContents();
    }

    private void BuildRevenueProfitSheet(XLWorkbook workbook)
    {
        var ws = workbook.Worksheets.Add("DoanhThu_LoiNhuan");
        ws.Cell(1, 1).Value = "KyBaoCao";
        ws.Cell(1, 2).Value = "MocThoiGian";
        ws.Cell(1, 3).Value = "DoanhThuGop";
        ws.Cell(1, 4).Value = "GiamGiaVaDieuChinh";
        ws.Cell(1, 5).Value = "DoanhThuThuan";
        ws.Cell(1, 6).Value = "LoiNhuanUocTinh";

        int row = 2;
        foreach (var period in GetPeriodKinds())
        {
            var rows = _transactions
                .Select(t => new
                {
                    PeriodKind = period.Name,
                    PeriodKey = period.KeySelector(NormalizeToLocal(t.CreatedAt)),
                    GrossRevenue = t.SubtotalAmount,
                    NetRevenue = t.TotalAmount,
                    Adjustment = t.DiscountAmount + t.DepositRefund,
                    GroupKey = $"{period.Name}|{period.KeySelector(NormalizeToLocal(t.CreatedAt))}",
                })
                .GroupBy(x => x.GroupKey, StringComparer.OrdinalIgnoreCase)
                .Select(g => new
                {
                    PeriodKind = g.First().PeriodKind,
                    PeriodKey = g.First().PeriodKey,
                    GrossRevenue = g.Sum(x => x.GrossRevenue),
                    Adjustment = g.Sum(x => x.Adjustment),
                    NetRevenue = g.Sum(x => x.NetRevenue),
                })
                .OrderBy(x => x.PeriodKind, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.PeriodKey, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var item in rows)
            {
                ws.Cell(row, 1).Value = item.PeriodKind;
                ws.Cell(row, 2).Value = item.PeriodKey;
                ws.Cell(row, 3).Value = item.GrossRevenue;
                ws.Cell(row, 4).Value = item.Adjustment;
                ws.Cell(row, 5).Value = item.NetRevenue;
                ws.Cell(row, 6).Value = item.NetRevenue;
                row++;
            }
        }

        ws.Columns(3, 6).Style.NumberFormat.Format = "#,##0.00";
        ws.Columns().AdjustToContents();
    }

    private static IReadOnlyList<(string Name, Func<DateTime, string> KeySelector)> GetPeriodKinds()
    {
        return
        [
            ("Ngay", date => date.ToString("yyyy-MM-dd")),
            ("Tuan", date =>
            {
                DateTime weekStart = StartOfWeek(date.Date, DayOfWeek.Monday);
                DateTime weekEnd = weekStart.AddDays(6);
                return $"{weekStart:yyyy-MM-dd}..{weekEnd:yyyy-MM-dd}";
            }),
            ("Thang", date => date.ToString("yyyy-MM")),
            ("Nam", date => date.ToString("yyyy")),
        ];
    }

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
        return string.IsNullOrWhiteSpace(customer) ? string.Empty : $"customer:{customer}";
    }

    private static double ToMillion(decimal amount)
        => (double)(amount / 1_000_000m);

    private static DateTime StartOfWeek(DateTime date, DayOfWeek startOfWeek)
    {
        int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
        return date.AddDays(-diff).Date;
    }

    private sealed record ChartDataset(IReadOnlyList<string> Labels, IReadOnlyList<double> Values);
}
