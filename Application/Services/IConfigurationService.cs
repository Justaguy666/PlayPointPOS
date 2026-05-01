namespace Application.Services;

/// <summary>
/// Giao diện quản lý cấu hình ứng dụng (đọc/ghi từ file config cục bộ).
/// Dùng để lưu trữ các thiết lập quan trọng như chuỗi kết nối API, trạng thái đăng nhập.
/// </summary>
public interface IConfigurationService
{
    /// <summary>Địa chỉ máy chủ API.</summary>
    string ServerAddress { get; }
    
    /// <summary>Cổng (Port) kết nối tới API.</summary>
    int Port { get; }
    
    /// <summary>
    /// Key xác thực để gọi API (nếu có).
    /// // SECURITY: Không lưu trữ raw key nếu key này có quyền admin hệ thống.
    /// </summary>
    string ApiKey { get; }
    
    /// <summary>Trạng thái ghi nhớ đăng nhập cho lần mở app tiếp theo.</summary>
    bool RememberMe { get; }
    
    /// <summary>Kiểm tra xem ứng dụng đã được cấu hình các thông số cơ bản chưa.</summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Tải cấu hình từ bộ nhớ (thường gọi lúc app startup).
    /// </summary>
    Task LoadAsync();

    /// <summary>
    /// Lưu các cấu hình mới xuống bộ nhớ.
    /// </summary>
    Task SaveAsync(string serverAddress, int port, string apiKey, bool rememberMe);
}
