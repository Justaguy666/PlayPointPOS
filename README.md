# PlayPointPOS

> Dự án cung cấp phần mềm quản lý bán hàng cho cửa tiệm board game nhỏ

## 🚀 Demo

## 📋 Yêu cầu

## ⚙️ Cài đặt

## 🔧 Cấu hình

## 📖 Sử dụng

## 🤝 Đóng góp

Mọi đóng góp đều được chào đón! Vui lòng làm theo các bước sau:

1. **Fork** repository này

2. **Tạo branch** cho tính năng của bạn

   ```bash
   git checkout -b feature/ten-tinh-nang
   ```

3. **Commit** các thay đổi

   ```bash
   git commit -m "feat: add feature X"
   ```

4. **Push** lên branch của bạn

   ```bash
   git push origin feature/ten-tinh-nang
   ```

5. Mở **Pull Request** và mô tả rõ những thay đổi bạn đã làm

### Quy ước commit

Dự án sử dụng [Conventional Commits](https://www.conventionalcommits.org/):

| Prefix     | Ý nghĩa                |
| :--------- | :--------------------  |
| `feat`     | Thêm tính năng mới     |
| `fix`      | Sửa lỗi                |
| `docs`     | Cập nhật tài liệu      |
| `refactor` | Tái cấu trúc code      |
| `test`     | Thêm hoặc sửa test     |

## 🗂 Cấu trúc thư mục

Dự án sử dụng Clean Architecture

Dưới đây là cấu trúc thư mục chính của dự án PlayPointPOS:

```text
PlayPointPOS/
├── API/                # Dự án GraphQL API (ASP.NET Core), xử lý request từ WinUI
│   ├── Program.cs
│   ├── appsettings.json
│   └── ...
├── Application/        # Thư mục chứa các service, logic nghiệp vụ, chỉ phụ thuộc Domain
│   └── ...
├── Domain/             # Thư mục chứa các entity, domain model, chứa logic cốt lõi nhất, không phụ thuộc layer nào khác
│   └── ...
├── Infrastructure/     # Thư mục chứa các lớp truy cập dữ liệu, repository, thực thi các Interface định nghĩa ở lớp trong
│   └── ...
├── IntegrationTests/   # Thư mục chứa các test tích hợp
│   └── ...
├── UnitTests/          # Thư mục chứa các test đơn vị
│   └── ...
├── WinUI/              # Ứng dụng giao diện người dùng (WinUI)
│   ├── App.xaml
│   ├── MainWindow.xaml
│   └── ...
├── PlayPointPOS.slnx   # File solution cho toàn bộ dự án
└── README.md           # Tài liệu này
```

Mỗi thư mục con đều có file README.md riêng để mô tả chi tiết hơn về vai trò và cấu trúc bên trong.

## 📄 License

Dự án này được cấp phép theo giấy phép **MIT License**.

---

© 2026 **Nguyễn Minh Khôi** & **Ong Khánh Vinh**  
🎓 Khoa Công nghệ Thông tin  
🏛️ Trường Đại học Khoa học Tự nhiên  
🇻🇳 Đại học Quốc gia Thành phố Hồ Chí Minh  
