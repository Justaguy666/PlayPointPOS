using Domain.Enums;

namespace Application.Transactions;

public sealed record TransactionLineRecord
{
    public TransactionLineType Type { get; init; }

    public string ItemName { get; init; } = string.Empty;

    public decimal Quantity { get; init; }

    public decimal TotalAmount { get; init; }
}
