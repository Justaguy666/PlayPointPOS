namespace Application.Services;

/// <summary>
/// Mức độ ưu tiên / phân loại của thông báo.
/// </summary>
public enum NotificationType
{
    Success,
    Info,
    Warning,
    Error,
}

/// <summary>
/// Giao diện cho phép đẩy thông báo (Toast, Snackbar) từ tầng Application/ViewModel ra màn hình người dùng.
/// Giúp tách biệt logic xử lý hiển thị khỏi logic nghiệp vụ.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Gửi một thông báo lên hệ thống UI.
    /// </summary>
    /// <param name="title">Tiêu đề thông báo.</param>
    /// <param name="message">Nội dung chi tiết.</param>
    /// <param name="type">Loại thông báo (Success, Error...). Mặc định là Info.</param>
    Task SendAsync(string title, string message, NotificationType type = NotificationType.Info);
}

/// <summary>
/// Nơi phát ra sự kiện thông báo để tầng UI (MainWindow) có thể lắng nghe và hiển thị.
/// // WHY: Tách riêng Service (để push) và EventSource (để listen) giúp tránh vòng lặp phụ thuộc (circular dependency).
/// </summary>
public interface INotificationEventSource
{
    /// <summary>
    /// Bắn ra khi có một yêu cầu hiển thị thông báo mới.
    /// </summary>
    event Action<string, string, NotificationType>? NotificationRequested;
}
