namespace Application.Navigation;

/// <summary>
/// Quản lý việc điều hướng (Routing) giữa các trang (Pages) trong ứng dụng WinUI.
/// Đảm bảo ViewModel có thể chuyển trang mà không cần tham chiếu trực tiếp đến các class của UI Framework (như Frame).
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Gắn đối tượng Frame (thường là Frame chính của MainWindow) vào Service để thực thi việc chuyển trang.
    /// // WORKAROUND: Nhận object thay vì Frame để tránh rò rỉ (leak) thư viện WinUI xuống tầng Application.
    /// </summary>
    /// <param name="frame">UI Frame object.</param>
    void SetFrame(object frame);

    /// <summary>
    /// Điều hướng tới một màn hình mới dựa trên Navigation Request.
    /// </summary>
    /// <param name="request">Request chứa thông tin trang cần đến và tham số truyền đi.</param>
    void Navigate(INavigationRequest request);
}
