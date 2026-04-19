using Domain.Entities;

namespace Application.Transactions;

public interface ITransactionFilterService
{
    IReadOnlyList<Transaction> Apply(IEnumerable<Transaction> transactions, TransactionFilter filter);
}
