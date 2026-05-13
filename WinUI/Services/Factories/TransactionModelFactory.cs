using Domain.Entities;
using System;

namespace WinUI.Services.Factories;

public sealed class TransactionModelFactory
{
    public UIModels.Management.TransactionModel Create(Transaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        return new UIModels.Management.TransactionModel
        {
            Id = transaction.Id,
            Code = transaction.Code,
            MemberId = transaction.MemberId,
            CustomerName = string.IsNullOrWhiteSpace(transaction.CustomerName) ? "Khach vang lai" : transaction.CustomerName,
            PaymentMethod = transaction.PaymentMethod,
            SubtotalAmount = transaction.SubtotalAmount,
            DepositRefund = transaction.DepositRefund,
            DiscountAmount = transaction.DiscountAmount,
            TotalAmount = transaction.TotalAmount,
            CreatedAt = transaction.CreatedAt,
            Lines = transaction.Lines,
        };
    }
}
