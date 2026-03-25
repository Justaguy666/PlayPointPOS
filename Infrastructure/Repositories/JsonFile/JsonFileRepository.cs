using System.Text.Json;
using Application.Interfaces;
using Domain.Entities;

namespace Infrastructure.Repositories.JsonFile;

public class JsonFileRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly string _filePath;
    private List<T> _items = new();

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public JsonFileRepository(string filePath)
    {
        _filePath = filePath;
        LoadFromFile();
    }

    public Task<IEnumerable<T>> GetAllAsync()
        => Task.FromResult<IEnumerable<T>>(_items);

    public Task<T?> GetByIdAsync(string id)
        => Task.FromResult(_items.FirstOrDefault(x => x.Id == id));

    public async Task AddAsync(T entity)
    {
        _items.Add(entity);
        await SaveToFileAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        var index = _items.FindIndex(x => x.Id == entity.Id);
        if (index >= 0)
        {
            _items[index] = entity;
            await SaveToFileAsync();
        }
    }

    public async Task DeleteAsync(string id)
    {
        _items.RemoveAll(x => x.Id == id);
        await SaveToFileAsync();
    }

    private void LoadFromFile()
    {
        if (!File.Exists(_filePath)) return;

        var json = File.ReadAllText(_filePath);
        _items = JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new();
    }

    private async Task SaveToFileAsync()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(_items, _jsonOptions);
        await File.WriteAllTextAsync(_filePath, json);
    }
}
