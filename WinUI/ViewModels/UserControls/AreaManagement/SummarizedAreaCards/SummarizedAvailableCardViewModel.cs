using System;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Domain.Enums;
using WinUI.UIModels.AreaManagement;

namespace WinUI.ViewModels.AreaManagement.SummarizedAreaCards;

public sealed partial class SummarizedAvailableCardViewModel : LocalizedViewModelBase, ISummarizedAreaCardViewModel, IDisposable
{
    private bool _isDisposed;

    public string AreaName => Model.AreaName;

    public PlayAreaType PlayAreaType => Model.PlayAreaType;

    public PlayAreaStatus Status => Model.Status;

    public int MaxCapacity => Model.MaxCapacity;

    [ObservableProperty]
    public partial string AvailableStatusText { get; set; } = string.Empty;

    public SummarizedAvailableCardViewModel(
        ILocalizationService localizationService,
        AreaModel model)
        : base(localizationService)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Model.PropertyChanged += HandleModelPropertyChanged;
        RefreshLocalizedText();
    }

    public AreaModel Model { get; }

    protected override void RefreshLocalizedText()
    {
        AvailableStatusText = LocalizationService.GetString("AreaManagementAvailableStatusText");
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
        OnPropertyChanged(nameof(MaxCapacity));
    }
}
