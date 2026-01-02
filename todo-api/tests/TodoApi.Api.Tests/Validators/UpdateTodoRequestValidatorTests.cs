using FluentValidation.Results;
using TodoApi.Api.Validators;
using TodoApi.Core.DTOs;
using TodoApi.Core.Entities;
using System.Linq;

namespace TodoApi.Api.Tests.Validators;

[TestFixture]
public class UpdateTodoRequestValidatorTests
{
    private UpdateTodoRequestValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new UpdateTodoRequestValidator();
    }

    // Valid request with all fields passes validation
    [Test]
    public void Should_Pass_For_Valid_Request()
    {
        var request = new UpdateTodoRequest
        {
            Title = "Valid title",
            Description = "Valid description",
            Priority = Priority.High,
            IsCompleted = true,
            DueDate = DateTime.UtcNow.AddDays(2)
        };

        var result = _validator.Validate(request);

        Assert.That(result.IsValid, Is.True);
    }

    // Whitespace-only title fails validation
    [Test]
    public void Should_Fail_When_Title_Is_Whitespace()
    {
        var request = new UpdateTodoRequest { Title = "   ", Priority = Priority.Low, IsCompleted = false };

        var result = _validator.Validate(request);

        Assert.That(HasErrorFor(result, nameof(UpdateTodoRequest.Title)), Is.True);
    }

    // Past due date fails validation
    [Test]
    public void Should_Fail_When_DueDate_Is_In_Past()
    {
        var request = new UpdateTodoRequest
        {
            Title = "Valid title",
            Priority = Priority.Medium,
            IsCompleted = false,
            DueDate = DateTime.UtcNow.AddDays(-3)
        };

        var result = _validator.Validate(request);

        Assert.That(HasErrorFor(result, nameof(UpdateTodoRequest.DueDate)), Is.True);
    }

    // Description over 1000 chars fails validation
    [Test]
    public void Should_Fail_When_Description_Too_Long()
    {
        var request = new UpdateTodoRequest
        {
            Title = "Valid title",
            Priority = Priority.Low,
            IsCompleted = false,
            Description = new string('a', 1001)
        };

        var result = _validator.Validate(request);

        Assert.That(HasErrorFor(result, nameof(UpdateTodoRequest.Description)), Is.True);
    }

    // Invalid enum value for priority fails validation
    [Test]
    public void Should_Fail_When_Priority_Invalid()
    {
        var request = new UpdateTodoRequest { Title = "Valid title", Priority = (Priority)99, IsCompleted = false };

        var result = _validator.Validate(request);

        Assert.That(HasErrorFor(result, nameof(UpdateTodoRequest.Priority)), Is.True);
    }

    private static bool HasErrorFor(ValidationResult result, string propertyName)
    {
        return result.Errors.Any(error => error.PropertyName == propertyName);
    }
}
