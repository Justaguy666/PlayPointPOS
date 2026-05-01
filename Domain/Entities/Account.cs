namespace Domain.Entities;

/// <summary>
/// Thực thể đại diện cho tài khoản quản trị của cửa hàng (Shop) dùng để đăng nhập vào hệ thống POS.
/// </summary>
public class Account : BaseEntity
{
    public string ShopName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    
    // SECURITY: Tuyệt đối chỉ lưu trữ chuỗi Hash (đã bao gồm Salt). 
    // Không bao giờ được phép lưu trữ hoặc log mật khẩu plain-text ra bất kỳ đâu.
    public string PasswordHash { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
}
