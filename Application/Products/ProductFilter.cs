using Domain.Enums;

namespace Application.Products;

public sealed record ProductFilter
{
    public ProductType? ProductType { get; init; }

    public decimal? PriceMin { get; init; }

    public decimal? PriceMax { get; init; }
}
