using System;
using Application.Services;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.UserControls.Dashboard;

namespace WinUI.Services;

public sealed class StatCardControlViewModelFactory
{
    private readonly ILocalizationService _localizationService;

    public StatCardControlViewModelFactory(ILocalizationService localizationService)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    }

    public StatCardControlViewModel Create(
        string resourcePrefix,
        IconKind iconKind,
        bool usesPositiveTrendColors,
        Func<ILocalizationService, string> valueTextFactory)
    {
        return new StatCardControlViewModel(
            _localizationService,
            resourcePrefix,
            iconKind,
            usesPositiveTrendColors,
            valueTextFactory);
    }
}
