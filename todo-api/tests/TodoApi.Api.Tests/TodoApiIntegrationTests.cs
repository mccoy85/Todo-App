using System.Net;
using System.Net.Http.Json;
using TodoApi.Core.DTOs;
using TodoApi.Core.Entities;

namespace TodoApi.Api.Tests;

[TestFixture]
public class TodoApiIntegrationTests
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
        _factory.EnsureDatabaseCreated();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Test]
    public async Task GetTodos_ShouldReturnEmptyList_WhenNoTodosExist()
    {
        // Act
        var response = await _client.GetAsync("/api/todo");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<TodoListResponse>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.TotalCount, Is.EqualTo(0));
    }

    [Test]
    public async Task CreateTodo_ShouldReturnCreatedTodo()
    {
        // Arrange
        var request = new CreateTodoRequest
        {
            Title = "Test Todo",
            Description = "Test Description",
            Priority = Priority.High
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/todo", request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var result = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Title, Is.EqualTo("Test Todo"));
        Assert.That(result.Description, Is.EqualTo("Test Description"));
        Assert.That(result.Priority, Is.EqualTo(Priority.High));
    }

    [Test]
    public async Task CreateTodo_ShouldReturnBadRequest_WhenTitleIsEmpty()
    {
        // Arrange
        var request = new CreateTodoRequest
        {
            Title = "",
            Description = "Test Description"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/todo", request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetTodoById_ShouldReturnTodo_WhenExists()
    {
        // Arrange - Create a todo first
        var createRequest = new CreateTodoRequest
        {
            Title = "Test Todo",
            Priority = Priority.Medium
        };
        var createResponse = await _client.PostAsJsonAsync("/api/todo", createRequest);
        var createdTodo = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();

        // Act
        var response = await _client.GetAsync($"/api/todo/{createdTodo!.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(createdTodo.Id));
        Assert.That(result.Title, Is.EqualTo("Test Todo"));
    }

    [Test]
    public async Task GetTodoById_ShouldReturnNotFound_WhenDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/api/todo/99999");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task UpdateTodo_ShouldReturnUpdatedTodo()
    {
        // Arrange - Create a todo first
        var createRequest = new CreateTodoRequest
        {
            Title = "Original Title",
            Priority = Priority.Low
        };
        var createResponse = await _client.PostAsJsonAsync("/api/todo", createRequest);
        var createdTodo = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();

        var updateRequest = new UpdateTodoRequest
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Priority = Priority.High,
            IsCompleted = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/todo/{createdTodo!.Id}", updateRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Title, Is.EqualTo("Updated Title"));
        Assert.That(result.Description, Is.EqualTo("Updated Description"));
        Assert.That(result.Priority, Is.EqualTo(Priority.High));
        Assert.That(result.IsCompleted, Is.True);
    }

    [Test]
    public async Task UpdateTodo_ShouldReturnNotFound_WhenDoesNotExist()
    {
        // Arrange
        var updateRequest = new UpdateTodoRequest
        {
            Title = "Updated Title",
            Priority = Priority.High
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/todo/99999", updateRequest);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task ToggleTodo_ShouldToggleCompletionStatus()
    {
        // Arrange - Create a todo first
        var createRequest = new CreateTodoRequest
        {
            Title = "Test Todo",
            Priority = Priority.Medium
        };
        var createResponse = await _client.PostAsJsonAsync("/api/todo", createRequest);
        var createdTodo = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.That(createdTodo!.IsCompleted, Is.False);

        // Act - Toggle to complete
        var response = await _client.PatchAsync($"/api/todo/{createdTodo.Id}/toggle", null);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.IsCompleted, Is.True);

        // Act - Toggle back to incomplete
        response = await _client.PatchAsync($"/api/todo/{createdTodo.Id}/toggle", null);

        // Assert
        response.EnsureSuccessStatusCode();
        result = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.That(result!.IsCompleted, Is.False);
    }

    [Test]
    public async Task DeleteTodo_ShouldReturnNoContent()
    {
        // Arrange - Create a todo first
        var createRequest = new CreateTodoRequest
        {
            Title = "Test Todo",
            Priority = Priority.Medium
        };
        var createResponse = await _client.PostAsJsonAsync("/api/todo", createRequest);
        var createdTodo = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();

        // Act
        var response = await _client.DeleteAsync($"/api/todo/{createdTodo!.Id}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Verify it's actually deleted
        var getResponse = await _client.GetAsync($"/api/todo/{createdTodo.Id}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task DeleteTodo_ShouldReturnNotFound_WhenDoesNotExist()
    {
        // Act
        var response = await _client.DeleteAsync("/api/todo/99999");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetDeletedTodos_ShouldReturnDeletedItems()
    {
        // Arrange
        var createRequest = new CreateTodoRequest
        {
            Title = "Deleted Todo",
            Priority = Priority.Medium
        };
        var createResponse = await _client.PostAsJsonAsync("/api/todo", createRequest);
        var createdTodo = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();

        await _client.DeleteAsync($"/api/todo/{createdTodo!.Id}");

        // Act
        var response = await _client.GetAsync("/api/todo/deleted");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<TodoListResponse>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.TotalCount, Is.EqualTo(1));
        Assert.That(result.Items.First().Id, Is.EqualTo(createdTodo.Id));
    }

    [Test]
    public async Task RestoreTodo_ShouldReturnRestoredTodo()
    {
        // Arrange
        var createRequest = new CreateTodoRequest
        {
            Title = "Restore Todo",
            Priority = Priority.Low
        };
        var createResponse = await _client.PostAsJsonAsync("/api/todo", createRequest);
        var createdTodo = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();

        await _client.DeleteAsync($"/api/todo/{createdTodo!.Id}");

        // Act
        var restoreResponse = await _client.PatchAsync($"/api/todo/{createdTodo.Id}/restore", null);

        // Assert
        restoreResponse.EnsureSuccessStatusCode();
        var restoredTodo = await restoreResponse.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.That(restoredTodo, Is.Not.Null);
        Assert.That(restoredTodo!.Id, Is.EqualTo(createdTodo.Id));

        var getResponse = await _client.GetAsync($"/api/todo/{createdTodo.Id}");
        getResponse.EnsureSuccessStatusCode();
    }
}
