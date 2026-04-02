using System;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using WinUI.Resources;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels;

namespace WinUI.ViewModels.UserControls.Dashboard;

public sealed partial class StatCardControlViewModel : LocalizedViewModelBase
{
    private readonly string _resourcePrefix;
    private readonly Func<ILocalizationService, string> _valueTextFactory;

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ValueText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TrendText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ComparisonText { get; set; } = string.Empty;

    public Brush TrendBackground { get; }

    public Brush TrendForeground { get; }

    public IconState IconState { get; }

    public StatCardControlViewModel(
        ILocalizationService localizationService,
        string resourcePrefix,
        IconKind iconKind,
        bool usesPositiveTrendColors,
        Func<ILocalizationService, string> valueTextFactory)
        : base(localizationService)
    {
        _resourcePrefix = string.IsNullOrWhiteSpace(resourcePrefix)
            ? throw new ArgumentException("Resource prefix is required.", nameof(resourcePrefix))
            : resourcePrefix;
        _valueTextFactory = valueTextFactory ?? throw new ArgumentNullException(nameof(valueTextFactory));

        TrendBackground = AppResourceLookup.GetBrush(
            usesPositiveTrendColors
                ? "DashboardStatCardPositiveTrendBackgroundBrush"
                : "DashboardStatCardNegativeTrendBackgroundBrush",
            usesPositiveTrendColors
                ? AppColors.DashboardStatCardPositiveTrendBackground
                : AppColors.DashboardStatCardNegativeTrendBackground);

        TrendForeground = AppResourceLookup.GetBrush(
            usesPositiveTrendColors ? "DashboardTrendPositiveBrush" : "DashboardTrendNegativeBrush",
            usesPositiveTrendColors ? AppColors.DashboardTrendPositive : AppColors.DashboardTrendNegative);

        IconState = new IconState
        {
            Kind = iconKind,
            Size = 24,
            AlwaysFilled = true,
        };

        RefreshLocalizedText();
    }

    protected override void RefreshLocalizedText()
    {
        Title = LocalizationService.GetString($"{_resourcePrefix}Title");
        TrendText = LocalizationService.GetString($"{_resourcePrefix}TrendText");
        ComparisonText = LocalizationService.GetString($"{_resourcePrefix}ComparisonText");
        ValueText = _valueTextFactory(LocalizationService);
    }
}
