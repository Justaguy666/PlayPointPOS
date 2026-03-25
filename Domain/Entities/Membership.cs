namespace Domain.Entities;

public class Membership : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
}
