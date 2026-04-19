using System;
using System.Collections.ObjectModel;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Domain.Entities;
using Domain.Enums;
using WinUI.UIModels.Management;

namespace WinUI.ViewModels.Dialogs.Management;

public partial class TransactionDetailDialogViewModel : LocalizedViewModelBase
{
    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ServiceDetailHeaderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AmountHeaderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DepositRefundText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DepositRefundAmountText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DiscountText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DiscountAmountText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TotalLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TotalAmountText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CloseTooltipText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasDiscount { get; set; }

    [ObservableProperty]
    public partial bool HasDepositRefund { get; set; }

    public ObservableCollection<TransactionLineDisplayModel> Lines { get; } = [];

    public TransactionDetailDialogViewModel(ILocalizationService localizationService)
        : base(localizationService)
    {
        RefreshLocalizedText();
    }

    public void Configure(TransactionDetailDialogRequest? request)
    {
        if (request?.Model is null)
        {
            return;
        }

        TransactionModel model = request.Model;
        Title = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("TransactionDetailDialogTitleFormat"),
            model.Code);

        Lines.Clear();
        foreach (TransactionLine line in model.Lines)
        {
            string description = line.Type switch
            {
                TransactionLineType.AreaRental => $"{line.Quantity} {LocalizationService.GetString("TransactionDetailHourSuffix")}",
                TransactionLineType.BoardGameRental => $"{line.Quantity} {LocalizationService.GetString("TransactionDetailHourSuffix")}",
                _ => $"{LocalizationService.GetString("TransactionDetailQuantityPrefix")}: {line.Quantity}",
            };

            Lines.Add(new TransactionLineDisplayModel
            {
                Name = line.ItemName,
                Description = description,
                AmountText = LocalizationService.FormatCurrency(line.TotalAmount),
            });
        }

        HasDepositRefund = model.DepositRefund != 0;
        DepositRefundAmountText = LocalizationService.FormatCurrency(-Math.Abs(model.DepositRefund));

        HasDiscount = model.DiscountAmount > 0;
        DiscountAmountText = LocalizationService.FormatCurrency(-model.DiscountAmount);

        TotalAmountText = LocalizationService.FormatCurrency(model.TotalAmount);
    }

    protected override void RefreshLocalizedText()
    {
        ServiceDetailHeaderText = LocalizationService.GetString("TransactionDetailServiceHeaderText");
        AmountHeaderText = LocalizationService.GetString("TransactionDetailAmountHeaderText");
        DepositRefundText = LocalizationService.GetString("TransactionDetailDepositRefundText");
        DiscountText = LocalizationService.GetString("TransactionDetailDiscountText");
        TotalLabelText = LocalizationService.GetString("TransactionDetailTotalLabelText");
        CloseTooltipText = LocalizationService.GetString("TransactionDetailCloseTooltipText");
    }
}

public sealed class TransactionLineDisplayModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AmountText { get; set; } = string.Empty;
}
