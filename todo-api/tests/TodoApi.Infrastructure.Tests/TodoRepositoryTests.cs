using Microsoft.EntityFrameworkCore;
using TodoApi.Core.Entities;
using TodoApi.Infrastructure.Data;
using TodoApi.Infrastructure.Repositories;

namespace TodoApi.Infrastructure.Tests;

[TestFixture]
public class TodoRepositoryTests
{
    private TodoDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TodoDbContext(options);
    }

    [Test]
    public async Task GetByIdAsync_WhenTodoExists_ShouldReturnTodo()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var todo = new TodoItem
        {
            Title = "Test Todo",
            Description = "Description",
            Priority = Priority.Medium,
            CreatedAt = DateTime.UtcNow
        };
        context.TodoItems.Add(todo);
        await context.SaveChangesAsync();

        var repository = new TodoRepository(context);

        // Act
        var result = await repository.GetByIdAsync(todo.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(todo.Id));
        Assert.That(result.Title, Is.EqualTo("Test Todo"));
    }

    [Test]
    public async Task GetByIdAsync_WhenTodoDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new TodoRepository(context);

        // Act
        var result = await repository.GetByIdAsync(999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task AddAsync_ShouldAddTodoToDatabase()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new TodoRepository(context);

        var todo = new TodoItem
        {
            Title = "New Todo",
            Description = "Description",
            Priority = Priority.High,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await repository.AddAsync(todo);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.GreaterThan(0));
        Assert.That(result.Title, Is.EqualTo("New Todo"));

        var savedTodo = await context.TodoItems.FindAsync(result.Id);
        Assert.That(savedTodo, Is.Not.Null);
    }

    [Test]
    public async Task UpdateAsync_ShouldUpdateExistingTodo()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var todo = new TodoItem
        {
            Title = "Original Title",
            Description = "Original Description",
            Priority = Priority.Low,
            CreatedAt = DateTime.UtcNow
        };
        context.TodoItems.Add(todo);
        await context.SaveChangesAsync();

        var repository = new TodoRepository(context);

        // Detach the entity to simulate a real-world scenario
        context.Entry(todo).State = EntityState.Detached;

        todo.Title = "Updated Title";
        todo.Description = "Updated Description";
        todo.Priority = Priority.High;

        // Act
        var result = await repository.UpdateAsync(todo);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("Updated Title"));
        Assert.That(result.Description, Is.EqualTo("Updated Description"));
        Assert.That(result.Priority, Is.EqualTo(Priority.High));
    }

    [Test]
    public async Task DeleteAsync_WhenTodoExists_ShouldReturnTrue()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var todo = new TodoItem
        {
            Title = "To Delete",
            CreatedAt = DateTime.UtcNow
        };
        context.TodoItems.Add(todo);
        await context.SaveChangesAsync();

        var repository = new TodoRepository(context);

        // Act
        var result = await repository.DeleteAsync(todo.Id);

        // Assert
        Assert.That(result, Is.True);
        var deletedTodo = await context.TodoItems
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(item => item.Id == todo.Id);
        Assert.That(deletedTodo, Is.Not.Null);
        Assert.That(deletedTodo!.IsDeleted, Is.True);
        Assert.That(deletedTodo.DeletedAt, Is.Not.Null);
    }

    [Test]
    public async Task DeleteAsync_WhenTodoDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new TodoRepository(context);

        // Act
        var result = await repository.DeleteAsync(999);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task GetAllWithFiltersAsync_WithCompletedFilter_ShouldReturnOnlyCompletedTodos()
    {
        // Arrange
        var context = CreateInMemoryContext();
        context.TodoItems.AddRange(
            new TodoItem { Title = "Completed", IsCompleted = true, CreatedAt = DateTime.UtcNow },
            new TodoItem { Title = "Incomplete", IsCompleted = false, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var repository = new TodoRepository(context);

        // Act
        var (items, totalCount) = await repository.GetAllWithFiltersAsync(
            isCompleted: true,
            priority: null,
            sortBy: "createdat",
            sortDescending: true,
            page: 1,
            pageSize: 10);

        // Assert
        Assert.That(totalCount, Is.EqualTo(1));
        Assert.That(items.Count(), Is.EqualTo(1));
        Assert.That(items.First().Title, Is.EqualTo("Completed"));
    }

    [Test]
    public async Task GetAllWithFiltersAsync_WithPriorityFilter_ShouldReturnFilteredTodos()
    {
        // Arrange
        var context = CreateInMemoryContext();
        context.TodoItems.AddRange(
            new TodoItem { Title = "High Priority", Priority = Priority.High, CreatedAt = DateTime.UtcNow },
            new TodoItem { Title = "Low Priority", Priority = Priority.Low, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var repository = new TodoRepository(context);

        // Act
        var (items, totalCount) = await repository.GetAllWithFiltersAsync(
            isCompleted: null,
            priority: Priority.High,
            sortBy: "createdat",
            sortDescending: true,
            page: 1,
            pageSize: 10);

        // Assert
        Assert.That(totalCount, Is.EqualTo(1));
        Assert.That(items.Count(), Is.EqualTo(1));
        Assert.That(items.First().Title, Is.EqualTo("High Priority"));
    }

    [Test]
    public async Task GetAllWithFiltersAsync_WithSorting_ShouldReturnSortedResults()
    {
        // Arrange
        var context = CreateInMemoryContext();
        context.TodoItems.AddRange(
            new TodoItem { Title = "Zebra", CreatedAt = DateTime.UtcNow },
            new TodoItem { Title = "Apple", CreatedAt = DateTime.UtcNow },
            new TodoItem { Title = "Mango", CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var repository = new TodoRepository(context);

        // Act
        var (items, totalCount) = await repository.GetAllWithFiltersAsync(
            isCompleted: null,
            priority: null,
            sortBy: "title",
            sortDescending: false,
            page: 1,
            pageSize: 10);

        // Assert
        Assert.That(totalCount, Is.EqualTo(3));
        Assert.That(items.First().Title, Is.EqualTo("Apple"));
        Assert.That(items.Last().Title, Is.EqualTo("Zebra"));
    }

    [Test]
    public async Task GetAllWithFiltersAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var context = CreateInMemoryContext();
        for (int i = 1; i <= 15; i++)
        {
            context.TodoItems.Add(new TodoItem { Title = $"Todo {i}", CreatedAt = DateTime.UtcNow.AddMinutes(i) });
        }
        await context.SaveChangesAsync();

        var repository = new TodoRepository(context);

        // Act - Get second page with page size 5
        var (items, totalCount) = await repository.GetAllWithFiltersAsync(
            isCompleted: null,
            priority: null,
            sortBy: "createdat",
            sortDescending: false,
            page: 2,
            pageSize: 5);

        // Assert
        Assert.That(totalCount, Is.EqualTo(15));
        Assert.That(items.Count(), Is.EqualTo(5));
        Assert.That(items.First().Title, Is.EqualTo("Todo 6"));
        Assert.That(items.Last().Title, Is.EqualTo("Todo 10"));
    }

    [Test]
    public async Task GetDeletedWithFiltersAsync_ShouldReturnOnlyDeletedTodos()
    {
        // Arrange
        var context = CreateInMemoryContext();
        context.TodoItems.AddRange(
            new TodoItem { Title = "Deleted", IsDeleted = true, DeletedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow },
            new TodoItem { Title = "Active", IsDeleted = false, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var repository = new TodoRepository(context);

        // Act
        var (items, totalCount) = await repository.GetDeletedWithFiltersAsync(
            isCompleted: null,
            priority: null,
            sortBy: "createdat",
            sortDescending: true,
            page: 1,
            pageSize: 10);

        // Assert
        Assert.That(totalCount, Is.EqualTo(1));
        Assert.That(items.Count(), Is.EqualTo(1));
        Assert.That(items.First().Title, Is.EqualTo("Deleted"));
    }

    [Test]
    public async Task RestoreAsync_WhenTodoIsDeleted_ShouldRestoreTodo()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var todo = new TodoItem
        {
            Title = "Restore Me",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.TodoItems.Add(todo);
        await context.SaveChangesAsync();

        var repository = new TodoRepository(context);

        // Act
        var result = await repository.RestoreAsync(todo.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.IsDeleted, Is.False);
        Assert.That(result.DeletedAt, Is.Null);
    }
}
