using TodoApi.Core.Entities;

namespace TodoApi.Core.DTOs;

public record CreateTodoRequest
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime? DueDate { get; init; }
    public Priority Priority { get; init; } = Priority.Medium;
}

public record UpdateTodoRequest
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsCompleted { get; init; }
    public DateTime? DueDate { get; init; }
    public Priority Priority { get; init; }
}

public record TodoQueryParameters
{
    public bool? IsCompleted { get; init; }
    public Priority? Priority { get; init; }
    public string? SortBy { get; init; } = "createdat";
    public bool SortDescending { get; init; } = true;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public record TodoResponse
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsCompleted { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? DueDate { get; init; }
    public Priority Priority { get; init; }
}

public record TodoListResponse
{
    public IEnumerable<TodoResponse> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}
