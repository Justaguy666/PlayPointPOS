# 🖥️ WinUI Layer (Presentation)

> Tầng **WinUI** là giao diện người dùng desktop xây dựng trên **WinUI 3**. Tầng này đóng vai trò là tầng hiển thị và là **Composition Root** (nơi khởi tạo toàn bộ Dependency Injection).

## Trách nhiệm
* Trực tiếp tương tác với người dùng thông qua giao diện XAML hiện đại.
* Đóng vai trò cấu hình hệ thống: Nạp cấu hình `appsettings.json`, khởi tạo `IHost`, đăng ký tất cả Services/Views/ViewModels vào container (Dependency Injection).
* Tuân thủ chuẩn kiến trúc **MVVM (Model-View-ViewModel)** mạnh mẽ thông qua thư viện `CommunityToolkit.Mvvm`.

## Cấu trúc thư mục cốt lõi
Kiến trúc UI khá đồ sộ, nhưng bạn chỉ cần chú ý đến các thành phần cốt lõi sau đây:

```text
WinUI/
├── App.xaml / .cs                # Entry point, khởi tạo IHost và Dependency Injection
├── Assets/                       # Hình ảnh, font chữ, icon tĩnh của ứng dụng
├── Behaviors/                    # Chứa logic gắn thêm cho UI elements (ví dụ như AutoScroll)
├── Composition/                  # Cấu hình DI cốt lõi (PlayPointServiceCollectionExtensions.cs)
├── Configurations/               # File cấu hình dạng JSON (appsettings.json)
├── Converters/                   # Chuyển đổi dữ liệu cho Binding (BooleanToVisibilityConverter, CurrencyConverter...)
├── Helpers/                      # Các hàm tiện ích tĩnh (WindowHelpers, ResourceHelpers...)
├── Properties/                   # Chứa launchSettings.json cho cấu hình khởi chạy
├── Resources/                    # Resource Dictionary (XAML) chứa Colors, FontSize, Images, Animations
├── Selectors/                    # Template Selectors cho List/Grid hiển thị item theo điều kiện
├── Services/                     # Các Service dành riêng cho giao diện (DialogService, NavigationService, WindowService)
├── Themes/                       # Resource định nghĩa style mặc định cho UI Controls (Generic.xaml)
├── UIModels/                     # Các Model trung gian chỉ dùng trên UI (để không dùng trực tiếp Domain Entities)
├── ViewModels/                   # Logic MVVM (C# code) chia theo Pages, Dialogs, UserControls
└── Views/                        # Giao diện hiển thị (XAML) chia theo Pages, Dialogs, UserControls, Windows
```

## 💡 Hướng dẫn làm việc với MVVM & DI
1. **Thêm thành phần UI mới:**
   * Tạo `View` (XAML) trong thư mục `Views/...`
   * Tạo `ViewModel` kế thừa từ `LocalizedViewModelBase` hoặc `ObservableObject`.
   * Gắn ViewModel vào View thông qua cơ chế Data Binding.
   * Quan trọng: Phải đăng ký cả View và ViewModel trong `Composition/PlayPointServiceCollectionExtensions.cs` để DI container có thể tự động inject.

2. **Quy tắc ViewModel:**
   * ViewModel **không được phép** tham chiếu trực tiếp đến các control UI (ví dụ: `TextBox`, `Frame`, `ContentDialog`).
   * Để gọi popup, dùng `IDialogService`.
   * Để chuyển trang, dùng `INavigationService`.
   * ViewModel không chứa các vòng lặp tính toán nghiệp vụ phức tạp. Nếu cần xử lý nghiệp vụ, hãy gọi đến các Service/UseCases từ tầng `Application` thông qua constructor injection.
