using Domain.Enums;

namespace Domain.Entities;

public class Transaction : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public string? MemberId { get; set; }
    public string? PlayAreaUnitId { get; set; }
    public string? PlayAreaSessionId { get; set; }
    public string? PlayAreaReservationId { get; set; }
    public List<TransactionLine> Lines { get; set; } = [];
    public PaymentMethod PaymentMethod { get; set; }
    
    // WHY: Bắt buộc sử dụng kiểu decimal thay vì double/float để tránh sai số làm tròn khi tính toán tiền bạc.
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    
    // NOTE: Lưu trữ cả Subtotal và Discount thay vì chỉ lưu Total giúp việc in lại biên lai (receipt) và thống kê chính xác hơn.
    public decimal TotalAmount { get; set; }
    
    // WHY: Dùng DateTime.UtcNow thay vì Now để đảm bảo tính nhất quán về thời gian khi lưu trữ, bất chấp múi giờ của máy trạm.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
