namespace Domain.Entities;

public class Membership : BaseEntity
{
    public string MemberId { get; set; } = string.Empty;
    public string MembershipRankId { get; set; } = string.Empty;
    public decimal TotalSpentAmount { get; set; }
    public DateTime JoinDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
