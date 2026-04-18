using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Enums;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using WinUI.UIModels;
using WinUI.UIModels.Management;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.Dialogs.Management;

namespace WinUI.ViewModels.AreaManagement.DetailedAreaCards;

public partial class DetailedAvailableCardViewModel : LocalizedViewModelBase, IDetailedAreaCardViewModel, IDisposable
{
    private readonly IDialogService _dialogService;
    private bool _isDisposed;

    public string AreaName => Model.AreaName;
    
    [ObservableProperty]
    public partial string MaxCapacityText { get; set; }

    [ObservableProperty]
    public partial string StartSessionButtonText { get; set; }

    [ObservableProperty]
    public partial string ReserveButtonText { get; set; }

    public IconState IconState => CreateIconState();

    public IAsyncRelayCommand StartSessionCommand { get; }

    public IAsyncRelayCommand ReserveCommand { get; }

    public DetailedAvailableCardViewModel(
        ILocalizationService localizationService,
        IDialogService dialogService,
        AreaModel model)
        : base(localizationService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Model.PropertyChanged += HandleModelPropertyChanged;

        StartSessionCommand = new AsyncRelayCommand(OpenStartSessionDialogAsync);
        ReserveCommand = new AsyncRelayCommand(OpenReservationDialogAsync);
        RefreshLocalizedText();
    }

    public AreaModel Model { get; }

    protected override void RefreshLocalizedText()
    {
        MaxCapacityText = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("DetailedCardMaxCapacityFormat"),
            Model.MaxCapacity);

        StartSessionButtonText = LocalizationService.GetString("StartSessionButtonText");
        ReserveButtonText = LocalizationService.GetString("ReserveButtonText");
    }

    private Task OpenStartSessionDialogAsync()
    {
        return _dialogService.ShowDialogAsync("StartSession", Model);
    }

    private Task OpenReservationDialogAsync()
    {
        return _dialogService.ShowDialogAsync(
            "Reservation",
            new ReservationDialogRequest
            {
                Mode = UpsertDialogMode.Add,
                Model = Model,
            });
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

    private void HandleModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isDisposed)
        {
            return;
        }

        OnPropertyChanged(nameof(AreaName));
        OnPropertyChanged(nameof(IconState));
        RefreshLocalizedText();
    }

    private IconState CreateIconState()
    {
        return new IconState
        {
            Size = 24,
            Kind = Model.PlayAreaType switch
            {
                PlayAreaType.Table => IconKind.Table,
                PlayAreaType.Room => IconKind.Room,
                _ => IconKind.Table
            },
            AlwaysFilled = true,
        };
    }
}
