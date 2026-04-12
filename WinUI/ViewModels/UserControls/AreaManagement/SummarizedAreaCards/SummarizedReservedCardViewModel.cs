using System;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Domain.Enums;
using WinUI.UIModels.AreaManagement;

namespace WinUI.ViewModels.AreaManagement.SummarizedAreaCards;

public sealed partial class SummarizedReservedCardViewModel : LocalizedViewModelBase, ISummarizedAreaCardViewModel, IDisposable
{
    private bool _isDisposed;

    public string AreaName => Model.AreaName;

    public PlayAreaType PlayAreaType => Model.PlayAreaType;

    public PlayAreaStatus Status => Model.Status;

    public string CustomerName => Model.CustomerName;

    public string PhoneNumber => Model.PhoneNumber;

    public decimal HourlyPrice => Model.HourlyPrice;

    [ObservableProperty]
    public partial string CheckInTime { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Capacity { get; set; } = string.Empty;

    public SummarizedReservedCardViewModel(
        ILocalizationService localizationService,
        AreaModel model)
        : base(localizationService)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Model.PropertyChanged += HandleModelPropertyChanged;
        RefreshLocalizedText();
    }

    public AreaModel Model { get; }

    public DateTime CheckInDateTime => Model.CheckInDateTime ?? DateTime.MinValue;

    public int ReservationCapacity => Model.Capacity;

    protected override void RefreshLocalizedText()
    {
        CheckInTime = (Model.CheckInDateTime ?? DateTime.MinValue).ToString("HH:mm - dd/MM/yy", LocalizationService.Culture);

        Capacity = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("AreaManagementCapacityFormat"),
            Model.Capacity);
    }

    public new void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        Model.PropertyChanged -= HandleModelPropertyChanged;
        _isDisposed = true;
        base.Dispose();
    }

    void IDisposable.Dispose()
    {
        Dispose();
    }

    private void HandleModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_isDisposed)
        {
            return;
        }

        OnPropertyChanged(nameof(AreaName));
        OnPropertyChanged(nameof(PlayAreaType));
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(CustomerName));
        OnPropertyChanged(nameof(PhoneNumber));
        OnPropertyChanged(nameof(HourlyPrice));
        OnPropertyChanged(nameof(CheckInDateTime));
        OnPropertyChanged(nameof(ReservationCapacity));
        RefreshLocalizedText();
    }
}
