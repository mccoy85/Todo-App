using FluentValidation.Results;
using TodoApi.Api.Validators;
using TodoApi.Core.DTOs;
using TodoApi.Core.Entities;
using System.Linq;

namespace TodoApi.Api.Tests.Validators;

[TestFixture]
public class TodoQueryParametersValidatorTests
{
    private TodoQueryParametersValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new TodoQueryParametersValidator();
    }

    // Valid request with all fields passes validation
    [Test]
    public void Should_Pass_For_Valid_Request()
    {
        var request = new TodoQueryParameters
        {
            IsCompleted = true,
            Priority = Priority.High,
            SortBy = "title",
            SortDescending = false,
            Page = 1,
            PageSize = 10
        };

        var result = _validator.Validate(request);

        Assert.That(result.IsValid, Is.True);
    }

    // Default values pass validation
    [Test]
    public void Should_Pass_For_Default_Values()
    {
        var request = new TodoQueryParameters();

        var result = _validator.Validate(request);

        Assert.That(result.IsValid, Is.True);
    }

    // Invalid enum value for priority fails validation
    [Test]
    public void Should_Fail_When_Priority_Invalid()
    {
        var request = new TodoQueryParameters { Priority = (Priority)99 };

        var result = _validator.Validate(request);

        Assert.That(HasErrorFor(result, nameof(TodoQueryParameters.Priority)), Is.True);
    }

    // Invalid sort field fails validation
    [Test]
    public void Should_Fail_When_SortBy_Invalid()
    {
        var request = new TodoQueryParameters { SortBy = "invalidfield" };

        var result = _validator.Validate(request);

        Assert.That(HasErrorFor(result, nameof(TodoQueryParameters.SortBy)), Is.True);
    }

    // SortBy is case-insensitive
    [Test]
    public void Should_Pass_When_SortBy_Is_Valid_CaseInsensitive()
    {
        var request = new TodoQueryParameters { SortBy = "DueDate" };

        var result = _validator.Validate(request);

        Assert.That(result.IsValid, Is.True);
    }

    // Page=0 fails validation
    [Test]
    public void Should_Fail_When_Page_Is_Zero()
    {
        var request = new TodoQueryParameters { Page = 0 };

        var result = _validator.Validate(request);

        Assert.That(HasErrorFor(result, nameof(TodoQueryParameters.Page)), Is.True);
    }

    // Negative page fails validation
    [Test]
    public void Should_Fail_When_Page_Is_Negative()
    {
        var request = new TodoQueryParameters { Page = -1 };

        var result = _validator.Validate(request);

        Assert.That(HasErrorFor(result, nameof(TodoQueryParameters.Page)), Is.True);
    }

    // PageSize=0 fails validation
    [Test]
    public void Should_Fail_When_PageSize_Is_Zero()
    {
        var request = new TodoQueryParameters { PageSize = 0 };

        var result = _validator.Validate(request);

        Assert.That(HasErrorFor(result, nameof(TodoQueryParameters.PageSize)), Is.True);
    }

    // PageSize over 100 fails validation
    [Test]
    public void Should_Fail_When_PageSize_Exceeds_Maximum()
    {
        var request = new TodoQueryParameters { PageSize = 101 };

        var result = _validator.Validate(request);

        Assert.That(HasErrorFor(result, nameof(TodoQueryParameters.PageSize)), Is.True);
    }

    private static bool HasErrorFor(ValidationResult result, string propertyName)
    {
        return result.Errors.Any(error => error.PropertyName == propertyName);
    }
}
