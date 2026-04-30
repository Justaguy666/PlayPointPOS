using Domain.Enums;
using WinUI.UIModels.Management;

namespace WinUI.Services.Factories;

public sealed class ProductDraftFactory
{
    public ProductModel Create()
    {
        return new ProductModel
        {
            Name = string.Empty,
            ProductType = ProductType.Food,
            Price = 0m,
            StockQuantity = 0,
            ImageUri = "ms-appx:///Assets/Mock.png",
        };
    }
}
