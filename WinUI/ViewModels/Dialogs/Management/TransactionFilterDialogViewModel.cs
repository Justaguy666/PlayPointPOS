using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Application.Services;
using Application.Transactions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Enums;
using WinUI.UIModels;

namespace WinUI.ViewModels.Dialogs.Management;

public partial class TransactionFilterDialogViewModel : LocalizedViewModelBase
{
    private Func<TransactionFilter, Task>? _onSubmittedAsync;
    private TransactionFilter _initialCriteria = new();
    private event Action? CloseRequestedInternal;

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PaymentMethodLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AmountRangeLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AmountMinPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AmountMaxPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DateRangeLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DateFromPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DateToPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ResetButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ApplyButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CloseTooltipText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? SelectedPaymentMethod { get; set; }

    [ObservableProperty]
    public partial string AmountMinText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AmountMaxText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDateFrom))]
    [NotifyPropertyChangedFor(nameof(DateFromDisplayText))]
    [NotifyPropertyChangedFor(nameof(DateFromFlyoutDate))]
    public partial DateTimeOffset? DateFrom { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDateTo))]
    [NotifyPropertyChangedFor(nameof(DateToDisplayText))]
    [NotifyPropertyChangedFor(nameof(DateToFlyoutDate))]
    public partial DateTimeOffset? DateTo { get; set; }

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasError { get; set; }

    public ObservableCollection<LocalizationOptionModel> PaymentMethodOptions { get; } = [];

    public bool HasDateFrom => DateFrom.HasValue;

    public bool HasDateTo => DateTo.HasValue;

    public string DateFromDisplayText => DateFrom.HasValue
        ? DateFrom.Value.ToString("dd/MM/yyyy", LocalizationService.Culture)
        : DateFromPlaceholderText;

    public string DateToDisplayText => DateTo.HasValue
        ? DateTo.Value.ToString("dd/MM/yyyy", LocalizationService.Culture)
        : DateToPlaceholderText;

    public DateTimeOffset DateFlyoutMinYear => new(new DateTime(2000, 1, 1), DateTimeOffset.Now.Offset);

    public DateTimeOffset DateFlyoutMaxYear => NormalizeDate(DateTimeOffset.Now).AddYears(5);

    public DateTimeOffset DateFromFlyoutDate => CoerceFlyoutDate(DateFrom);

    public DateTimeOffset DateToFlyoutDate => CoerceFlyoutDate(DateTo);

    public IRelayCommand ResetCommand { get; }

    public IAsyncRelayCommand ApplyCommand { get; }

    public IRelayCommand CloseCommand { get; }

    public TransactionFilterDialogViewModel(
        ILocalizationService localizationService)
        : base(localizationService)
    {
        ResetCommand = new RelayCommand(ExecuteReset);
        ApplyCommand = new AsyncRelayCommand(ExecuteApplyAsync);
        CloseCommand = new RelayCommand(ExecuteClose);
        RefreshLocalizedText();
    }

    public event Action? CloseRequested
    {
        add => CloseRequestedInternal += value;
        remove => CloseRequestedInternal -= value;
    }

    public void Configure(TransactionFilterDialogRequest? request)
    {
        if (request is null)
        {
            return;
        }

        _onSubmittedAsync = request.OnSubmittedAsync;
        _initialCriteria = request.InitialCriteria ?? new TransactionFilter();
        ApplyInitialCriteria();
    }

    protected override void RefreshLocalizedText()
    {
        Title = LocalizationService.GetString("TransactionFilterDialogTitleText");
        PaymentMethodLabelText = LocalizationService.GetString("TransactionFilterPaymentMethodLabelText");
        AmountRangeLabelText = LocalizationService.GetString("TransactionFilterAmountRangeLabelText");
        AmountMinPlaceholderText = LocalizationService.GetString("TransactionFilterAmountMinPlaceholderText");
        AmountMaxPlaceholderText = LocalizationService.GetString("TransactionFilterAmountMaxPlaceholderText");
        DateRangeLabelText = LocalizationService.GetString("TransactionFilterDateRangeLabelText");
        DateFromPlaceholderText = LocalizationService.GetString("TransactionFilterDateFromPlaceholderText");
        DateToPlaceholderText = LocalizationService.GetString("TransactionFilterDateToPlaceholderText");
        ResetButtonText = LocalizationService.GetString("TransactionFilterResetButtonText");
        ApplyButtonText = LocalizationService.GetString("TransactionFilterApplyButtonText");
        CloseTooltipText = LocalizationService.GetString("TransactionFilterCloseTooltipText");

        RefreshPaymentMethodOptions();
        OnPropertyChanged(nameof(DateFromDisplayText));
        OnPropertyChanged(nameof(DateToDisplayText));
    }

    private void RefreshPaymentMethodOptions()
    {
        string currentValue = SelectedPaymentMethod ?? string.Empty;
        PaymentMethodOptions.Clear();
        PaymentMethodOptions.Add(new LocalizationOptionModel { Value = "", DisplayName = LocalizationService.GetString("TransactionFilterAllOptionsText") });
        PaymentMethodOptions.Add(new LocalizationOptionModel { Value = "Cash", DisplayName = LocalizationService.GetString("PaymentMethodCashText") });
        PaymentMethodOptions.Add(new LocalizationOptionModel { Value = "Banking", DisplayName = LocalizationService.GetString("PaymentMethodBankingText") });
        SelectedPaymentMethod = PaymentMethodOptions.Any(o => o.Value == currentValue) ? currentValue : "";
    }

    private void ApplyInitialCriteria()
    {
        SelectedPaymentMethod = _initialCriteria.PaymentMethod?.ToString() ?? "";
        AmountMinText = _initialCriteria.AmountMin?.ToString() ?? string.Empty;
        AmountMaxText = _initialCriteria.AmountMax?.ToString() ?? string.Empty;
        DateFrom = _initialCriteria.DateFrom.HasValue ? NormalizeDate(new DateTimeOffset(_initialCriteria.DateFrom.Value)) : null;
        DateTo = _initialCriteria.DateTo.HasValue ? NormalizeDate(new DateTimeOffset(_initialCriteria.DateTo.Value)) : null;
        HasError = false;
        ErrorMessage = string.Empty;
    }

    private void ExecuteReset()
    {
        SelectedPaymentMethod = "";
        AmountMinText = string.Empty;
        AmountMaxText = string.Empty;
        DateFrom = null;
        DateTo = null;
        HasError = false;
        ErrorMessage = string.Empty;
    }

    public void ApplyDateFromSelection(DateTimeOffset date)
    {
        DateTimeOffset selectedDate = NormalizeDate(date);
        DateFrom = selectedDate;

        if (DateTo.HasValue && NormalizeDate(DateTo.Value) < selectedDate)
        {
            DateTo = selectedDate;
        }

        HasError = false;
        ErrorMessage = string.Empty;
    }

    public void ApplyDateToSelection(DateTimeOffset date)
    {
        DateTimeOffset selectedDate = NormalizeDate(date);
        DateTo = selectedDate;

        if (DateFrom.HasValue && NormalizeDate(DateFrom.Value) > selectedDate)
        {
            DateFrom = selectedDate;
        }

        HasError = false;
        ErrorMessage = string.Empty;
    }

    private async Task ExecuteApplyAsync()
    {
        HasError = false;

        decimal? amountMin = ParseDecimal(AmountMinText);
        decimal? amountMax = ParseDecimal(AmountMaxText);

        if (amountMin.HasValue && amountMax.HasValue && amountMin > amountMax)
        {
            ErrorMessage = LocalizationService.GetString("TransactionFilterInvalidRangeText");
            HasError = true;
            return;
        }

        PaymentMethod? paymentMethod = SelectedPaymentMethod switch
        {
            "Cash" => PaymentMethod.Cash,
            "Banking" => PaymentMethod.Banking,
            _ => null,
        };

        var criteria = new TransactionFilter
        {
            PaymentMethod = paymentMethod,
            AmountMin = amountMin,
            AmountMax = amountMax,
            DateFrom = DateFrom?.DateTime,
            DateTo = DateTo?.DateTime.Date.AddDays(1).AddTicks(-1),
        };

        if (_onSubmittedAsync is not null)
        {
            await _onSubmittedAsync(criteria);
        }

        CloseRequestedInternal?.Invoke();
    }

    private void ExecuteClose()
    {
        CloseRequestedInternal?.Invoke();
    }

    private static decimal? ParseDecimal(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return decimal.TryParse(text, out decimal value) ? value : null;
    }

    private DateTimeOffset CoerceFlyoutDate(DateTimeOffset? date)
    {
        DateTimeOffset selectedDate = date.HasValue ? NormalizeDate(date.Value) : NormalizeDate(DateTimeOffset.Now);

        if (selectedDate < DateFlyoutMinYear)
        {
            return DateFlyoutMinYear;
        }

        if (selectedDate > DateFlyoutMaxYear)
        {
            return DateFlyoutMaxYear;
        }

        return selectedDate;
    }

    private static DateTimeOffset NormalizeDate(DateTimeOffset date)
    {
        return new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, date.Offset);
    }
}
