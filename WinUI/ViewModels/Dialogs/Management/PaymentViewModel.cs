using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Application.Services;
using Application.Transactions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Enums;
using WinUI.UIModels;
using WinUI.UIModels.Management;
using WinUI.UIModels.Enums;
using Application.Services.Areas;

namespace WinUI.ViewModels.Dialogs.Management;

public partial class PaymentViewModel : LocalizedViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IAreaSessionService _areaSessionService;
    private readonly IManagementApiService _managementApiService;
    private readonly IManagementDataPreloadService _managementDataPreloadService;
    private readonly INotificationService _notificationService;
    private AreaModel? _model;
    private event Action? CloseRequestedInternal;

    public event Action? DialogHideRequested;
    public event Action? DialogShowRequested;

    public PaymentViewModel(
        ILocalizationService localizationService,
        IDialogService dialogService,
        IAreaSessionService areaSessionService,
        IManagementApiService managementApiService,
        IManagementDataPreloadService managementDataPreloadService,
        INotificationService notificationService)
        : base(localizationService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _areaSessionService = areaSessionService ?? throw new ArgumentNullException(nameof(areaSessionService));
        _managementApiService = managementApiService ?? throw new ArgumentNullException(nameof(managementApiService));
        _managementDataPreloadService = managementDataPreloadService ?? throw new ArgumentNullException(nameof(managementDataPreloadService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        RefreshLocalizedText();
    }

    public IconState Icon { get; set; } = new() { Kind = IconKind.Wallet, Size = 24, AlwaysFilled = true };

    [ObservableProperty]
    public partial string TitleText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AreaLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AreaValueText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DurationLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DurationValueText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AreaFeeLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AreaFeeValueText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ProductFeeLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ProductFeeValueText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string GameFeeLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string GameFeeValueText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DepositLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DepositValueText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DiscountLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DiscountValueText { get; set; } = string.Empty;

    private string _paymentMethodLabelText = string.Empty;
    public string PaymentMethodLabelText
    {
        get => _paymentMethodLabelText;
        set => SetProperty(ref _paymentMethodLabelText, value);
    }

    private string _selectedPaymentMethodValue = nameof(PaymentMethod.Cash);
    public string SelectedPaymentMethodValue
    {
        get => _selectedPaymentMethodValue;
        set => SetProperty(ref _selectedPaymentMethodValue, value);
    }

    [ObservableProperty]
    public partial string TotalLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TotalValueText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ConfirmButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CancelButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CloseTooltipText { get; set; } = string.Empty;

    public ObservableCollection<LocalizationOptionModel> PaymentMethodOptions { get; } = [];

    public event Action? CloseRequested
    {
        add => CloseRequestedInternal += value;
        remove => CloseRequestedInternal -= value;
    }

    public void Configure(AreaModel? model)
    {
        _model = model;
        RefreshLocalizedText();
    }

    protected override void RefreshLocalizedText()
    {
        TitleText = LocalizationService.GetString("PaymentDialogTitleText");
        DurationLabelText = LocalizationService.GetString("PaymentDialogDurationLabelText");
        AreaFeeLabelText = LocalizationService.GetString("PaymentDialogAreaFeeLabelText");
        ProductFeeLabelText = LocalizationService.GetString("PaymentDialogProductFeeLabelText");
        GameFeeLabelText = LocalizationService.GetString("PaymentDialogGameFeeLabelText");
        DepositLabelText = LocalizationService.GetString("PaymentDialogDepositLabelText");
        DiscountLabelText = LocalizationService.GetString("PaymentDialogDiscountLabelText");
        PaymentMethodLabelText = LocalizationService.GetString("PaymentDialogMethodLabelText");
        TotalLabelText = LocalizationService.GetString("PaymentDialogTotalLabelText");
        ConfirmButtonText = LocalizationService.GetString("PaymentDialogConfirmButtonText");
        CancelButtonText = LocalizationService.GetString("CancelButtonText");
        CloseTooltipText = LocalizationService.GetString("CloseTooltipText");

        RefreshPaymentMethodOptions();

        UpdateComputedValues();
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseRequestedInternal?.Invoke();
    }

    [RelayCommand]
    private async Task ConfirmAsync()
    {
        if (_model is null)
        {
            CloseRequestedInternal?.Invoke();
            return;
        }

        DialogHideRequested?.Invoke();

        bool isConfirmed = await _dialogService.ShowConfirmationAsync(
            titleKey: "ConfirmPaymentTitle",
            messageKey: "ConfirmPaymentMessage",
            confirmButtonTextKey: "ConfirmPaymentButton",
            cancelButtonTextKey: "CancelButtonText");

        if (!isConfirmed)
        {
            DialogShowRequested?.Invoke();
            return;
        }

        if (!int.TryParse(_model.Id.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int areaId)
            || areaId <= 0
            || !int.TryParse(_model.ActiveSessionId?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int sessionId)
            || sessionId <= 0)
        {
            DialogShowRequested?.Invoke();
            await _notificationService.SendAsync(
                LocalizationService.GetString("PaymentDialogTitleText"),
                LocalizationService.GetString("PaymentCheckoutMissingSessionMessage"),
                NotificationType.Warning);
            return;
        }

        if (!Enum.TryParse(SelectedPaymentMethodValue, ignoreCase: true, out PaymentMethod paymentMethod))
        {
            paymentMethod = PaymentMethod.Cash;
        }

        TimeSpan elapsedTime = _areaSessionService.GetSessionElapsedTime(_model, DateTime.UtcNow);
        decimal areaFee = _areaSessionService.CalculateAreaSessionTotal(_model, elapsedTime);

        var extras = new List<AreaSessionCheckoutExtra>();
        foreach (PendingSessionSaleLine line in _model.PendingSessionLines)
        {
            if (line.IsGame)
            {
                DateTime started = line.GameRentalStartUtc ?? DateTime.UtcNow;
                decimal hours = (decimal)Math.Max(1d / 60d, (DateTime.UtcNow - started).TotalHours);
                extras.Add(new AreaSessionCheckoutExtra("game", line.CatalogId, hours, line.UnitPrice));
            }
            else
            {
                extras.Add(new AreaSessionCheckoutExtra("product", line.CatalogId, line.ProductQuantity, line.UnitPrice));
            }
        }

        try
        {
            await _managementApiService.CompleteAreaSessionCheckoutAsync(
                new AreaSessionCheckoutArgs(areaId, sessionId, paymentMethod, areaFee, extras));

            await _managementDataPreloadService.WarmUpAsync();
        }
        catch (Exception ex)
        {
            DialogShowRequested?.Invoke();
            await _notificationService.SendAsync(
                LocalizationService.GetString("PaymentCheckoutFailedTitle"),
                string.Format(
                    LocalizationService.Culture,
                    LocalizationService.GetString("PaymentCheckoutFailedMessage"),
                    ex.Message),
                NotificationType.Error);
            return;
        }

        _model.PendingSessionLines.Clear();
        _areaSessionService.CompletePayment(_model);
        CloseRequestedInternal?.Invoke();
    }

    private void UpdateComputedValues()
    {
        if (_model is null)
        {
            AreaLabelText = string.Empty;
            AreaValueText = string.Empty;
            DurationValueText = string.Empty;
            AreaFeeValueText = LocalizationService.FormatCurrency(0m);
            ProductFeeValueText = LocalizationService.FormatCurrency(0m);
            GameFeeValueText = LocalizationService.FormatCurrency(0m);
            DepositValueText = LocalizationService.FormatCurrency(0m);
            DiscountValueText = LocalizationService.FormatCurrency(0m);
            TotalValueText = LocalizationService.FormatCurrency(0m);
            return;
        }

        TimeSpan elapsedTime = _areaSessionService.GetSessionElapsedTime(_model, DateTime.UtcNow);
        decimal areaFee = _areaSessionService.CalculateAreaSessionTotal(_model, elapsedTime);
        decimal productFee = 0m;
        decimal gameFee = 0m;
        foreach (PendingSessionSaleLine line in _model.PendingSessionLines)
        {
            if (!line.IsGame)
            {
                productFee += line.UnitPrice * line.ProductQuantity;
            }
            else if (line.GameRentalStartUtc is DateTime startedUtc)
            {
                decimal hours = (decimal)Math.Max(1d / 60d, (DateTime.UtcNow - startedUtc).TotalHours);
                gameFee += line.UnitPrice * hours;
            }
        }

        decimal deposit = 0m;
        decimal discount = 0m;
        decimal total = areaFee + productFee + gameFee - deposit - discount;

        AreaLabelText = string.Format(
            LocalizationService.Culture,
            "{0}:",
            LocalizationService.GetString(
                _model.PlayAreaType == PlayAreaType.Room
                    ? "AreaDialogRoomTypeText"
                    : "AreaDialogTableTypeText"));
        AreaValueText = _model.AreaName;
        DurationValueText = FormatElapsedTime(elapsedTime);
        AreaFeeValueText = LocalizationService.FormatCurrency(areaFee);
        ProductFeeValueText = LocalizationService.FormatCurrency(productFee);
        GameFeeValueText = LocalizationService.FormatCurrency(gameFee);
        DepositValueText = FormatAdjustment(deposit);
        DiscountValueText = FormatAdjustment(discount);
        TotalValueText = LocalizationService.FormatCurrency(total);
    }

    private string FormatElapsedTime(TimeSpan elapsedTime)
    {
        return string.Format(
            LocalizationService.Culture,
            "{0:00}:{1:00}:{2:00}",
            (int)elapsedTime.TotalHours,
            elapsedTime.Minutes,
            elapsedTime.Seconds);
    }

    private string FormatAdjustment(decimal amount)
    {
        if (amount <= 0m)
        {
            return LocalizationService.FormatCurrency(0m);
        }

        return $"-{LocalizationService.FormatCurrency(amount)}";
    }

    private void RefreshPaymentMethodOptions()
    {
        PaymentMethodOptions.Clear();
        PaymentMethodOptions.Add(new LocalizationOptionModel
        {
            Value = nameof(PaymentMethod.Cash),
            DisplayName = LocalizationService.GetString("PaymentMethodCashText"),
        });
        PaymentMethodOptions.Add(new LocalizationOptionModel
        {
            Value = nameof(PaymentMethod.Banking),
            DisplayName = LocalizationService.GetString("PaymentMethodBankingText"),
        });

        if (string.IsNullOrWhiteSpace(SelectedPaymentMethodValue)
            || (SelectedPaymentMethodValue != nameof(PaymentMethod.Cash)
                && SelectedPaymentMethodValue != nameof(PaymentMethod.Banking)))
        {
            SelectedPaymentMethodValue = nameof(PaymentMethod.Cash);
        }
    }
}
