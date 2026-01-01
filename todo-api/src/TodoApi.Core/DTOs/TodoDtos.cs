using System.ComponentModel.DataAnnotations;
using TodoApi.Core.Entities;

namespace TodoApi.Core.DTOs;

public class SortByValidationAttribute : ValidationAttribute
{
    private static readonly string[] ValidValues = { "title", "duedate", "priority", "iscompleted", "createdat" };

    // Validate that SortBy is one of the allowed values (case-insensitive)
    // or null/empty (which defaults to createdat).
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return ValidationResult.Success;

        var sortBy = value.ToString()!.ToLower();

        if (ValidValues.Contains(sortBy))
            return ValidationResult.Success;

        return new ValidationResult(
            $"SortBy must be one of: {string.Join(", ", ValidValues)} (case-insensitive)");
    }
}

public record CreateTodoRequest
{
    [Required(ErrorMessage = "Title is required")]
    [MinLength(1, ErrorMessage = "Title cannot be empty")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; init; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; init; }

    public DateTime? DueDate { get; init; }

    [EnumDataType(typeof(Priority), ErrorMessage = "Priority must be Low (0), Medium (1), or High (2)")]
    public Priority Priority { get; init; } = Priority.Medium;
}

public record UpdateTodoRequest
{
    [Required(ErrorMessage = "Title is required")]
    [MinLength(1, ErrorMessage = "Title cannot be empty")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; init; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; init; }

    public bool IsCompleted { get; init; }

    public DateTime? DueDate { get; init; }

    [EnumDataType(typeof(Priority), ErrorMessage = "Priority must be Low (0), Medium (1), or High (2)")]
    public Priority Priority { get; init; }
}

public record TodoQueryParameters
{
    public bool? IsCompleted { get; init; }

    [EnumDataType(typeof(Priority), ErrorMessage = "Priority must be Low (0), Medium (1), or High (2)")]
    public Priority? Priority { get; init; }

    [SortByValidation]
    public string? SortBy { get; init; } = "createdat";

    public bool SortDescending { get; init; } = true;

    [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1")]
    public int Page { get; init; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
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
