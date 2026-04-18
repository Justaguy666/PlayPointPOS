using Application.Services;
using Application.Services.Areas;
using System;
using WinUI.UIModels.Management;
using WinUI.ViewModels.AreaManagement.SummarizedAreaCards;

namespace WinUI.Services.Factories;

public sealed class SummarizedRentedCardViewModelFactory
{
    private readonly ILocalizationService _localizationService;
    private readonly IAreaSessionService _areaSessionService;

    public SummarizedRentedCardViewModelFactory(
        ILocalizationService localizationService,
        IAreaSessionService areaSessionService)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _areaSessionService = areaSessionService ?? throw new ArgumentNullException(nameof(areaSessionService));
    }

    public SummarizedRentedCardViewModel Create(AreaModel model)
    {
        return new SummarizedRentedCardViewModel(_localizationService, _areaSessionService, model);
    }
}
