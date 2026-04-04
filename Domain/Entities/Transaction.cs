using Domain.Enums;

namespace Domain.Entities;

public class Transaction : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public string? MemberId { get; set; }
    public string? PlayAreaUnitId { get; set; }
    public List<TransactionLine> Lines { get; set; } = [];
    public PaymentMethod PaymentMethod { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
