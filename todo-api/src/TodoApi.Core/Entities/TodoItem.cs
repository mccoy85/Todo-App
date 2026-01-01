using System.ComponentModel.DataAnnotations;

using TodoApi.Core.Interfaces;

namespace TodoApi.Core.Entities;

public class TodoItem : ISoftDeletable
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DueDate { get; set; }

    public Priority Priority { get; set; } = Priority.Medium;

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }
}

public enum Priority
{
    Low = 0,
    Medium = 1,
    High = 2
}
