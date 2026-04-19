using Domain.Enums;

namespace Application.Products;

public interface IProductFilterable
{
    ProductType ProductType { get; }

    decimal Price { get; }
}
