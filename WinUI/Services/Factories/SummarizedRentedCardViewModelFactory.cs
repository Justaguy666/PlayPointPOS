using Application.Services;
using System;
using WinUI.UIModels.AreaManagement;
using WinUI.ViewModels.AreaManagement.SummarizedAreaCards;

namespace WinUI.Services.Factories;

public sealed class SummarizedRentedCardViewModelFactory
{
    private readonly ILocalizationService _localizationService;

    public SummarizedRentedCardViewModelFactory(ILocalizationService localizationService)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    }

    public SummarizedRentedCardViewModel Create(AreaModel model)
    {
        return new SummarizedRentedCardViewModel(_localizationService, model);
    }
}
