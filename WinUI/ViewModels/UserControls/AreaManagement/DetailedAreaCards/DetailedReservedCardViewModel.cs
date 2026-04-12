using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Enums;
using Microsoft.UI.Xaml;
using WinUI.UIModels;
using WinUI.UIModels.AreaManagement;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.Dialogs.Management;

namespace WinUI.ViewModels.AreaManagement.DetailedAreaCards;

public partial class DetailedReservedCardViewModel : LocalizedViewModelBase, IDetailedAreaCardViewModel, IDisposable
{
    private readonly ILocalizationPreferencesService _localizationPreferencesService;
    private readonly IDialogService _dialogService;
    private readonly DispatcherTimer _timer;
    private bool _isDisposed;

    public string AreaName => Model.AreaName;

    [ObservableProperty]
    public partial string MaxCapacityText { get; set; }

    [ObservableProperty]
    public partial string ReservationInfoTitleText { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<LabelValueRowModel> ReservationInfoRows { get; set; } = Array.Empty<LabelValueRowModel>();

    [ObservableProperty]
    public partial string CheckInButtonText { get; set; }

    [ObservableProperty]
    public partial string EditButtonText { get; set; }

    [ObservableProperty]
    public partial string CancelReservationButtonText { get; set; }

    public IconState IconState => CreateIconState();

    public DetailedReservedCardViewModel(
        ILocalizationService localizationService,
        ILocalizationPreferencesService localizationPreferencesService,
        IDialogService dialogService,
        AreaModel model)
        : base(localizationService)
    {
        _localizationPreferencesService = localizationPreferencesService ?? throw new ArgumentNullException(nameof(localizationPreferencesService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Model.PropertyChanged += HandleModelPropertyChanged;

        CheckInCommand = new AsyncRelayCommand(ExecuteCheckInAsync, CanCheckIn);
        EditCommand = new AsyncRelayCommand(ExecuteEditReservationAsync);
        CancelReservationCommand = new AsyncRelayCommand(ExecuteCancelReservationAsync);

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (s, e) => CheckInCommand.NotifyCanExecuteChanged();
        _timer.Start();

        RefreshLocalizedText();
    }

    public AreaModel Model { get; }

    public IAsyncRelayCommand CheckInCommand { get; }

    public IAsyncRelayCommand EditCommand { get; }

    public IAsyncRelayCommand CancelReservationCommand { get; }

    protected override void RefreshLocalizedText()
    {
        MaxCapacityText = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("DetailedCardMaxCapacityFormat"),
            Model.MaxCapacity);

        DateTime reservationDateTime = Model.CheckInDateTime ?? DateTime.MinValue;
        string reservationDateText = FormatReservationDate(reservationDateTime);
        string reservationTimeText = reservationDateTime.ToString("HH:mm", LocalizationService.Culture);
        string reservationCustomerNumberText = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("AreaManagementCapacityFormat"),
            Model.Capacity);
        string reservationPriceText = LocalizationService.FormatCurrency(Model.ReservationPrice);

        CheckInButtonText = LocalizationService.GetString("CheckInButtonText");
        EditButtonText = LocalizationService.GetString("EditReservationButtonText");
        CancelReservationButtonText = LocalizationService.GetString("CancelReservationButtonText");

        ReservationInfoTitleText = LocalizationService.GetString("ReservationInfoTitleText");

        ReservationInfoRows =
        [
            new LabelValueRowModel(
                LocalizationService.GetString("ReservationCustomerNameLabelText"),
                Model.CustomerName),
            new LabelValueRowModel(
                LocalizationService.GetString("ReservationCustomerPhoneNumberLabelText"),
                Model.PhoneNumber),
            new LabelValueRowModel(
                LocalizationService.GetString("ReservationDateLabelText"),
                reservationDateText),
            new LabelValueRowModel(
                LocalizationService.GetString("ReservationTimeLabelText"),
                reservationTimeText,
                isHighlighted: true),
            new LabelValueRowModel(
                LocalizationService.GetString("ReservationCustomerNumberLabelText"),
                reservationCustomerNumberText),
            new LabelValueRowModel(
                LocalizationService.GetString("ReservationPriceLabelText"),
                reservationPriceText,
                showDivider: false),
        ];
    }

    private string FormatReservationDate(DateTime value)
    {
        string dateFormat = _localizationPreferencesService.Preferences.DateFormat;
        if (string.IsNullOrWhiteSpace(dateFormat))
        {
            dateFormat = LocalizationPreferences.DefaultDateFormat;
        }

        try
        {
            return value.ToString(dateFormat, LocalizationService.Culture);
        }
        catch (FormatException)
        {
            return value.ToString(LocalizationPreferences.DefaultDateFormat, LocalizationService.Culture);
        }
    }

    private bool CanCheckIn()
    {
        if (Model.CheckInDateTime == null)
            return false;

        return DateTime.Now >= Model.CheckInDateTime.Value;
    }

    private async Task ExecuteCheckInAsync()
    {
        bool isConfirmed = await _dialogService.ShowConfirmationAsync(
            titleKey: "ConfirmCheckInTitle",
            messageKey: "ConfirmCheckInMessage",
            confirmButtonTextKey: "ConfirmCheckInButton",
            cancelButtonTextKey: "CancelButtonText"
        );

        if (!isConfirmed)
        {
            return;
        }

        DateTime sessionStartedAtUtc = DateTime.UtcNow;

        Model.StartTime = sessionStartedAtUtc;
        Model.IsSessionPaused = false;
        Model.SessionPausedAt = null;
        Model.SessionPausedDuration = TimeSpan.Zero;
        Model.TotalAmount = 0m;
        Model.CheckInDateTime = null;
        Model.Status = PlayAreaStatus.Rented;
    }

    private Task ExecuteEditReservationAsync()
    {
        return _dialogService.ShowDialogAsync(
            "Reservation",
            new ReservationDialogRequest
            {
                Mode = UpsertDialogMode.Edit,
                Model = Model,
            });
    }

    private static Task ExecuteNoopAsync()
    {
        return Task.CompletedTask;
    }

    private async Task ExecuteCancelReservationAsync()
    {
        bool isConfirmed = await _dialogService.ShowConfirmationAsync(
            titleKey: "ConfirmCancelReservationTitle",
            messageKey: "ConfirmCancelReservationMessage",
            confirmButtonTextKey: "ConfirmCancelReservationButton",
            cancelButtonTextKey: "CancelButtonText");

        if (!isConfirmed)
        {
            return;
        }

        Model.CustomerName = string.Empty;
        Model.PhoneNumber = string.Empty;
        Model.MemberId = null;
        Model.CheckInDateTime = null;
        Model.Capacity = 0;
        Model.StartTime = null;
        Model.IsSessionPaused = false;
        Model.SessionPausedAt = null;
        Model.SessionPausedDuration = TimeSpan.Zero;
        Model.TotalAmount = 0m;
        Model.Status = PlayAreaStatus.Available;
    }

    public new void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _timer.Stop();
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
