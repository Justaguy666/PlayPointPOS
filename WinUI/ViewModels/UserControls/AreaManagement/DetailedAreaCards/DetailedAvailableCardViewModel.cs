using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Enums;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using WinUI.Helpers;
using WinUI.UIModels;
using WinUI.UIModels.Management;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.Dialogs.Management;

namespace WinUI.ViewModels.AreaManagement.DetailedAreaCards;

public partial class DetailedAvailableCardViewModel : LocalizedViewModelBase, IDetailedAreaCardViewModel, IDisposable
{
    private readonly IDialogService _dialogService;
    private readonly INotificationService _notificationService;
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
        INotificationService notificationService,
        AreaModel model)
        : base(localizationService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Model.PropertyChanged += HandleModelPropertyChanged;

        StartSessionCommand = new AsyncRelayCommand(OpenStartSessionDialogAsync);
        ReserveCommand = new AsyncRelayCommand(
            OpenReservationDialogAsync,
            AsyncRelayCommandOptions.AllowConcurrentExecutions);
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

    private async Task OpenStartSessionDialogAsync()
    {
        Debug.WriteLine($"[KhuVực>Mở] Command invoked. AreaId={Model.Id}, Name={Model.AreaName}, Type={Model.PlayAreaType}, Status={Model.Status}");
        try
        {
            await _dialogService.ShowDialogAsync("StartSession", Model);

            // Only flip status after dialog has fully returned to avoid WinUI teardown races.
            if (!string.IsNullOrWhiteSpace(Model.ActiveSessionId)
                && Model.StartTime is not null
                && Model.Status != PlayAreaStatus.Rented)
            {
                Model.Status = PlayAreaStatus.Rented;
            }
        }
        catch (Exception ex)
        {
            SessionFlowDebugLog.Append("DetailedAvailable.OpenStartSessionDialogAsync", ex);
            Debug.WriteLine(ex.ToString());
            await _notificationService.SendAsync(
                LocalizationService.GetString("StartSessionUnexpectedErrorTitle"),
                $"{ex}\n\nLog: {SessionFlowDebugLog.LogFilePath}",
                NotificationType.Error);
        }

        Debug.WriteLine("[KhuVực>Mở] await ShowDialogAsync completed (see [DialogService] BOUNDARY-* lines).");
    }

    private async Task OpenReservationDialogAsync()
    {
        Debug.WriteLine($"[KhuVực>Đặt chỗ] Command invoked. AreaId={Model.Id}, Name={Model.AreaName}, Type={Model.PlayAreaType}, Status={Model.Status}");
        await _dialogService.ShowDialogAsync(
            "Reservation",
            new ReservationDialogRequest
            {
                Mode = UpsertDialogMode.Add,
                Model = Model,
            });
        Debug.WriteLine("[KhuVực>Đặt chỗ] await ShowDialogAsync completed (see [DialogService] BOUNDARY-* lines).");
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
