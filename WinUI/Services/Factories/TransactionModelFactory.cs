using Domain.Entities;
using System;

namespace WinUI.Services.Factories;

public sealed class TransactionModelFactory
{
    private static readonly string[] CustomerNames =
    [
        "Nguyễn Văn A",
        "Trần Thị B",
        "Lê Hoàng C",
        "Phạm Minh D",
        "Võ Thanh E",
    ];

    public UIModels.Management.TransactionModel Create(Transaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        string customerName = ResolveCustomerName(transaction.MemberId);

        return new UIModels.Management.TransactionModel
        {
            Code = transaction.Code,
            MemberId = transaction.MemberId,
            CustomerName = customerName,
            PaymentMethod = transaction.PaymentMethod,
            SubtotalAmount = transaction.SubtotalAmount,
            DiscountAmount = transaction.DiscountAmount,
            TotalAmount = transaction.TotalAmount,
            CreatedAt = transaction.CreatedAt,
            Lines = transaction.Lines,
        };
    }

    private static string ResolveCustomerName(string? memberId)
    {
        if (string.IsNullOrWhiteSpace(memberId))
        {
            return "Khách vãng lai";
        }

        int index = Math.Abs(memberId.GetHashCode()) % CustomerNames.Length;
        return CustomerNames[index];
    }
}
