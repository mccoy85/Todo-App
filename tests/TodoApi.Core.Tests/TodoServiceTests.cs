using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Legacy;
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
            Title = "Test Todo",
            Description = "Test Description",
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
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual("Test Todo", result.Title);
        ClassicAssert.AreEqual("Test Description", result.Description);
        ClassicAssert.AreEqual(Priority.High, result.Priority);
        ClassicAssert.IsFalse(result.IsCompleted);
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
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(1, result!.Id);
        ClassicAssert.AreEqual("Existing Todo", result.Title);
    }

    [Test]
    public async Task GetTodoByIdAsync_WhenTodoDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((TodoItem?)null);

        // Act
        var result = await _service.GetTodoByIdAsync(999);

        // Assert
        ClassicAssert.IsNull(result);
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
            Title = "Updated Title",
            Description = "Updated Description",
            Priority = Priority.High,
            IsCompleted = true
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(todo);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TodoItem>())).ReturnsAsync((TodoItem t) => t);

        // Act
        var result = await _service.UpdateTodoAsync(1, updateRequest);

        // Assert
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual("Updated Title", result!.Title);
        ClassicAssert.AreEqual("Updated Description", result.Description);
        ClassicAssert.AreEqual(Priority.High, result.Priority);
        ClassicAssert.IsTrue(result.IsCompleted);
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
        ClassicAssert.IsNotNull(result);
        ClassicAssert.IsTrue(result!.IsCompleted);
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
        ClassicAssert.IsTrue(result);
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
        ClassicAssert.IsFalse(result);
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
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(1, result.Items.Count());
        ClassicAssert.AreEqual("High Priority Incomplete", result.Items.First().Title);
        ClassicAssert.AreEqual(1, result.TotalCount);
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
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(3, result.Items.Count());
        ClassicAssert.AreEqual("Apple", result.Items.First().Title);
        ClassicAssert.AreEqual("Zebra", result.Items.Last().Title);
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
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(1, result.Items.Count());
        ClassicAssert.AreEqual("Deleted Todo", result.Items.First().Title);
        ClassicAssert.AreEqual(1, result.TotalCount);
    }

    [Test]
    public async Task RestoreTodoAsync_WhenTodoIsDeleted_ShouldReturnTodo()
    {
        // Arrange
        var todo = new TodoItem
        {
            Id = 1,
            Title = "Restore Me",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock.Setup(r => r.RestoreAsync(1)).ReturnsAsync(todo);

        // Act
        var result = await _service.RestoreTodoAsync(1);

        // Assert
        ClassicAssert.IsNotNull(result);
        ClassicAssert.IsFalse(result!.IsDeleted);
        _repositoryMock.Verify(r => r.RestoreAsync(1), Times.Once);
    }
}
