using Domain.Enums;

namespace Domain.Entities;

public class TransactionLine : BaseEntity
{
    public string TransactionId { get; set; } = string.Empty;
    public TransactionLineType Type { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal Quantity { get; set; }
    public decimal TotalAmount { get; set; }
}
