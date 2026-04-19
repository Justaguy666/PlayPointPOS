using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Application.Services;
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
    private AreaModel? _model;
    private event Action? CloseRequestedInternal;

    public event Action? DialogHideRequested;
    public event Action? DialogShowRequested;

    public PaymentViewModel(
        ILocalizationService localizationService,
        IDialogService dialogService,
        IAreaSessionService areaSessionService)
        : base(localizationService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _areaSessionService = areaSessionService ?? throw new ArgumentNullException(nameof(areaSessionService));
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
