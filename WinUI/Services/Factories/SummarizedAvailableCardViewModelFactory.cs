using System;
using Application.Services;
using WinUI.UIModels.AreaManagement;
using WinUI.ViewModels.AreaManagement.SummarizedAreaCards;

namespace WinUI.Services.Factories;

public sealed class SummarizedAvailableCardViewModelFactory
{
    private readonly ILocalizationService _localizationService;

    public SummarizedAvailableCardViewModelFactory(ILocalizationService localizationService)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    }

    public SummarizedAvailableCardViewModel Create(AreaModel model)
    {
        return new SummarizedAvailableCardViewModel(_localizationService, model);
    }
}
