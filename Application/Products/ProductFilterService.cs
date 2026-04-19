using System;
using Application.Services.Products;

namespace Application.Products;

public sealed class ProductFilterService : IProductFilterService
{
    public IReadOnlyList<TProduct> Apply<TProduct>(IEnumerable<TProduct> products, ProductFilter filter)
        where TProduct : IProductFilterable
    {
        ArgumentNullException.ThrowIfNull(products);
        ArgumentNullException.ThrowIfNull(filter);

        return products
            .Where(product =>
                MatchesProductType(product, filter)
                && MatchesPrice(product, filter))
            .ToList();
    }

    private static bool MatchesProductType(IProductFilterable product, ProductFilter filter)
    {
        return filter.ProductType is null || product.ProductType == filter.ProductType.Value;
    }

    private static bool MatchesPrice(IProductFilterable product, ProductFilter filter)
    {
        return (filter.PriceMin is null || product.Price >= filter.PriceMin.Value)
            && (filter.PriceMax is null || product.Price <= filter.PriceMax.Value);
    }
}
