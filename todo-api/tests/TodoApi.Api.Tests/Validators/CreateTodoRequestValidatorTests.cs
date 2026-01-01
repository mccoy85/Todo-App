using FluentValidation.Results;
using TodoApi.Api.Validators;
using TodoApi.Core.DTOs;
using TodoApi.Core.Entities;
using System.Linq;

namespace TodoApi.Api.Tests.Validators;

[TestFixture]
public class CreateTodoRequestValidatorTests
{
    private CreateTodoRequestValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateTodoRequestValidator();
    }

    [Test]
    public void Should_Pass_For_Valid_Request()
    {
        var request = new CreateTodoRequest
        {
            Title = "Valid title",
            Description = "Valid description",
            Priority = Priority.Medium,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        var result = _validator.Validate(request);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Should_Fail_When_Title_Is_Whitespace()
    {
        var request = new CreateTodoRequest
        {
            Title = "   ",
            Priority = Priority.Low
        };

        var result = _validator.Validate(request);

        Assert.That(HasErrorFor(result, nameof(CreateTodoRequest.Title)), Is.True);
    }

    [Test]
    public void Should_Fail_When_DueDate_Is_In_Past()
    {
        var request = new CreateTodoRequest
        {
            Title = "Valid title",
            DueDate = DateTime.UtcNow.AddDays(-1)
        };

        var result = _validator.Validate(request);

        Assert.That(HasErrorFor(result, nameof(CreateTodoRequest.DueDate)), Is.True);
    }

    [Test]
    public void Should_Fail_When_Description_Too_Long()
    {
        var request = new CreateTodoRequest
        {
            Title = "Valid title",
            Description = new string('a', 1001)
        };

        var result = _validator.Validate(request);

        Assert.That(HasErrorFor(result, nameof(CreateTodoRequest.Description)), Is.True);
    }

    [Test]
    public void Should_Fail_When_Priority_Invalid()
    {
        var request = new CreateTodoRequest
        {
            Title = "Valid title",
            Priority = (Priority)99
        };

        var result = _validator.Validate(request);

        Assert.That(HasErrorFor(result, nameof(CreateTodoRequest.Priority)), Is.True);
    }

    private static bool HasErrorFor(ValidationResult result, string propertyName)
    {
        return result.Errors.Any(error => error.PropertyName == propertyName);
    }
}
