using Domain.Entities;

namespace Application.Interfaces;

/// <summary>
/// Cung cấp giao diện chuẩn để truy xuất và thao tác với các thực thể trong cơ sở dữ liệu.
/// Dùng interface này thay vì gọi trực tiếp DB để dễ dàng thay đổi storage (Mock/API/SQL).
/// </summary>
/// <typeparam name="T">Loại thực thể kế thừa từ BaseEntity.</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Lấy toàn bộ danh sách thực thể.
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Lấy một thực thể dựa trên khóa chính.
    /// </summary>
    /// <param name="id">Mã định danh duy nhất của thực thể.</param>
    /// <returns>Thực thể nếu tìm thấy, ngược lại trả về null.</returns>
    Task<T?> GetByIdAsync(string id);

    /// <summary>
    /// Thêm một thực thể mới vào hệ thống lưu trữ.
    /// </summary>
    Task AddAsync(T entity);

    /// <summary>
    /// Cập nhật thông tin của một thực thể đã tồn tại.
    /// </summary>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Xóa một thực thể khỏi hệ thống dựa trên khóa chính.
    /// </summary>
    Task DeleteAsync(string id);
}
