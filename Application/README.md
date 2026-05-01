# ⚙️ Application Layer

> Tầng **Application** chứa toàn bộ logic nghiệp vụ (Business Rules) của hệ thống. Nó quy định các "Hành động" mà ứng dụng có thể thực hiện.

## Trách nhiệm
* Định nghĩa các **Use Cases** (Ví dụ: Đăng nhập, Build Pagination, v.v).
* Định nghĩa các **Interfaces** (Abstractions) cho hệ thống lưu trữ hoặc các dịch vụ bên ngoài (Ví dụ: `IRepository<T>`, `ITransactionCatalogService`, `INotificationService`).
* Cung cấp các service nghiệp vụ như tính toán, lọc dữ liệu (FilterServices), phân trang.

## Tính độc lập
Tầng này **chỉ phụ thuộc duy nhất vào tầng `Domain`**. Tầng này hoàn toàn không biết WinUI hiển thị ra sao hay dữ liệu được lưu ở SQL hay dạng Mock.

## Cấu trúc thư mục
```text
Application/
├── Areas/            # Xử lý logic và filter cho khu vực (Area)
├── Games/            # Xử lý logic, filter và thống kê cho trò chơi (Game)
├── Interfaces/       # Các bản hợp đồng (Abstractions) để giao tiếp ra bên ngoài (ví dụ: IRepository, IFilePickerService)
├── Members/          # Xử lý logic liên quan đến Hội viên (Member, Membership)
├── Navigation/       # Chứa các Request phục vụ việc điều hướng (Routing) giữa các trang an toàn
├── Products/         # Xử lý logic nghiệp vụ cho sản phẩm bán kèm (Product, Snack, Drink)
├── Services/         # Chứa các interface/service dùng chung toàn app (ví dụ: ILocalizationService, INotificationService)
├── Transactions/     # Xử lý các nghiệp vụ cốt lõi về giao dịch và thanh toán
└── UseCases/         # Các luồng xử lý độc lập, mang tính hành động (ví dụ: Auth/LoginUserUseCase)
```

## 💡 Lưu ý cho Developer
* Mọi tương tác ra bên ngoài (đọc/ghi file, lấy dữ liệu, hiển thị thông báo) đều phải thông qua **Interface**.
* Implementation thực tế của các Interface này sẽ nằm ở tầng `Infrastructure` hoặc `WinUI` và được tiêm vào tự động qua Dependency Injection (DI) tại thời điểm runtime.
