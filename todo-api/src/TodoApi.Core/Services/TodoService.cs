using Microsoft.Extensions.Logging;
using TodoApi.Core.DTOs;
using TodoApi.Core.Entities;
using TodoApi.Core.Interfaces;

namespace TodoApi.Core.Services;

// Business logic for todo operations.
public class TodoService : ITodoService
{
    private readonly ITodoRepository _repository;
    private readonly ILogger<TodoService> _logger;

    public TodoService(ITodoRepository repository, ILogger<TodoService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    // Get all todos with optional filtering, sorting, and pagination.
    public async Task<TodoListResponse> GetAllTodosAsync(TodoQueryParameters queryParams)
    {
        var (items, totalCount) = await _repository.GetAllWithFiltersAsync(
            queryParams.IsCompleted,
            queryParams.Priority,
            queryParams.SortBy,
            queryParams.SortDescending,
            queryParams.Page,
            queryParams.PageSize);

        return new TodoListResponse
        {
            Items = items.Select(MapToResponse),
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        };
    }
    // Get all deleted todos with optional filtering, sorting, and pagination.
    public async Task<TodoListResponse> GetDeletedTodosAsync(TodoQueryParameters queryParams)
    {
        var (items, totalCount) = await _repository.GetDeletedWithFiltersAsync(
            queryParams.IsCompleted,
            queryParams.Priority,
            queryParams.SortBy,
            queryParams.SortDescending,
            queryParams.Page,
            queryParams.PageSize);

        return new TodoListResponse
        {
            Items = items.Select(MapToResponse),
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        };
    }

    public async Task<TodoItem?> GetTodoByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<TodoItem> CreateTodoAsync(CreateTodoRequest request)
    {
        var (title, description) = Normalize(request.Title, request.Description);

        var todo = new TodoItem
        {
            Title = title,
            Description = string.IsNullOrWhiteSpace(description) ? null : description,
            DueDate = request.DueDate,
            Priority = request.Priority,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(todo);

        _logger.LogInformation("Created todo with ID {Id}", todo.Id);

        return todo;
    }

    public async Task<TodoItem?> UpdateTodoAsync(int id, UpdateTodoRequest request)
    {
        var todo = await _repository.GetByIdAsync(id);

        if (todo == null)
        {
            return null;
        }

        var (title, description) = Normalize(request.Title, request.Description);

        todo.Title = title;
        todo.Description = string.IsNullOrWhiteSpace(description) ? null : description;
        todo.IsCompleted = request.IsCompleted;
        todo.DueDate = request.DueDate;
        todo.Priority = request.Priority;

        await _repository.UpdateAsync(todo);

        _logger.LogInformation("Updated todo with ID {Id}", id);

        return todo;
    }

    public async Task<TodoItem?> ToggleTodoAsync(int id)
    {
        var todo = await _repository.GetByIdAsync(id);

        if (todo == null)
        {
            return null;
        }

        todo.IsCompleted = !todo.IsCompleted;
        await _repository.UpdateAsync(todo);

        _logger.LogInformation("Toggled todo {Id} to {Status}", id, todo.IsCompleted ? "completed" : "incomplete");

        return todo;
    }

    public async Task<bool> DeleteTodoAsync(int id)
    {
        var result = await _repository.DeleteAsync(id);

        if (result)
        {
            _logger.LogInformation("Deleted todo with ID {Id}", id);
        }

        return result;
    }

    public async Task<TodoItem?> RestoreTodoAsync(int id)
    {
        var todo = await _repository.RestoreAsync(id);

        if (todo != null)
        {
            _logger.LogInformation("Restored todo with ID {Id}", id);
        }

        return todo;
    }

    private static TodoResponse MapToResponse(TodoItem todo) => new()
    {
        Id = todo.Id,
        Title = todo.Title,
        Description = todo.Description,
        IsCompleted = todo.IsCompleted,
        CreatedAt = todo.CreatedAt,
        DueDate = todo.DueDate,
        Priority = todo.Priority
    };
    // Normalize title and description by trimming whitespace.
    private static (string Title, string? Description) Normalize(
        string? title,
        string? description)
    {
        var trimmedTitle = title?.Trim() ?? string.Empty;
        var trimmedDescription = description?.Trim();

        return (trimmedTitle, trimmedDescription);
    }
}
