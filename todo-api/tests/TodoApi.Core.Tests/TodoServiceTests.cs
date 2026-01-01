using Microsoft.Extensions.Logging;
using Moq;
using TodoApi.Core.DTOs;
using TodoApi.Core.Entities;
using TodoApi.Core.Interfaces;
using TodoApi.Core.Services;

namespace TodoApi.Core.Tests;

[TestFixture]
public class TodoServiceTests
{
    private Mock<ITodoRepository> _repositoryMock = null!;
    private Mock<ILogger<TodoService>> _loggerMock = null!;
    private TodoService _service = null!;

    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<ITodoRepository>();
        _loggerMock = new Mock<ILogger<TodoService>>();
        _service = new TodoService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task CreateTodoAsync_ShouldCreateTodoSuccessfully()
    {
        // Arrange
        var request = new CreateTodoRequest
        {
            Title = "  Test Todo  ",
            Description = "  Test Description  ",
            Priority = Priority.High
        };

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TodoItem>()))
            .ReturnsAsync((TodoItem todo) =>
            {
                todo.Id = 1;
                return todo;
            });

        // Act
        var result = await _service.CreateTodoAsync(request);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("Test Todo"));
        Assert.That(result.Description, Is.EqualTo("Test Description"));
        Assert.That(result.Priority, Is.EqualTo(Priority.High));
        Assert.That(result.IsCompleted, Is.False);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<TodoItem>()), Times.Once);
    }


    [Test]
    public async Task GetTodoByIdAsync_WhenTodoExists_ShouldReturnTodo()
    {
        // Arrange
        var todo = new TodoItem
        {
            Id = 1,
            Title = "Existing Todo",
            Description = "Description",
            Priority = Priority.Medium,
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(todo);

        // Act
        var result = await _service.GetTodoByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(1));
        Assert.That(result.Title, Is.EqualTo("Existing Todo"));
    }

    [Test]
    public async Task GetTodoByIdAsync_WhenTodoDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((TodoItem?)null);

        // Act
        var result = await _service.GetTodoByIdAsync(999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateTodoAsync_WhenTodoExists_ShouldUpdateSuccessfully()
    {
        // Arrange
        var todo = new TodoItem
        {
            Id = 1,
            Title = "Original Title",
            Description = "Original Description",
            Priority = Priority.Low,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        var updateRequest = new UpdateTodoRequest
        {
            Title = "  Updated Title  ",
            Description = "  Updated Description  ",
            Priority = Priority.High,
            IsCompleted = true
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(todo);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TodoItem>())).ReturnsAsync((TodoItem t) => t);

        // Act
        var result = await _service.UpdateTodoAsync(1, updateRequest);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Title, Is.EqualTo("Updated Title"));
        Assert.That(result.Description, Is.EqualTo("Updated Description"));
        Assert.That(result.Priority, Is.EqualTo(Priority.High));
        Assert.That(result.IsCompleted, Is.True);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TodoItem>()), Times.Once);
    }


    [Test]
    public async Task ToggleTodoAsync_ShouldToggleCompletionStatus()
    {
        // Arrange
        var todo = new TodoItem
        {
            Id = 1,
            Title = "Toggle Test",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(todo);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TodoItem>())).ReturnsAsync((TodoItem t) => t);

        // Act
        var result = await _service.ToggleTodoAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.IsCompleted, Is.True);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TodoItem>()), Times.Once);
    }

    [Test]
    public async Task DeleteTodoAsync_WhenTodoExists_ShouldReturnTrue()
    {
        // Arrange
        _repositoryMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteTodoAsync(1);

        // Assert
        Assert.That(result, Is.True);
        _repositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Test]
    public async Task DeleteTodoAsync_WhenTodoDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        _repositoryMock.Setup(r => r.DeleteAsync(999)).ReturnsAsync(false);

        // Act
        var result = await _service.DeleteTodoAsync(999);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task GetAllTodosAsync_WithFilters_ShouldReturnFilteredResults()
    {
        // Arrange
        var todos = new List<TodoItem>
        {
            new() { Id = 1, Title = "High Priority Incomplete", Priority = Priority.High, IsCompleted = false, CreatedAt = DateTime.UtcNow }
        };

        _repositoryMock
            .Setup(r => r.GetAllWithFiltersAsync(false, Priority.High, "createdat", true, 1, 10))
            .ReturnsAsync((todos.AsEnumerable(), 1));

        var queryParams = new TodoQueryParameters
        {
            Priority = Priority.High,
            IsCompleted = false,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _service.GetAllTodosAsync(queryParams);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count(), Is.EqualTo(1));
        Assert.That(result.Items.First().Title, Is.EqualTo("High Priority Incomplete"));
        Assert.That(result.TotalCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAllTodosAsync_WithSorting_ShouldReturnSortedResults()
    {
        // Arrange
        var todos = new List<TodoItem>
        {
            new() { Id = 1, Title = "Apple", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Title = "Mango", CreatedAt = DateTime.UtcNow },
            new() { Id = 3, Title = "Zebra", CreatedAt = DateTime.UtcNow }
        };

        _repositoryMock
            .Setup(r => r.GetAllWithFiltersAsync(null, null, "title", false, 1, 10))
            .ReturnsAsync((todos.AsEnumerable(), 3));

        var queryParams = new TodoQueryParameters
        {
            SortBy = "title",
            SortDescending = false,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _service.GetAllTodosAsync(queryParams);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count(), Is.EqualTo(3));
        Assert.That(result.Items.First().Title, Is.EqualTo("Apple"));
        Assert.That(result.Items.Last().Title, Is.EqualTo("Zebra"));
    }

    [Test]
    public async Task GetDeletedTodosAsync_ShouldReturnDeletedResults()
    {
        // Arrange
        var todos = new List<TodoItem>
        {
            new() { Id = 1, Title = "Deleted Todo", IsDeleted = true, CreatedAt = DateTime.UtcNow }
        };

        _repositoryMock
            .Setup(r => r.GetDeletedWithFiltersAsync(null, null, "createdat", true, 1, 10))
            .ReturnsAsync((todos.AsEnumerable(), 1));

        var queryParams = new TodoQueryParameters
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _service.GetDeletedTodosAsync(queryParams);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count(), Is.EqualTo(1));
        Assert.That(result.Items.First().Title, Is.EqualTo("Deleted Todo"));
        Assert.That(result.TotalCount, Is.EqualTo(1));
    }

    [Test]
    public async Task RestoreTodoAsync_WhenTodoIsDeleted_ShouldReturnTodo()
    {
        // Arrange
        var restoredTodo = new TodoItem
        {
            Id = 1,
            Title = "Restore Me",
            IsDeleted = false,
            DeletedAt = null,
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock.Setup(r => r.RestoreAsync(1)).ReturnsAsync(restoredTodo);

        // Act
        var result = await _service.RestoreTodoAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.IsDeleted, Is.False);
        _repositoryMock.Verify(r => r.RestoreAsync(1), Times.Once);
    }
}
