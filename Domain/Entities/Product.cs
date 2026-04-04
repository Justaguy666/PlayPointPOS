using Domain.Enums;

namespace Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public ProductType Type { get; set; }
    public int StockQuantity { get; set; }
}
