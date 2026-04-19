using Application.Products;

namespace Application.Services.Products;

public interface IProductFilterService
{
    IReadOnlyList<TProduct> Apply<TProduct>(IEnumerable<TProduct> products, ProductFilter filter)
        where TProduct : IProductFilterable;
}
