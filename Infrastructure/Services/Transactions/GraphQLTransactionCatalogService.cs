using Application.Services;
using Application.Services.Transactions;
using Domain.Entities;

namespace Infrastructure.Services.Transactions;

public sealed class GraphQLTransactionCatalogService : ITransactionCatalogService
{
    private readonly IManagementApiService _managementApiService;

    public GraphQLTransactionCatalogService(IManagementApiService managementApiService)
    {
        _managementApiService = managementApiService;
    }

    public IReadOnlyList<Transaction> GetTransactions()
    {
        return Task.Run(async () => await _managementApiService.GetTransactionsAsync().ConfigureAwait(false))
            .GetAwaiter()
            .GetResult();
    }
}
