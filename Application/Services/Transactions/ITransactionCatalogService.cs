using Application.Transactions;

namespace Application.Services.Transactions;

public interface ITransactionCatalogService
{
    IReadOnlyList<TransactionRecord> GetTransactions();
}
