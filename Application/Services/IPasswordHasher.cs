namespace Application.Services;

/// <summary>
/// Giao diện xử lý việc băm (hash) và xác thực mật khẩu.
/// // SECURITY: Bắt buộc phải implement bằng các thuật toán an toàn mạnh như BCrypt hoặc Argon2.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Băm mật khẩu dạng plain-text một cách an toàn để lưu vào cơ sở dữ liệu.
    /// </summary>
    /// <param name="plainPassword">Mật khẩu nguyên bản chưa mã hóa.</param>
    /// <returns>Chuỗi mật khẩu đã được băm (có kèm salt tự động sinh).</returns>
    string HashPassword(string plainPassword);

    /// <summary>
    /// Xác thực mật khẩu người dùng nhập vào so với chuỗi hash trong cơ sở dữ liệu.
    /// </summary>
    /// <param name="plainPassword">Mật khẩu người dùng nhập.</param>
    /// <param name="hash">Mật khẩu đã băm (lấy từ DB).</param>
    /// <returns>True nếu mật khẩu khớp, ngược lại False.</returns>
    bool VerifyPassword(string plainPassword, string hash);
}
