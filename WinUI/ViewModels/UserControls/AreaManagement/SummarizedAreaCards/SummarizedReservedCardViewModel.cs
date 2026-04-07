using System;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using WinUI.UIModels.AreaManagement.SummarizedAreaCards;

namespace WinUI.ViewModels.AreaManagement.SummarizedAreaCards;

public sealed partial class SummarizedReservedCardViewModel : LocalizedViewModelBase, ISummarizedAreaCardViewModel
{
    private readonly SummarizedReservedCardModel _model;

    [ObservableProperty]
    public partial string AreaName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CustomerName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CheckInTime { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Capacity { get; set; } = string.Empty;

    public SummarizedReservedCardViewModel(
        ILocalizationService localizationService,
        SummarizedReservedCardModel model)
        : base(localizationService)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        AreaName = _model.AreaName;
        CustomerName = _model.CustomerName;
        RefreshLocalizedText();
    }

    protected override void RefreshLocalizedText()
    {
        CheckInTime = _model.CheckInTime.ToString("HH:mm - dd/MM/yy", LocalizationService.Culture);

        Capacity = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("AreaManagementCapacityFormat"),
            _model.Capacity);
    }
}
