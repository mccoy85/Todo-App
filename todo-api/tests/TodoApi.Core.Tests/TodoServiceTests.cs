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

    // Creates todo and trims whitespace from title/description
    [Test]
    public async Task CreateTodoAsync_ShouldCreateTodoSuccessfully()
    {
        var request = new CreateTodoRequest
        {
            Title = "  Test Todo  ",
            Description = "  Test Description  ",
            Priority = Priority.High
        };
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TodoItem>()))
            .ReturnsAsync((TodoItem todo) => { todo.Id = 1; return todo; });

        var result = await _service.CreateTodoAsync(request);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("Test Todo"));
        Assert.That(result.Description, Is.EqualTo("Test Description"));
        Assert.That(result.Priority, Is.EqualTo(Priority.High));
        Assert.That(result.IsCompleted, Is.False);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<TodoItem>()), Times.Once);
    }

    // Returns todo when it exists
    [Test]
    public async Task GetTodoByIdAsync_WhenTodoExists_ShouldReturnTodo()
    {
        var todo = new TodoItem { Id = 1, Title = "Existing Todo", Description = "Description", Priority = Priority.Medium, CreatedAt = DateTime.UtcNow };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(todo);

        var result = await _service.GetTodoByIdAsync(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(1));
        Assert.That(result.Title, Is.EqualTo("Existing Todo"));
    }

    // Returns null when todo doesn't exist
    [Test]
    public async Task GetTodoByIdAsync_WhenTodoDoesNotExist_ShouldReturnNull()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((TodoItem?)null);

        var result = await _service.GetTodoByIdAsync(999);

        Assert.That(result, Is.Null);
    }

    // Updates todo and trims whitespace
    [Test]
    public async Task UpdateTodoAsync_WhenTodoExists_ShouldUpdateSuccessfully()
    {
        var todo = new TodoItem { Id = 1, Title = "Original Title", Description = "Original Description", Priority = Priority.Low, IsCompleted = false, CreatedAt = DateTime.UtcNow };
        var updateRequest = new UpdateTodoRequest { Title = "  Updated Title  ", Description = "  Updated Description  ", Priority = Priority.High, IsCompleted = true };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(todo);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TodoItem>())).ReturnsAsync((TodoItem t) => t);

        var result = await _service.UpdateTodoAsync(1, updateRequest);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Title, Is.EqualTo("Updated Title"));
        Assert.That(result.Description, Is.EqualTo("Updated Description"));
        Assert.That(result.Priority, Is.EqualTo(Priority.High));
        Assert.That(result.IsCompleted, Is.True);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TodoItem>()), Times.Once);
    }

    // Toggles IsCompleted from false to true
    [Test]
    public async Task ToggleTodoAsync_ShouldToggleCompletionStatus()
    {
        var todo = new TodoItem { Id = 1, Title = "Toggle Test", IsCompleted = false, CreatedAt = DateTime.UtcNow };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(todo);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TodoItem>())).ReturnsAsync((TodoItem t) => t);

        var result = await _service.ToggleTodoAsync(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.IsCompleted, Is.True);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TodoItem>()), Times.Once);
    }

    // Returns true when delete succeeds
    [Test]
    public async Task DeleteTodoAsync_WhenTodoExists_ShouldReturnTrue()
    {
        _repositoryMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _service.DeleteTodoAsync(1);

        Assert.That(result, Is.True);
        _repositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    // Returns false when todo doesn't exist
    [Test]
    public async Task DeleteTodoAsync_WhenTodoDoesNotExist_ShouldReturnFalse()
    {
        _repositoryMock.Setup(r => r.DeleteAsync(999)).ReturnsAsync(false);

        var result = await _service.DeleteTodoAsync(999);

        Assert.That(result, Is.False);
    }

    // Filters by priority and completion status
    [Test]
    public async Task GetAllTodosAsync_WithFilters_ShouldReturnFilteredResults()
    {
        var todos = new List<TodoItem> { new() { Id = 1, Title = "High Priority Incomplete", Priority = Priority.High, IsCompleted = false, CreatedAt = DateTime.UtcNow } };
        _repositoryMock.Setup(r => r.GetAllWithFiltersAsync(false, Priority.High, "createdat", true, 1, 10)).ReturnsAsync((todos.AsEnumerable(), 1));
        var queryParams = new TodoQueryParameters { Priority = Priority.High, IsCompleted = false, Page = 1, PageSize = 10 };

        var result = await _service.GetAllTodosAsync(queryParams);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count(), Is.EqualTo(1));
        Assert.That(result.Items.First().Title, Is.EqualTo("High Priority Incomplete"));
        Assert.That(result.TotalCount, Is.EqualTo(1));
    }

    // Returns items sorted by title ascending
    [Test]
    public async Task GetAllTodosAsync_WithSorting_ShouldReturnSortedResults()
    {
        var todos = new List<TodoItem>
        {
            new() { Id = 1, Title = "Apple", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Title = "Mango", CreatedAt = DateTime.UtcNow },
            new() { Id = 3, Title = "Zebra", CreatedAt = DateTime.UtcNow }
        };
        _repositoryMock.Setup(r => r.GetAllWithFiltersAsync(null, null, "title", false, 1, 10)).ReturnsAsync((todos.AsEnumerable(), 3));
        var queryParams = new TodoQueryParameters { SortBy = "title", SortDescending = false, Page = 1, PageSize = 10 };

        var result = await _service.GetAllTodosAsync(queryParams);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count(), Is.EqualTo(3));
        Assert.That(result.Items.First().Title, Is.EqualTo("Apple"));
        Assert.That(result.Items.Last().Title, Is.EqualTo("Zebra"));
    }

    // Returns only soft-deleted todos
    [Test]
    public async Task GetDeletedTodosAsync_ShouldReturnDeletedResults()
    {
        var todos = new List<TodoItem> { new() { Id = 1, Title = "Deleted Todo", IsDeleted = true, CreatedAt = DateTime.UtcNow } };
        _repositoryMock.Setup(r => r.GetDeletedWithFiltersAsync(null, null, "createdat", true, 1, 10)).ReturnsAsync((todos.AsEnumerable(), 1));
        var queryParams = new TodoQueryParameters { Page = 1, PageSize = 10 };

        var result = await _service.GetDeletedTodosAsync(queryParams);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count(), Is.EqualTo(1));
        Assert.That(result.Items.First().Title, Is.EqualTo("Deleted Todo"));
        Assert.That(result.TotalCount, Is.EqualTo(1));
    }

    // Restores soft-deleted todo
    [Test]
    public async Task RestoreTodoAsync_WhenTodoIsDeleted_ShouldReturnTodo()
    {
        var restoredTodo = new TodoItem { Id = 1, Title = "Restore Me", IsDeleted = false, DeletedAt = null, CreatedAt = DateTime.UtcNow };
        _repositoryMock.Setup(r => r.RestoreAsync(1)).ReturnsAsync(restoredTodo);

        var result = await _service.RestoreTodoAsync(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.IsDeleted, Is.False);
        _repositoryMock.Verify(r => r.RestoreAsync(1), Times.Once);
    }
}
