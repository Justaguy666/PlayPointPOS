using System;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using WinUI.UIModels.AreaManagement.SummarizedAreaCards;

namespace WinUI.ViewModels.AreaManagement.SummarizedAreaCards;

public sealed partial class SummarizedAvailableCardViewModel : LocalizedViewModelBase, ISummarizedAreaCardViewModel
{
    private readonly SummarizedAvailableCardModel _model;

    [ObservableProperty]
    public partial string AreaName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AvailableStatusText { get; set; } = string.Empty;

    public SummarizedAvailableCardViewModel(
        ILocalizationService localizationService,
        SummarizedAvailableCardModel model)
        : base(localizationService)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        AreaName = _model.AreaName;
        RefreshLocalizedText();
    }

    protected override void RefreshLocalizedText()
    {
        AvailableStatusText = LocalizationService.GetString("AreaManagementAvailableStatusText");
    }
}
