using Domain.Entities;

namespace Application.Transactions;

public sealed class TransactionFilterService : ITransactionFilterService
{
    public IReadOnlyList<Transaction> Apply(IEnumerable<Transaction> transactions, TransactionFilter filter)
    {
        ArgumentNullException.ThrowIfNull(transactions);
        ArgumentNullException.ThrowIfNull(filter);

        IEnumerable<Transaction> result = transactions;

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
