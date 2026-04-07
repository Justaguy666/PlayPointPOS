using System;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using WinUI.UIModels.AreaManagement.SummarizedAreaCards;

namespace WinUI.ViewModels.AreaManagement.SummarizedAreaCards;

public sealed partial class SummarizedRentedCardViewModel : LocalizedViewModelBase, ISummarizedAreaCardViewModel, IDisposable
{
    private readonly SummarizedRentedCardModel _model;
    private readonly DispatcherQueueTimer? _timer;
    private bool _isDisposed;

    [ObservableProperty]
    public partial string AreaName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Capacity { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ElapsedTimeText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TotalAmountText { get; set; } = string.Empty;

    public SummarizedRentedCardViewModel(
        ILocalizationService localizationService,
        SummarizedRentedCardModel model)
        : base(localizationService)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        AreaName = _model.AreaName;

        var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        if (dispatcherQueue is not null)
        {
            _timer = dispatcherQueue.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += HandleTimerTick;
            _timer.Start();
        }

        RefreshLocalizedText();
    }

    protected override void RefreshLocalizedText()
    {
        UpdateElapsedTimeText();
        TotalAmountText = LocalizationService.FormatCurrency(_model.TotalAmount);

        Capacity = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("AreaManagementCapacityFormat"),
            _model.Capacity);
    }

    public new void Dispose()
    {
        if (_isDisposed)
            return;

        if (_timer is not null)
        {
            _timer.Stop();
            _timer.Tick -= HandleTimerTick;
        }

        _isDisposed = true;
        base.Dispose();
    }

    void IDisposable.Dispose()
    {
        Dispose();
    }

    private void HandleTimerTick(DispatcherQueueTimer sender, object args)
    {
        if (_isDisposed)
            return;

        UpdateElapsedTimeText();
    }

    private void UpdateElapsedTimeText()
    {
        var elapsedTime = DateTime.UtcNow - _model.StartTime.ToUniversalTime();
        if (elapsedTime < TimeSpan.Zero)
        {
            elapsedTime = TimeSpan.Zero;
        }

        ElapsedTimeText = string.Format(
            LocalizationService.Culture,
            "{0:00}:{1:00}:{2:00}",
            (int)elapsedTime.TotalHours,
            elapsedTime.Minutes,
            elapsedTime.Seconds);
    }
}
