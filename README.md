# PlayPointPOS

> Dự án cung cấp phần mềm quản lý bán hàng cho cửa tiệm board game nhỏ

PlayPointPOS là một ứng dụng Desktop hiện đại được xây dựng trên nền tảng **WinUI 3** và **.NET 10**, thiết kế theo chuẩn **Clean Architecture** kết hợp với pattern **MVVM**.

## 🏗️ Tổng quan kiến trúc (Clean Architecture)

Dự án được chia thành 4 layer chính, tuân thủ nghiêm ngặt **Dependency Rule** (Luồng phụ thuộc chỉ hướng vào trong tâm):

1. **[Domain](./Domain/README.md)**: Chứa các Models/Entities cốt lõi (Member, Product, Game...). Hoàn toàn độc lập, không phụ thuộc bất kỳ layer nào.
2. **[Application](./Application/README.md)**: Chứa các Business Logic, Use Cases và định nghĩa Interfaces (IRepository, IServices). Chỉ phụ thuộc vào Domain.
3. **[Infrastructure](./Infrastructure/README.md)**: Triển khai (implement) các Interfaces từ Application (Mock Data, File System, Cryptography). Phụ thuộc vào Application và Domain.
4. **[WinUI](./WinUI/README.md)**: Tầng giao diện người dùng. Đóng vai trò là Composition Root (Nơi thiết lập Dependency Injection) và chứa toàn bộ logic giao diện theo chuẩn MVVM.

### 🔄 Luồng hoạt động (Data Flow)
* **View** (WinUI XAML) binding dữ liệu từ **ViewModel**.
* **ViewModel** không chứa business logic phức tạp, mà gọi các **Use Cases / Services** từ tầng `Application`.
* Các **Use Cases** tương tác với dữ liệu thông qua các **Interfaces** (như `IRepository`).
* Tầng **Infrastructure** cung cấp implementation thực tế (Mock Data, API Client) cho các Interfaces đó, được tự động tiêm vào thông qua **Dependency Injection (DI)**.

## ⚙️ Công nghệ sử dụng
* **Framework:** .NET 10, Windows App SDK (WinUI 3).
* **Pattern:** Clean Architecture, MVVM, Dependency Injection, Factory Pattern.
* **Libraries:** `CommunityToolkit.Mvvm`, `Microsoft.Extensions.DependencyInjection`, `BCrypt.Net-Next`.

## 🚀 Hướng dẫn bắt đầu

Nếu bạn mới tiếp cận dự án, kiến trúc có thể trông khá lớn. Hãy làm quen theo thứ tự sau để không bị ngợp:
1. Đọc **Domain Layer** để hiểu ứng dụng đang quản lý những thực thể (Entities) nào.
2. Đọc **Application Layer** (đặc biệt là thư mục `Interfaces` và `UseCases`) để biết ứng dụng làm được những gì.
3. Đọc file cấu hình DI `PlayPointServiceCollectionExtensions.cs` trong **WinUI/Composition** để biết cách các class được liên kết với nhau.
4. Cuối cùng, xem cách các `Views` và `ViewModels` trong **WinUI** tương tác để hiển thị dữ liệu lên màn hình.

## 📝 Code Comment Convention

Dự án áp dụng quy tắc comment tối giản nhưng hiệu quả, nhằm giải thích **LÝ DO (WHY)** thay vì lặp lại **HÀNH ĐỘNG (WHAT)** của code:

- Ưu tiên code dễ đọc (clean code, naming tốt) hơn là lạm dụng comment.
- Comment phải giải thích **TẠI SAO** quyết định kỹ thuật/nghiệp vụ lại được đưa ra.
- **Public Interfaces/APIs** dùng chung cho nhiều module bắt buộc phải có XML documentation (`/// <summary>`).
- Bắt buộc comment khi có: Business rules phức tạp, Security checks, Workarounds, Quyết định kỹ thuật khó hiểu (Performance...).
- Khi dùng `TODO` hoặc `FIXME`, phải luôn đính kèm lý do hoặc Ticket ID.
- Xóa ngay comment cũ khi sửa code.

**Các tag thường dùng trong comment:**
- `// WHY:` Giải thích lý do nghiệp vụ hoặc kỹ thuật.
- `// TODO:` Việc cần làm tiếp (kèm Ticket/Lý do).
- `// FIXME:` Lỗi cần sửa (kèm Ticket/Lý do).
- `// WORKAROUND:` Cách xử lý tạm thời cho một vấn đề framework/library.
- `// NOTE:` Thông tin quan trọng cho người đọc.
- `// SECURITY:` Điều kiện liên quan bảo mật.
- `// PERF:` Lý do tối ưu hiệu năng.

## 🤝 Đóng góp
Dự án sử dụng [Conventional Commits](https://www.conventionalcommits.org/):
| Prefix | Ý nghĩa |
| :--- | :--- |
| `feat` | Thêm tính năng mới |
| `fix` | Sửa lỗi |
| `docs` | Cập nhật tài liệu |
| `refactor` | Tái cấu trúc code |
| `test` | Thêm hoặc sửa test |

## 📄 License
Dự án này được cấp phép theo giấy phép **MIT License**.

---
© 2026 **Nguyễn Minh Khôi** & **Ong Khánh Vinh** | Khoa CNTT - ĐH Khoa học Tự nhiên TP.HCM
