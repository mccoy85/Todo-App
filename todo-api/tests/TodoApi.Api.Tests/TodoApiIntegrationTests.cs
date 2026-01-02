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

    // Returns empty list when no todos exist
    [Test]
    public async Task GetTodos_ShouldReturnEmptyList_WhenNoTodosExist()
    {
        var response = await _client.GetAsync("/api/todo");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<TodoListResponse>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.TotalCount, Is.EqualTo(0));
    }

    // Creates a todo and returns it with 201 status
    [Test]
    public async Task CreateTodo_ShouldReturnCreatedTodo()
    {
        var request = new CreateTodoRequest
        {
            Title = "Test Todo",
            Description = "Test Description",
            Priority = Priority.High
        };

        var response = await _client.PostAsJsonAsync("/api/todo", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var result = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Title, Is.EqualTo("Test Todo"));
        Assert.That(result.Description, Is.EqualTo("Test Description"));
        Assert.That(result.Priority, Is.EqualTo(Priority.High));
    }

    // Returns 400 when title is empty
    [Test]
    public async Task CreateTodo_ShouldReturnBadRequest_WhenTitleIsEmpty()
    {
        var request = new CreateTodoRequest
        {
            Title = "",
            Description = "Test Description"
        };

        var response = await _client.PostAsJsonAsync("/api/todo", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    // Returns a todo by ID when it exists
    [Test]
    public async Task GetTodoById_ShouldReturnTodo_WhenExists()
    {
        var createRequest = new CreateTodoRequest { Title = "Test Todo", Priority = Priority.Medium };
        var createResponse = await _client.PostAsJsonAsync("/api/todo", createRequest);
        var createdTodo = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();

        var response = await _client.GetAsync($"/api/todo/{createdTodo!.Id}");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(createdTodo.Id));
        Assert.That(result.Title, Is.EqualTo("Test Todo"));
    }

    // Returns 404 when todo doesn't exist
    [Test]
    public async Task GetTodoById_ShouldReturnNotFound_WhenDoesNotExist()
    {
        var response = await _client.GetAsync("/api/todo/99999");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    // Updates a todo and returns the updated version
    [Test]
    public async Task UpdateTodo_ShouldReturnUpdatedTodo()
    {
        var createRequest = new CreateTodoRequest { Title = "Original Title", Priority = Priority.Low };
        var createResponse = await _client.PostAsJsonAsync("/api/todo", createRequest);
        var createdTodo = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();

        var updateRequest = new UpdateTodoRequest
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Priority = Priority.High,
            IsCompleted = true
        };

        var response = await _client.PutAsJsonAsync($"/api/todo/{createdTodo!.Id}", updateRequest);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Title, Is.EqualTo("Updated Title"));
        Assert.That(result.Description, Is.EqualTo("Updated Description"));
        Assert.That(result.Priority, Is.EqualTo(Priority.High));
        Assert.That(result.IsCompleted, Is.True);
    }

    // Returns 404 when updating non-existent todo
    [Test]
    public async Task UpdateTodo_ShouldReturnNotFound_WhenDoesNotExist()
    {
        var updateRequest = new UpdateTodoRequest { Title = "Updated Title", Priority = Priority.High };

        var response = await _client.PutAsJsonAsync("/api/todo/99999", updateRequest);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    // Toggles completion status from false to true and back
    [Test]
    public async Task ToggleTodo_ShouldToggleCompletionStatus()
    {
        var createRequest = new CreateTodoRequest { Title = "Test Todo", Priority = Priority.Medium };
        var createResponse = await _client.PostAsJsonAsync("/api/todo", createRequest);
        var createdTodo = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.That(createdTodo!.IsCompleted, Is.False);

        var response = await _client.PatchAsync($"/api/todo/{createdTodo.Id}/toggle", null);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.IsCompleted, Is.True);

        response = await _client.PatchAsync($"/api/todo/{createdTodo.Id}/toggle", null);

        response.EnsureSuccessStatusCode();
        result = await response.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.That(result!.IsCompleted, Is.False);
    }

    // Soft deletes a todo and verifies it's no longer accessible
    [Test]
    public async Task DeleteTodo_ShouldReturnNoContent()
    {
        var createRequest = new CreateTodoRequest { Title = "Test Todo", Priority = Priority.Medium };
        var createResponse = await _client.PostAsJsonAsync("/api/todo", createRequest);
        var createdTodo = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();

        var response = await _client.DeleteAsync($"/api/todo/{createdTodo!.Id}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        var getResponse = await _client.GetAsync($"/api/todo/{createdTodo.Id}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    // Returns 404 when deleting non-existent todo
    [Test]
    public async Task DeleteTodo_ShouldReturnNotFound_WhenDoesNotExist()
    {
        var response = await _client.DeleteAsync("/api/todo/99999");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    // Returns soft-deleted todos in the deleted endpoint
    [Test]
    public async Task GetDeletedTodos_ShouldReturnDeletedItems()
    {
        var createRequest = new CreateTodoRequest { Title = "Deleted Todo", Priority = Priority.Medium };
        var createResponse = await _client.PostAsJsonAsync("/api/todo", createRequest);
        var createdTodo = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();
        await _client.DeleteAsync($"/api/todo/{createdTodo!.Id}");

        var response = await _client.GetAsync("/api/todo/deleted");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<TodoListResponse>();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.TotalCount, Is.EqualTo(1));
        Assert.That(result.Items.First().Id, Is.EqualTo(createdTodo.Id));
    }

    // Restores a soft-deleted todo and makes it accessible again
    [Test]
    public async Task RestoreTodo_ShouldReturnRestoredTodo()
    {
        var createRequest = new CreateTodoRequest { Title = "Restore Todo", Priority = Priority.Low };
        var createResponse = await _client.PostAsJsonAsync("/api/todo", createRequest);
        var createdTodo = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();
        await _client.DeleteAsync($"/api/todo/{createdTodo!.Id}");

        var restoreResponse = await _client.PatchAsync($"/api/todo/{createdTodo.Id}/restore", null);

        restoreResponse.EnsureSuccessStatusCode();
        var restoredTodo = await restoreResponse.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.That(restoredTodo, Is.Not.Null);
        Assert.That(restoredTodo!.Id, Is.EqualTo(createdTodo.Id));
        var getResponse = await _client.GetAsync($"/api/todo/{createdTodo.Id}");
        getResponse.EnsureSuccessStatusCode();
    }
}
