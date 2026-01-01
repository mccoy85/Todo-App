using Microsoft.AspNetCore.Mvc;
using TodoApi.Core.DTOs;
using TodoApi.Core.Entities;
using TodoApi.Core.Interfaces;

namespace TodoApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private readonly ITodoService _todoService;

    public TodoController(ITodoService todoService)
    {
        _todoService = todoService;
    }

    /// <summary>
    /// Get all todos with optional filtering, sorting, and pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<TodoListResponse>> GetTodos([FromQuery] TodoQueryParameters queryParams)
    {
        var response = await _todoService.GetAllTodosAsync(queryParams);
        return Ok(response);
    }

    /// <summary>
    /// Get deleted todos with optional filtering, sorting, and pagination
    /// </summary>
    [HttpGet("deleted")]
    public async Task<ActionResult<TodoListResponse>> GetDeletedTodos([FromQuery] TodoQueryParameters queryParams)
    {
        var response = await _todoService.GetDeletedTodosAsync(queryParams);
        return Ok(response);
    }

    /// <summary>
    /// Get a specific todo by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoResponse>> GetTodo(int id)
    {
        var todo = await _todoService.GetTodoByIdAsync(id);

        if (todo == null)
        {
            return NotFound(new { message = $"Todo with ID {id} not found" });
        }

        return Ok(MapToResponse(todo));
    }

    /// <summary>
    /// Create a new todo
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TodoResponse>> CreateTodo([FromBody] CreateTodoRequest request)
    {
        var todo = await _todoService.CreateTodoAsync(request);
        return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, MapToResponse(todo));
    }

    /// <summary>
    /// Update an existing todo
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<TodoResponse>> UpdateTodo(int id, [FromBody] UpdateTodoRequest request)
    {
        var todo = await _todoService.UpdateTodoAsync(id, request);

        if (todo == null)
        {
            return NotFound(new { message = $"Todo with ID {id} not found" });
        }

        return Ok(MapToResponse(todo));
    }

    /// <summary>
    /// Toggle the completion status of a todo
    /// </summary>
    [HttpPatch("{id}/toggle")]
    public async Task<ActionResult<TodoResponse>> ToggleTodo(int id)
    {
        var todo = await _todoService.ToggleTodoAsync(id);

        if (todo == null)
        {
            return NotFound(new { message = $"Todo with ID {id} not found" });
        }

        return Ok(MapToResponse(todo));
    }

    /// <summary>
    /// Delete a todo
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodo(int id)
    {
        var deleted = await _todoService.DeleteTodoAsync(id);

        if (!deleted)
        {
            return NotFound(new { message = $"Todo with ID {id} not found" });
        }

        return NoContent();
    }

    /// <summary>
    /// Restore a deleted todo
    /// </summary>
    [HttpPatch("{id}/restore")]
    public async Task<ActionResult<TodoResponse>> RestoreTodo(int id)
    {
        var todo = await _todoService.RestoreTodoAsync(id);

        if (todo == null)
        {
            return NotFound(new { message = $"Todo with ID {id} not found" });
        }

        return Ok(MapToResponse(todo));
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
}
