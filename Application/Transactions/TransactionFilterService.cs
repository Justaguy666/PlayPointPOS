namespace Application.Transactions;

public sealed class TransactionFilterService : ITransactionFilterService
{
    public IReadOnlyList<TTransaction> Apply<TTransaction>(IEnumerable<TTransaction> transactions, TransactionFilter filter)
        where TTransaction : ITransactionFilterable
    {
        ArgumentNullException.ThrowIfNull(transactions);
        ArgumentNullException.ThrowIfNull(filter);

        IEnumerable<TTransaction> result = transactions;

        if (filter.PaymentMethod.HasValue)
        {
            result = result.Where(t => t.PaymentMethod == filter.PaymentMethod.Value);
        }

        if (filter.AmountMin.HasValue)
        {
            result = result.Where(t => t.TotalAmount >= filter.AmountMin.Value);
        }

        if (filter.AmountMax.HasValue)
        {
            result = result.Where(t => t.TotalAmount <= filter.AmountMax.Value);
        }

        if (filter.DateFrom.HasValue)
        {
            result = result.Where(t => t.CreatedAt >= filter.DateFrom.Value);
        }

        if (filter.DateTo.HasValue)
        {
            result = result.Where(t => t.CreatedAt <= filter.DateTo.Value);
        }

        return result.ToList();
    }
}
