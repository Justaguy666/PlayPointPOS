using System;
using Application.Services;
using WinUI.UIModels.AreaManagement.SummarizedAreaCards;
using WinUI.ViewModels.AreaManagement.SummarizedAreaCards;

namespace WinUI.Services.Factories;

public sealed class SummarizedReservedCardViewModelFactory
{
    private readonly ILocalizationService _localizationService;

    public SummarizedReservedCardViewModelFactory(ILocalizationService localizationService)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    }

    public SummarizedReservedCardViewModel Create(SummarizedReservedCardModel model)
    {
        return new SummarizedReservedCardViewModel(_localizationService, model);
    }
}
