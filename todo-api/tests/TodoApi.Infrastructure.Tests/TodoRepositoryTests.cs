using Microsoft.EntityFrameworkCore;
using NUnit.Framework.Legacy;
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
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(todo.Id, result!.Id);
        ClassicAssert.AreEqual("Test Todo", result.Title);
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
        ClassicAssert.IsNull(result);
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
        ClassicAssert.IsNotNull(result);
        ClassicAssert.IsTrue(result.Id > 0);
        ClassicAssert.AreEqual("New Todo", result.Title);

        var savedTodo = await context.TodoItems.FindAsync(result.Id);
        ClassicAssert.IsNotNull(savedTodo);
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
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual("Updated Title", result.Title);
        ClassicAssert.AreEqual("Updated Description", result.Description);
        ClassicAssert.AreEqual(Priority.High, result.Priority);
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
        ClassicAssert.IsTrue(result);
        var deletedTodo = await context.TodoItems
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(item => item.Id == todo.Id);
        ClassicAssert.IsNotNull(deletedTodo);
        ClassicAssert.IsTrue(deletedTodo!.IsDeleted);
        ClassicAssert.IsNotNull(deletedTodo.DeletedAt);
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
        ClassicAssert.IsFalse(result);
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
        ClassicAssert.AreEqual(1, totalCount);
        ClassicAssert.AreEqual(1, items.Count());
        ClassicAssert.AreEqual("Completed", items.First().Title);
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
        ClassicAssert.AreEqual(1, totalCount);
        ClassicAssert.AreEqual(1, items.Count());
        ClassicAssert.AreEqual("High Priority", items.First().Title);
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
        ClassicAssert.AreEqual(3, totalCount);
        ClassicAssert.AreEqual("Apple", items.First().Title);
        ClassicAssert.AreEqual("Zebra", items.Last().Title);
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
        ClassicAssert.AreEqual(15, totalCount);
        ClassicAssert.AreEqual(5, items.Count());
        ClassicAssert.AreEqual("Todo 6", items.First().Title);
        ClassicAssert.AreEqual("Todo 10", items.Last().Title);
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
        ClassicAssert.AreEqual(1, totalCount);
        ClassicAssert.AreEqual(1, items.Count());
        ClassicAssert.AreEqual("Deleted", items.First().Title);
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
        ClassicAssert.IsNotNull(result);
        ClassicAssert.IsFalse(result!.IsDeleted);
        ClassicAssert.IsNull(result.DeletedAt);
    }
}
