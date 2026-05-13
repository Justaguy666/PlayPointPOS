using Application.Products;
using Application.Services;
using Application.Services.Products;

namespace Infrastructure.Services.Products;

public sealed class GraphQLProductCatalogService : IProductCatalogService
{
    private readonly IManagementApiService _managementApiService;

    public GraphQLProductCatalogService(IManagementApiService managementApiService)
    {
        _managementApiService = managementApiService;
    }

    public IReadOnlyList<ProductRecord> GetProducts()
    {
        return Task.Run(async () => await _managementApiService.GetProductsAsync().ConfigureAwait(false))
            .GetAwaiter()
            .GetResult();
    }
}
