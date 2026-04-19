using Domain.Enums;

namespace Application.Products;

public sealed record ProductRecord
{
    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public ProductType Type { get; set; }

    public int StockQuantity { get; set; }

    public string ImageUri { get; set; } = "ms-appx:///Assets/Mock.png";
}
