using FluentValidation;
using TodoApi.Core.DTOs;
using TodoApi.Core.Entities;

namespace TodoApi.Api.Validators;

public class TodoQueryParametersValidator : AbstractValidator<TodoQueryParameters>
{
    private static readonly string[] ValidSortByValues = { "title", "duedate", "priority", "iscompleted", "createdat" };

    public TodoQueryParametersValidator()
    {
        RuleFor(x => x.Priority)
            .IsInEnum()
            .When(x => x.Priority.HasValue)
            .WithMessage("Priority must be Low (0), Medium (1), or High (2)");

        RuleFor(x => x.SortBy)
            .Must(sortBy => string.IsNullOrWhiteSpace(sortBy) || ValidSortByValues.Contains(sortBy.ToLower()))
            .WithMessage($"SortBy must be one of: {string.Join(", ", ValidSortByValues)} (case-insensitive)");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100");
    }
}
