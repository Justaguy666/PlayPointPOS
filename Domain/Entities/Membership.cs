namespace Domain.Entities;

// WHY: Tách riêng Membership (thông tin thẻ, điểm số, hạng) khỏi Member (thông tin cá nhân)
// để cho phép một người dùng có thể làm lại thẻ mới, hoặc reset điểm theo mùa mà không ảnh hưởng đến profile cá nhân.
public class Membership : BaseEntity
{
    public string MemberId { get; set; } = string.Empty;
    public string MembershipRankId { get; set; } = string.Empty;
    public decimal TotalSpentAmount { get; set; }
    public DateTime JoinDate { get; set; } = DateTime.UtcNow;
}
