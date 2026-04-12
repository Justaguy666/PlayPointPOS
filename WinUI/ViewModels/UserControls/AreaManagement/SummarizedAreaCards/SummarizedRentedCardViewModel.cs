using System;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Domain.Enums;
using Microsoft.UI.Dispatching;
using WinUI.UIModels.AreaManagement;

namespace WinUI.ViewModels.AreaManagement.SummarizedAreaCards;

public sealed partial class SummarizedRentedCardViewModel : LocalizedViewModelBase, ISummarizedAreaCardViewModel, IDisposable
{
    private readonly DispatcherQueueTimer? _timer;
    private bool _isDisposed;

    public string AreaName => Model.AreaName;

    public PlayAreaType PlayAreaType => Model.PlayAreaType;

    public PlayAreaStatus Status => Model.Status;

    [ObservableProperty]
    public partial string Capacity { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ElapsedTimeText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TotalAmountText { get; set; } = string.Empty;

    public SummarizedRentedCardViewModel(
        ILocalizationService localizationService,
        AreaModel model)
        : base(localizationService)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Model.PropertyChanged += HandleModelPropertyChanged;

        var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        if (dispatcherQueue is not null)
        {
            _timer = dispatcherQueue.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += HandleTimerTick;
        }

        SyncTimerState();
        RefreshLocalizedText();
    }

    public AreaModel Model { get; }

    protected override void RefreshLocalizedText()
    {
        UpdateElapsedTimeText();
        TotalAmountText = LocalizationService.FormatCurrency(Model.TotalAmount);

        Capacity = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("AreaManagementCapacityFormat"),
            Model.Capacity);
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

        Model.PropertyChanged -= HandleModelPropertyChanged;

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
        TimeSpan elapsedTime = Model.GetSessionElapsedTime(DateTime.UtcNow);

        ElapsedTimeText = string.Format(
            LocalizationService.Culture,
            "{0:00}:{1:00}:{2:00}",
            (int)elapsedTime.TotalHours,
            elapsedTime.Minutes,
            elapsedTime.Seconds);
    }

    private void HandleModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_isDisposed)
        {
            return;
        }

        SyncTimerState();
        OnPropertyChanged(nameof(AreaName));
        OnPropertyChanged(nameof(PlayAreaType));
        OnPropertyChanged(nameof(Status));
        RefreshLocalizedText();
    }

    private void SyncTimerState()
    {
        if (_timer is null)
        {
            return;
        }

        if (Model.IsSessionPaused)
        {
            _timer.Stop();
        }
        else
        {
            _timer.Start();
        }
    }
}
