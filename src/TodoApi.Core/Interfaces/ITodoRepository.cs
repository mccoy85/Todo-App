using TodoApi.Core.Entities;

namespace TodoApi.Core.Interfaces;

public interface ITodoRepository : IRepository<TodoItem>
{
    Task<(IEnumerable<TodoItem> Items, int TotalCount)> GetAllWithFiltersAsync(
        bool? isCompleted,
        Priority? priority,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize);

    Task<(IEnumerable<TodoItem> Items, int TotalCount)> GetDeletedWithFiltersAsync(
        bool? isCompleted,
        Priority? priority,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize);

    Task<TodoItem?> RestoreAsync(int id);
}
