namespace Domain.Entities;

public class Member : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string MembershipId { get; set; } = string.Empty;
    public Membership? Membership { get; set; }
    public bool IsActive { get; set; } = true;
}
