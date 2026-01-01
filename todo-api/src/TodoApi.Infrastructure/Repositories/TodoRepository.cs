using Microsoft.EntityFrameworkCore;
using TodoApi.Core.Entities;
using TodoApi.Core.Interfaces;
using TodoApi.Infrastructure.Data;

namespace TodoApi.Infrastructure.Repositories;

// Todo-specific queries and soft-delete operations.
public class TodoRepository : Repository<TodoItem>, ITodoRepository
{
    public TodoRepository(TodoDbContext context) : base(context)
    {
    }

    public async Task<(IEnumerable<TodoItem> Items, int TotalCount)> GetAllWithFiltersAsync(
        bool? isCompleted,
        Priority? priority,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize)
    {
        var query = _dbSet.AsQueryable();

        // Apply completed filters
        if (isCompleted.HasValue)
        {
            query = query.Where(t => t.IsCompleted == isCompleted.Value);
        }
        // Apply priority filter
        if (priority.HasValue)
        {
            query = query.Where(t => t.Priority == priority.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "title" => sortDescending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
            "duedate" => sortDescending ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
            "priority" => sortDescending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            "iscompleted" => sortDescending ? query.OrderByDescending(t => t.IsCompleted) : query.OrderBy(t => t.IsCompleted),
            _ => sortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt)
        };

        // Apply pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(IEnumerable<TodoItem> Items, int TotalCount)> GetDeletedWithFiltersAsync(
        bool? isCompleted,
        Priority? priority,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize)
    {
        // Start with all deleted items
        var query = _dbSet.IgnoreQueryFilters().Where(t => t.IsDeleted);
        // Apply completed filters
        if (isCompleted.HasValue)
        {
            query = query.Where(t => t.IsCompleted == isCompleted.Value);
        }
        // Apply priority filter
        if (priority.HasValue)
        {
            query = query.Where(t => t.Priority == priority.Value);
        }
        // Get total count before pagination
        var totalCount = await query.CountAsync();
        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "title" => sortDescending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
            "duedate" => sortDescending ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
            "priority" => sortDescending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            "iscompleted" => sortDescending ? query.OrderByDescending(t => t.IsCompleted) : query.OrderBy(t => t.IsCompleted),
            _ => sortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt)
        };
        // Apply pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        // Return results
        return (items, totalCount);
    }

    public async Task<TodoItem?> RestoreAsync(int id)
    {   // Find the deleted todo
        var todo = await _dbSet.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == id);
        // If not found or not deleted, return null       
        if (todo == null || !todo.IsDeleted)
        {
            return null;
        }
        // Restore the todo
        todo.IsDeleted = false;
        todo.DeletedAt = null;
        _dbSet.Update(todo);
        await SaveChangesAsync();

        return todo;
    }
}
