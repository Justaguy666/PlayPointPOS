using Application.UseCases.Auth.Contracts;

namespace Application.Services;

/// <summary>
/// Cửa ngõ (Facade) điều phối các thao tác xác thực người dùng (Login/Register) ở tầng Application.
/// // WHY: Gom nhóm Use Cases lại giúp tầng UI gọi API xác thực gọn gàng hơn mà không phải phụ thuộc vào quá nhiều class lẻ.
/// </summary>
public interface IAuthenticationApplication
{
    /// <summary>
    /// Xác thực người dùng bằng email và mật khẩu.
    /// </summary>
    /// <param name="email">Email đăng nhập của người dùng.</param>
    /// <param name="password">Mật khẩu chưa mã hóa (sẽ được đối chiếu bằng <see cref="IPasswordHasher"/>).</param>
    /// <returns>Đối tượng LoginResult chứa trạng thái và thông tin tài khoản nếu thành công.</returns>
    Task<LoginResult> LoginAsync(string email, string password);

    /// <summary>
    /// Đăng ký tài khoản mới.
    /// </summary>
    /// <param name="email">Email đăng ký (yêu cầu là duy nhất trong hệ thống).</param>
    /// <param name="password">Mật khẩu đăng ký (sẽ được băm trước khi lưu trữ).</param>
    /// <param name="language">Ngôn ngữ mặc định của người dùng (ví dụ: "en-US", "vi-VN").</param>
    /// <returns>Đối tượng RegisterResult chứa trạng thái và thông báo lỗi (nếu có).</returns>
    Task<RegisterResult> RegisterAsync(string email, string password, string language);
}
