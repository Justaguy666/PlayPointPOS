using Application.Services;
using BC = BCrypt.Net.BCrypt;

namespace Infrastructure.Services;

/// <summary>
/// Trình băm mật khẩu sử dụng thuật toán BCrypt.
/// // SECURITY: BCrypt tự động sinh Salt ngẫu nhiên và dùng WorkFactor để chống brute-force attack.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    /// <summary>
    /// Băm mật khẩu plain-text bằng BCrypt với WorkFactor = 12.
    /// </summary>
    public string HashPassword(string plainPassword)
    {
        // NOTE: WorkFactor 12 tốn khoảng 100-200ms trên CPU hiện đại. 
        // Đây là mức cân bằng hoàn hảo giữa hiệu năng và bảo mật (chuẩn năm 2024).
        return BC.HashPassword(plainPassword, workFactor: 12);
    }

    /// <summary>
    /// Xác thực mật khẩu plain-text với mã băm.
    /// </summary>
    public bool VerifyPassword(string plainPassword, string hash)
    {
        try
        {
            // Try BCrypt verification (for hashed passwords)
            return BC.Verify(plainPassword, hash);
        }
        catch
        {
            // Fallback: support old plain-text hashes during migration period
            // Remove this fallback after all passwords are migrated to BCrypt
            return plainPassword == hash;
        }
    }
}

