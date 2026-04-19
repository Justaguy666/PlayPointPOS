using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Enums;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using WinUI.Resources;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.UIModels.Management;

namespace WinUI.ViewModels.UserControls.Transactions;

public partial class TransactionCardControlViewModel : LocalizedViewModelBase
{
    private static readonly Color CashBadgeBackgroundColor = Color.FromArgb(255, 16, 185, 129);
    private static readonly Color CashBadgeForegroundColor = Color.FromArgb(255, 255, 255, 255);
    private static readonly Color BankingBadgeBackgroundColor = Color.FromArgb(255, 59, 130, 246);
    private static readonly Color BankingBadgeForegroundColor = Color.FromArgb(255, 255, 255, 255);

    private readonly Func<TransactionModel, Task>? _showDetailAction;
    private readonly Func<TransactionModel, Task>? _togglePaymentMethodAction;

    [ObservableProperty]
    public partial string TransactionCode { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CustomerName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MemberIdText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TotalAmountText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DateText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TimeText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PaymentMethodText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial Brush PaymentBadgeBackground { get; set; } = new SolidColorBrush(CashBadgeBackgroundColor);

    [ObservableProperty]
    public partial Brush PaymentBadgeForeground { get; set; } = new SolidColorBrush(CashBadgeForegroundColor);

    [ObservableProperty]
    public partial string TransactionCodeLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MemberIdLabelText { get; set; } = string.Empty;

    public IconState CalendarIconState { get; } = new()
    {
        Kind = IconKind.Calendar,
        Size = 16,
        AlwaysFilled = true,
    };

    public IconState ClockIconState { get; } = new()
    {
        Kind = IconKind.Clock,
        Size = 16,
        AlwaysFilled = true,
    };

    public TransactionModel Model { get; }

    public IAsyncRelayCommand ShowDetailCommand { get; }

    public IAsyncRelayCommand TogglePaymentMethodCommand { get; }

    public TransactionCardControlViewModel(
        ILocalizationService localizationService,
        TransactionModel model,
        Func<TransactionModel, Task>? showDetailAction,
        Func<TransactionModel, Task>? togglePaymentMethodAction)
        : base(localizationService)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
        _showDetailAction = showDetailAction;
        _togglePaymentMethodAction = togglePaymentMethodAction;

        ShowDetailCommand = new AsyncRelayCommand(ExecuteShowDetailAsync);
        TogglePaymentMethodCommand = new AsyncRelayCommand(ExecuteTogglePaymentMethodAsync);

        Model.PropertyChanged += HandleModelPropertyChanged;
        RefreshLocalizedText();
        RefreshPresentation();
    }

    protected override void RefreshLocalizedText()
    {
        TransactionCodeLabelText = LocalizationService.GetString("TransactionCardCodeLabelText");
        MemberIdLabelText = LocalizationService.GetString("TransactionCardMemberIdLabelText");
        RefreshPresentation();
    }

    public new void Dispose()
    {
        Model.PropertyChanged -= HandleModelPropertyChanged;
        base.Dispose();
    }

    private void HandleModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        RefreshPresentation();
    }

    private void RefreshPresentation()
    {
        TransactionCode = Model.Code;
        CustomerName = Model.CustomerName;
        MemberIdText = string.IsNullOrWhiteSpace(Model.MemberId)
            ? $"{MemberIdLabelText} -"
            : $"{MemberIdLabelText} {Model.MemberId}";
        TotalAmountText = LocalizationService.FormatCurrency(Model.TotalAmount);
        DateText = Model.CreatedAt.ToString("dd/MM/yyyy");
        TimeText = Model.CreatedAt.ToString("HH:mm");

        bool isCash = Model.PaymentMethod == PaymentMethod.Cash;
        PaymentMethodText = isCash
            ? LocalizationService.GetString("PaymentMethodCashText")
            : LocalizationService.GetString("PaymentMethodBankingText");

        PaymentBadgeBackground = new SolidColorBrush(isCash ? CashBadgeBackgroundColor : BankingBadgeBackgroundColor);
        PaymentBadgeForeground = new SolidColorBrush(isCash ? CashBadgeForegroundColor : BankingBadgeForegroundColor);
    }

    private async Task ExecuteShowDetailAsync()
    {
        if (_showDetailAction is not null)
        {
            await _showDetailAction(Model);
        }
    }

    private async Task ExecuteTogglePaymentMethodAsync()
    {
        if (_togglePaymentMethodAction is not null)
        {
            await _togglePaymentMethodAction(Model);
        }
    }
}
