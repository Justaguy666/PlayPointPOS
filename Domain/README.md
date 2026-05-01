# 🧱 Domain Layer

> Tầng **Domain** là trung tâm của kiến trúc, chứa các entity và business rule thuần túy. Tầng này **KHÔNG** phụ thuộc vào bất kỳ framework, thư viện UI hay database nào.

## Trách nhiệm
* Định nghĩa các thực thể (Entities) lõi của hệ thống (Ví dụ: `Member`, `Product`, `BoardGame`, `Transaction`).
* Chứa các Value Objects nếu có.
* Định nghĩa các Enums trạng thái dùng chung cho toàn bộ dự án.

## Cấu trúc thư mục
```text
Domain/
├── Entities/         # Các lớp mô hình hóa dữ liệu cốt lõi (Account, Member, Area, Game...)
├── Enums/            # Các hằng số và trạng thái (Role, TransactionStatus, ObjectState...)
├── Events/           # Các sự kiện trong hệ thống (Domain events) để thông báo khi có thay đổi trạng thái
├── Services/         # Các logic tính toán thuần túy nghiệp vụ không thuộc về một Entity cụ thể (Domain Services)
└── ValueObjects/     # Các object biểu diễn giá trị, không có identity riêng biệt (ví dụ: Money, Address)
```

## 💡 Lưu ý cho Developer
* **Tuyệt đối không** add references từ `Application`, `Infrastructure`, hoặc `WinUI` vào project này.
* Code ở tầng này chỉ bao gồm các class C# thuần túy (POCO). Bất cứ điều chỉnh nào ở tầng Domain cũng có thể ảnh hưởng sâu rộng đến toàn bộ hệ thống.
