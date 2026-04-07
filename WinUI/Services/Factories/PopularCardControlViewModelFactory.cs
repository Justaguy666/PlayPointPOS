using System;
using System.Collections.Generic;
using Application.Navigation;
using Application.Services;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.UserControls.Dashboard;

namespace WinUI.Services.Factories;

public sealed class PopularCardControlViewModelFactory
{
    private readonly ILocalizationService _localizationService;
    private readonly INavigationService _navigationService;

    public PopularCardControlViewModelFactory(
        ILocalizationService localizationService,
        INavigationService navigationService)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }

    public PopularCardControlViewModel Create(
        string resourcePrefix,
        IconKind titleIconKind,
        string activityFormatResourceKey,
        IReadOnlyList<PopularCardItemData> sourceItems)
    {
        return new PopularCardControlViewModel(
            _localizationService,
            _navigationService,
            resourcePrefix,
            titleIconKind,
            activityFormatResourceKey,
            sourceItems);
    }
}
