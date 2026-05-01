# 🗄️ Infrastructure Layer

> Tầng **Infrastructure** là nơi "thực thi" (Implement) các Interface đã được định nghĩa ở tầng `Application`.

## Trách nhiệm
* Kết nối và xử lý dữ liệu thực tế (Database, API, File System).
* Hiện tại (Giai đoạn chưa tích hợp API Backend), tầng này đóng vai trò cung cấp các **Mock Services** và **Mock Repositories** để giả lập dữ liệu cho UI hiển thị mượt mà.
* Cung cấp các công cụ tiện ích hệ thống như mã hóa mật khẩu (`PasswordHasher`), lấy thời gian thực (`DateTimeService`), xử lý cài đặt file JSON (`ConfigurationService`).
* Xử lý các tác vụ liên đới trực tiếp đến Windows (Ví dụ: Windows Toast Notifications).

## Cấu trúc thư mục
```text
Infrastructure/
├── Repositories/     # Chứa các class tương tác trực tiếp với bộ nhớ lưu trữ
│   ├── Mock/         # Chứa MockAccountRepository, MockRepository... để giả lập dữ liệu tĩnh
│   └── (Tương lai)   # Sẽ chứa ApiRepository hoặc SqlRepository
│
└── Services/         # Chứa implementation thực tế của các Application/Domain services
    ├── Areas/        # Implementation cho Area Services (MockAreaCatalogService...)
    ├── Games/        # Implementation cho Game Services
    ├── Members/      # Implementation cho Member Services
    ├── Notification/ # Implementation hiển thị thông báo (ToastNotificationService tích hợp Windows OS)
    ├── Products/     # Implementation cho Product Services
    ├── Transactions/ # Implementation cho Transaction Services
    └── ConfigurationService.cs, DateTimeService.cs, PasswordHasher.cs... # Các class gọi hàm hệ thống
```

## 💡 Lưu ý cho Developer
* Tầng này phụ thuộc vào `Application` và `Domain`.
* Đây là nơi duy nhất nên chứa các package/thư viện kỹ thuật cụ thể (ví dụ: `BCrypt.Net-Next` để băm mật khẩu, Windows App SDK API).
* Khi dự án chuyển sang tích hợp HTTP API thật, developer sẽ tạo thêm các class `ApiRepository` tại đây (implement `IRepository`) để thay thế cho các `MockRepository` hiện tại mà không phải sửa đổi tầng UI hay tầng Application.
