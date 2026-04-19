using System;
using Application.Products;
using WinUI.UIModels.Management;

namespace WinUI.Services.Factories;

public sealed class ProductModelFactory
{
    public ProductModel Create(ProductRecord source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new ProductModel
        {
            Name = source.Name,
            Price = source.Price,
            ProductType = source.Type,
            StockQuantity = source.StockQuantity,
            ImageUri = string.IsNullOrWhiteSpace(source.ImageUri) ? "ms-appx:///Assets/Mock.png" : source.ImageUri,
        };
    }

    public ProductModel Clone(ProductModel source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new ProductModel
        {
            Name = source.Name,
            Price = source.Price,
            ProductType = source.ProductType,
            StockQuantity = source.StockQuantity,
            ImageUri = source.ImageUri,
        };
    }
}
