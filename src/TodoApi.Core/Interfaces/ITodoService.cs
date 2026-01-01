using TodoApi.Core.DTOs;
using TodoApi.Core.Entities;

namespace TodoApi.Core.Interfaces;

public interface ITodoService
{
    Task<TodoListResponse> GetAllTodosAsync(TodoQueryParameters queryParams);
    Task<TodoListResponse> GetDeletedTodosAsync(TodoQueryParameters queryParams);
    Task<TodoItem?> GetTodoByIdAsync(int id);
    Task<TodoItem> CreateTodoAsync(CreateTodoRequest request);
    Task<TodoItem?> UpdateTodoAsync(int id, UpdateTodoRequest request);
    Task<TodoItem?> ToggleTodoAsync(int id);
    Task<bool> DeleteTodoAsync(int id);
    Task<TodoItem?> RestoreTodoAsync(int id);
}
