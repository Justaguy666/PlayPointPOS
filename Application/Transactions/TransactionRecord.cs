using Domain.Enums;

namespace Application.Transactions;

public sealed record TransactionRecord
{
    public string Code { get; init; } = string.Empty;

    public string? MemberId { get; init; }

    public IReadOnlyList<TransactionLineRecord> Lines { get; init; } = [];

    public PaymentMethod PaymentMethod { get; init; }

    public decimal SubtotalAmount { get; init; }

    public decimal DiscountAmount { get; init; }

    public decimal TotalAmount { get; init; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
