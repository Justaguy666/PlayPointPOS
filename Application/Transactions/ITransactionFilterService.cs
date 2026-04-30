namespace Application.Transactions;

public interface ITransactionFilterService
{
    IReadOnlyList<TTransaction> Apply<TTransaction>(IEnumerable<TTransaction> transactions, TransactionFilter filter)
        where TTransaction : ITransactionFilterable;
}
