# 🖥️ WinUI Layer

> Tầng **Presentation** là ứng dụng desktop xây dựng trên **WinUI 3**, giao tiếp với backend thông qua HTTP API.

## Trách nhiệm

## Công nghệ

## Cấu trúc

Sơ đồ cấu trúc thư mục WinUI Layer:

```text
WinUI/
├── App.xaml, App.xaml.cs         # Entry point của ứng dụng, khai báo Resource toàn cục, DI, startup logic

├── Assets/                       # Tài nguyên tĩnh: ảnh, icon, font, splash screen, v.v.

├── Configurations/               # Cấu hình ứng dụng (có thể load theo môi trường)
│   └── config.json               # File config: API URL, feature flags, settings

├── Contracts/                    # Định nghĩa interface/abstraction (dùng cho DI, tách implementation)

├── Converters/                   # Chuyển đổi dữ liệu cho binding (bool→Visibility, enum→string, v.v.)

├── Extensions/                   # Extension methods mở rộng cho class (string, DateTime, IEnumerable, ...)

├── MainWindow.xaml, .cs          # Cửa sổ chính (shell), thường chứa Frame để điều hướng Page

├── Properties/                   # Cấu hình project (launchSettings, publish profile, assembly info)

├── README.md                     # Tài liệu mô tả project, cách chạy, structure, guideline

├── Resources/                    # Tài nguyên XAML dùng chung toàn app
│   ├── Strings/                  # Localization (đa ngôn ngữ: .resw như vi, en)
│   └── Styles/                   # Style/UI design system (Button, TextBox, ListView, ...)

├── Services/                     # Tầng service xử lý logic, giao tiếp bên ngoài hoặc UI abstraction
│   ├── Dialog/                   # Hiển thị dialog (alert, confirm, custom dialog) thông qua service
│   ├── Api/                      # Gọi HTTP API (client setup, request/response DTO)
│   ├── Implementations/          # Cài đặt cụ thể của các service (class thực thi)
│   ├── Interfaces/               # Interface của service (phục vụ DI, test, loose coupling)
│   └── Navigation/               # Điều hướng giữa các Page/View (INavigationService, routing)

├── UIModels/                     # Model phục vụ UI (view state, display model, không phải domain model)

├── Utilities/                    # Helper dùng chung (date formatter, validator, constants, v.v.)

├── ViewModels/                   # ViewModel trong MVVM (binding data + command cho View)

├── Views/                        # Tầng UI (XAML)
│   ├── Dialogs/                  # Các dialog cụ thể (ContentDialog custom)
│   ├── Notifications/            # Toast, snackbar, thông báo nhẹ
│   ├── Pages/                    # Các màn hình chính (Page)
│   ├── UserControls/             # Component UI tái sử dụng
│   └── Windows/                  # Các cửa sổ riêng biệt (multi-window nếu có)

├── Package.appxmanifest          # Cấu hình đóng gói app (permissions, identity, capabilities)

├── WinUI.csproj                  # File project (dependency, build config)

```

Mỗi thư mục đảm nhận một vai trò riêng biệt, tuân thủ mô hình MVVM.
