using System.Net;
using System.Net.Http.Json;
using NUnit.Framework.Legacy;
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
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(0, result!.TotalCount);
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
        ClassicAssert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TodoResponse>();
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual("Test Todo", result!.Title);
        ClassicAssert.AreEqual("Test Description", result.Description);
        ClassicAssert.AreEqual(Priority.High, result.Priority);
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
        ClassicAssert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
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
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(createdTodo.Id, result!.Id);
        ClassicAssert.AreEqual("Test Todo", result.Title);
    }

    [Test]
    public async Task GetTodoById_ShouldReturnNotFound_WhenDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/api/todo/99999");

        // Assert
        ClassicAssert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
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
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual("Updated Title", result!.Title);
        ClassicAssert.AreEqual("Updated Description", result.Description);
        ClassicAssert.AreEqual(Priority.High, result.Priority);
        ClassicAssert.IsTrue(result.IsCompleted);
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
        ClassicAssert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
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
        ClassicAssert.IsFalse(createdTodo!.IsCompleted);

        // Act - Toggle to complete
        var response = await _client.PatchAsync($"/api/todo/{createdTodo.Id}/toggle", null);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<TodoResponse>();
        ClassicAssert.IsNotNull(result);
        ClassicAssert.IsTrue(result!.IsCompleted);

        // Act - Toggle back to incomplete
        response = await _client.PatchAsync($"/api/todo/{createdTodo.Id}/toggle", null);

        // Assert
        response.EnsureSuccessStatusCode();
        result = await response.Content.ReadFromJsonAsync<TodoResponse>();
        ClassicAssert.IsFalse(result!.IsCompleted);
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
        ClassicAssert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

        // Verify it's actually deleted
        var getResponse = await _client.GetAsync($"/api/todo/{createdTodo.Id}");
        ClassicAssert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Test]
    public async Task DeleteTodo_ShouldReturnNotFound_WhenDoesNotExist()
    {
        // Act
        var response = await _client.DeleteAsync("/api/todo/99999");

        // Assert
        ClassicAssert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
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
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(1, result!.TotalCount);
        ClassicAssert.AreEqual(createdTodo.Id, result.Items.First().Id);
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
        ClassicAssert.IsNotNull(restoredTodo);
        ClassicAssert.AreEqual(createdTodo.Id, restoredTodo!.Id);

        var getResponse = await _client.GetAsync($"/api/todo/{createdTodo.Id}");
        getResponse.EnsureSuccessStatusCode();
    }
}
