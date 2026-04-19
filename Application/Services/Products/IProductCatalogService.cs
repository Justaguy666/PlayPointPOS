using Application.Products;

namespace Application.Services.Products;

public interface IProductCatalogService
{
    IReadOnlyList<ProductRecord> GetProducts();
}
