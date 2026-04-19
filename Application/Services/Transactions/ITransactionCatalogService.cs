using Domain.Entities;

namespace Application.Services.Transactions;

public interface ITransactionCatalogService
{
    IReadOnlyList<Transaction> GetTransactions();
}
