using FluentValidation;
using TodoApi.Core.DTOs;

namespace TodoApi.Api.Validators;

// FluentValidation rules for creating todos.
public class CreateTodoRequestValidator : AbstractValidator<CreateTodoRequest>
{
    public CreateTodoRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .MaximumLength(200)
            .WithMessage("Title is required and cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => x.Description != null)
            .WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.DueDate)
            .Must(ValidationHelpers.BeTodayOrLater)
            .When(x => x.DueDate.HasValue)
            .WithMessage("Due date must be today or later");

        RuleFor(x => x.Priority)
            .IsInEnum()
            .WithMessage("Priority must be Low, Medium, or High");
    }
}
