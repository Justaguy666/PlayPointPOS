using Application.Interfaces;
using Domain.Entities;

namespace Infrastructure.Repositories.Mock;

// TODO: Thay thế toàn bộ MockRepository này bằng ApiRepository khi Backend API đã sẵn sàng. [Ticket: TBD]
// Hiện tại dùng List trong RAM để UI có thể test luồng chạy độc lập.
public class MockRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly List<T> _items = new();

    public Task<IEnumerable<T>> GetAllAsync()
        => Task.FromResult<IEnumerable<T>>(_items);

    public Task<T?> GetByIdAsync(string id)
        => Task.FromResult(_items.FirstOrDefault(x => x.Id == id));

    public Task AddAsync(T entity)
    {
        _items.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity)
    {
        var index = _items.FindIndex(x => x.Id == entity.Id);
        if (index >= 0)
        {
            _items[index] = entity;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id)
    {
        _items.RemoveAll(x => x.Id == id);
        return Task.CompletedTask;
    }
}
