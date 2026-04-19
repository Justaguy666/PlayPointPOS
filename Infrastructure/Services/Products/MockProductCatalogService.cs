using Application.Products;
using Application.Services.Products;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Services.Products;

public sealed class MockProductCatalogService : IProductCatalogService
{
    public IReadOnlyList<ProductRecord> GetProducts()
    {
        Product[] products =
        [
            new Product { Name = "Americano", Price = 35000m, Type = ProductType.Drink, StockQuantity = 18 },
            new Product { Name = "Cappuccino", Price = 45000m, Type = ProductType.Drink, StockQuantity = 14 },
            new Product { Name = "Matcha Latte", Price = 52000m, Type = ProductType.Drink, StockQuantity = 10 },
            new Product { Name = "Peach Tea", Price = 39000m, Type = ProductType.Drink, StockQuantity = 16 },
            new Product { Name = "Lemon Soda", Price = 42000m, Type = ProductType.Drink, StockQuantity = 12 },
            new Product { Name = "Mineral Water", Price = 15000m, Type = ProductType.Drink, StockQuantity = 25 },
            new Product { Name = "French Fries", Price = 45000m, Type = ProductType.Food, StockQuantity = 11 },
            new Product { Name = "Chicken Nuggets", Price = 58000m, Type = ProductType.Food, StockQuantity = 9 },
            new Product { Name = "Cheese Corn Dog", Price = 48000m, Type = ProductType.Food, StockQuantity = 8 },
            new Product { Name = "Spicy Sausage", Price = 55000m, Type = ProductType.Food, StockQuantity = 7 },
            new Product { Name = "Instant Noodles", Price = 32000m, Type = ProductType.Food, StockQuantity = 15 },
            new Product { Name = "Popcorn Bucket", Price = 60000m, Type = ProductType.Food, StockQuantity = 6 },
        ];

        return products
            .Select(CreateProductRecord)
            .ToList();
    }

    private static ProductRecord CreateProductRecord(Product product)
    {
        return new ProductRecord
        {
            Name = product.Name,
            Price = product.Price,
            Type = product.Type,
            StockQuantity = product.StockQuantity,
            ImageUri = "ms-appx:///Assets/Mock.png",
        };
    }
}
