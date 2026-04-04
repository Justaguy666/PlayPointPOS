namespace Domain.Entities;

/// <summary>
/// Shop account entity representing the business information and credentials for accessing the system.
/// </summary>
public class Account : BaseEntity
{
    public string ShopName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
